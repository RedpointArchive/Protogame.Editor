using Grpc.Core;
using Protogame.Editor.Api.Version1;
using Protogame.Editor.Api.Version1.Core;
using Protogame.Editor.Api.Version1.ProjectManagement;
using Protogame.Editor.CommonHost;
using Protogame.Editor.CommonHost.SharedRendering;
using Protogame.Editor.Grpc.ExtensionHost;
using Protogame.Editor.Grpc.GameHost;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Srv = global::Grpc.Core.Server;

namespace Protogame.Editor.GameHost
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.Error.WriteLine("GameHost Main called with: " + args.Aggregate((a, b) => a + " " + b));

            if (args.Length > 1 && args[0] == "--running-in-app-domain")
            {
                // We are running in a shadow-copy enabled AppDomain, start the real
                // code now.
                return RealProgram.RealMain(args.Skip(1).ToArray());
            }
            else
            {
                // We aren't yet running with shadow copy enabled.  Create a new
                // AppDomain with shadow copy on and run the game host inside
                // it.  This allows the game to be rebuilt while the game is
                // still loaded.
                var appDomain = AppDomain.CreateDomain(
                    "Real Application Domain",
                    null,
                    AppDomain.CurrentDomain.BaseDirectory,
                    AppDomain.CurrentDomain.RelativeSearchPath,
                    true);
                return appDomain.ExecuteAssembly(Assembly.GetEntryAssembly().Location, new[] { "--running-in-app-domain" }.Concat(args).ToArray());
            }
        }
    }

    public static class RealProgram
    {
        public static int RealMain(string[] args)
        {
            int? trackProcessId = null;
            Console.Error.WriteLine("GameHost RealMain called with: " + args.Aggregate((a, b) => a + " " + b));
            var argsList = new Queue<string>(args);
            string editorUrl = null;
            string assemblyFile = null;
            while (argsList.Count > 0)
            {
                var arg = argsList.Dequeue();
                switch (arg)
                {
                    case "--trace":
                        {
                            var pid = argsList.Dequeue();
                            var process = Process.GetProcessById(int.Parse(pid));
                            process.Exited += (sender, e) =>
                            {
                                Console.Error.WriteLine("Parent process " + pid + " has exited, closing game host process.");
                                Environment.Exit(0);
                            };
                            process.EnableRaisingEvents = true;
                            if (process.HasExited)
                            {
                                Console.Error.WriteLine("Parent process " + pid + " has exited, closing game host process.");
                                Environment.Exit(0);
                            }
                            break;
                        }
                    case "--debug":
                        Debugger.Launch();
                        break;
                    case "--editor-url":
                        editorUrl = argsList.Dequeue();
                        break;
                    case "--assembly-path":
                        assemblyFile = argsList.Dequeue();
                        break;
                }
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.Error.WriteLine(e.ExceptionObject);
            };

            if (editorUrl == null || assemblyFile == null)
            {
                System.Console.Error.WriteLine("Editor URL or assembly file not specified");
                return 1;
            }

            System.Console.Error.WriteLine("Starting game host...");
            System.Console.Error.WriteLine("Editor URL is: {0}", editorUrl);
            System.Console.Error.WriteLine("Assembly path is: {0}", assemblyFile);
            
            System.Console.Error.WriteLine("Configuring kernel...");

            var kernel = new StandardKernel();
            kernel.Bind<IEditorClientProvider>().To<EditorClientProvider>().InSingletonScope();
            kernel.Bind<IProjectManager>().To<ProjectManager>().InSingletonScope();
            kernel.Bind<IWantsUpdateSignal>().To<ProjectManagerUpdateSignal>().InSingletonScope();
            kernel.Bind<IWantsUpdateSignal>().To<PresenceCheckerUpdateSignal>().InSingletonScope();
            kernel.Bind<Api.Version1.Core.IConsoleHandle>().To<ConsoleHandle>().InSingletonScope();
            kernel.Bind<IGameRunner>().To<HostedGameRunner>().InSingletonScope();
            kernel.Bind<HostedEventEngineHook>().To<HostedEventEngineHook>().InSingletonScope();
            kernel.Bind<ISharedRendererClientFactory>().ToFactory();

            System.Console.Error.WriteLine("Configuring editor client provider with URL: {0}", editorUrl);
            var editorClientProvider = kernel.Get<IEditorClientProvider>();
            editorClientProvider.CreateChannel(editorUrl);

            // Load the target assembly.
            System.Console.Error.WriteLine("Loading game assembly from " + assemblyFile + "...");
            var assembly = Assembly.LoadFrom(assemblyFile);

            System.Console.Error.WriteLine("Constructing standard kernel...");
            kernel.Bind<IRawLaunchArguments>()
                .ToMethod(x => new DefaultRawLaunchArguments(new string[0]))
                .InSingletonScope();

            // Bind our extension hook first so that it runs before everything else.
            kernel.Bind<IEngineHook>().To<ExtensionEngineHook>().InSingletonScope();

            Func<System.Reflection.Assembly, Type[]> TryGetTypes = a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return new Type[0];
                }
            };

            System.Console.Error.WriteLine("Finding configuration classes in " + assemblyFile + "...");
            var typeSource = new List<Type>();
            foreach (var attribute in assembly.GetCustomAttributes(false))
            {
                if (attribute.GetType().FullName == "Protogame.ConfigurationAttribute")
                {
                    typeSource.Add(((ConfigurationAttribute)attribute).GameConfigurationOrServerClass);
                }
            }

            if (typeSource.Count == 0)
            {
                // Scan all types to find implementors of IGameConfiguration
                typeSource.AddRange(from type in TryGetTypes(assembly)
                                    select type);
            }

            System.Console.Error.WriteLine("Found {0} configuration classes in " + assemblyFile, typeSource.Count);

            System.Console.Error.WriteLine("Constructing game configurations...");
            var gameConfigurations = new List<IGameConfiguration>();
            foreach (var type in typeSource)
            {
                if (typeof(IGameConfiguration).IsAssignableFrom(type) &&
                    !type.IsInterface && !type.IsAbstract)
                {
                    gameConfigurations.Add(Activator.CreateInstance(type) as IGameConfiguration);
                }
            }

            ICoreGame game = null;
            var hasBoundNewEventEngine = false;

            System.Console.Error.WriteLine("Configuring kernel and constructing game instance ({0} configurations)...", gameConfigurations.Count);
            foreach (var configuration in gameConfigurations)
            {
                System.Console.Error.WriteLine("Configuring with {0}...", configuration.GetType().FullName);

                configuration.ConfigureKernel(kernel);

                // Rebind services so the game renders correctly inside the editor.
                kernel.Rebind<IBaseDirectory>().To<HostedBaseDirectory>().InSingletonScope();
                kernel.Rebind<IBackBufferDimensions>().To<HostedBackBufferDimensions>().InSingletonScope();
                kernel.Rebind<IDebugRenderer>().To<DefaultDebugRenderer>().InSingletonScope();
                var bindings = kernel.GetCopyOfBindings();
                var mustBindNewEventEngine = false;
                if (bindings.ContainsKey(typeof(IEngineHook)))
                {
                    if (bindings[typeof(IEngineHook)].Any(x => x.Target == typeof(EventEngineHook)))
                    {
                        mustBindNewEventEngine = !hasBoundNewEventEngine;
                        kernel.UnbindSpecific<IEngineHook>(x => x.Target == typeof(EventEngineHook));
                    }

                    if (mustBindNewEventEngine)
                    {
                        kernel.Bind<IEngineHook>().ToMethod(ctx =>
                        {
                            return ctx.Kernel.Get<HostedEventEngineHook>(ctx.Parent);
                        }).InSingletonScope();
                    }
                }

                if (game == null)
                {
                    game = configuration.ConstructGame(kernel);
                }
            }

            if (game != null)
            {
                System.Console.Error.WriteLine("Game instance is {0}", game.GetType().FullName);
            }

            var runner = kernel.Get<IGameRunner>(new NamedConstructorArgument("game", game));

            System.Console.Error.WriteLine("Configuring gRPC logging...");
            GrpcEnvironment.SetLogger(new GrpcLogger());

            System.Console.Error.WriteLine("Creating gRPC server...");
            var server = new Srv
            {
                Services =
                {
                    GameHostServer.BindService(kernel.Get<GameHostServerImpl>())
                },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            server.Start();

            var serverUrl = "localhost:" + server.Ports.Select(x => x.BoundPort).First();
            System.Console.Error.WriteLine("gRPC server started on {0}", serverUrl);

            Console.WriteLine(serverUrl);
            Console.Error.WriteLine(serverUrl);

            System.Console.Error.WriteLine("LoadFromPath complete");

            runner.Run();

            return 0;
        }
    }
}
