using Grpc.Core;
using Protogame.Editor.Api.Version1;
using Protogame.Editor.Api.Version1.Core;
using Protogame.Editor.Api.Version1.ProjectManagement;
using Protogame.Editor.CommonHost;
using Protogame.Editor.Grpc.ExtensionHost;
using Protoinject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Srv = global::Grpc.Core.Server;

namespace Protogame.Editor.ExtHost
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.Error.WriteLine("ExtHost Main called with: " + args.Aggregate((a, b) => a + " " + b));

            if (args.Length > 1 && args[0] == "--running-in-app-domain")
            {
                // We are running in a shadow-copy enabled AppDomain, start the real
                // code now.
                return RealProgram.RealMain(args.Skip(1).ToArray());
            }
            else
            {
                // We aren't yet running with shadow copy enabled.  Create a new
                // AppDomain with shadow copy on and run the extension host inside
                // it.  This allows extensions to be rebuilt while the extension is
                // still running.
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
            Console.Error.WriteLine("ExtHost RealMain called with: " + args.Aggregate((a, b) => a + " " + b));
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
                                Console.Error.WriteLine("Parent process " + pid + " has exited, closing extension host process.");
                                Environment.Exit(0);
                            };
                            process.EnableRaisingEvents = true;
                            if (process.HasExited)
                            {
                                Console.Error.WriteLine("Parent process " + pid + " has exited, closing extension host process.");
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
            
            System.Console.Error.WriteLine("Starting extension host...");
            System.Console.Error.WriteLine("Editor URL is: {0}", editorUrl);
            System.Console.Error.WriteLine("Assembly path is: {0}", assemblyFile);

            System.Console.Error.WriteLine("Loading assembly from: {0}", assemblyFile);
            var assembly = Assembly.LoadFrom(assemblyFile);

            // Find the attributes that describe the extensions provided by this assembly.
            var editorExtensions = new List<IEditorExtension>();
            foreach (var attr in assembly.GetCustomAttributes<ExtensionAttribute>())
            {
                try
                {
                    editorExtensions.Add((IEditorExtension)Activator.CreateInstance(attr.Type));
                }
                catch (Exception e)
                {
                    System.Console.Error.WriteLine(e);
                }
            }

            System.Console.Error.WriteLine("Configuring kernel...");

            var kernel = new StandardKernel();
            kernel.Bind<IEditorClientProvider>().To<EditorClientProvider>().InSingletonScope();
            kernel.Bind<IProjectManager>().To<ProjectManager>().InSingletonScope();
            kernel.Bind<IWantsUpdateSignal>().To<ProjectManagerUpdateSignal>().InSingletonScope();
            kernel.Bind<IWantsUpdateSignal>().To<PresenceCheckerUpdateSignal>().InSingletonScope();
            kernel.Bind<Editor.Api.Version1.Core.IConsoleHandle>().To<ConsoleHandle>().InSingletonScope();
            foreach (var ext in editorExtensions)
            {
                ext.RegisterServices(kernel);
            }

            System.Console.Error.WriteLine("Configuring editor client provider with URL: {0}", editorUrl);
            var editorClientProvider = kernel.Get<IEditorClientProvider>();
            editorClientProvider.CreateChannel(editorUrl);

            System.Console.Error.WriteLine("Configuring gRPC logging...");
            GrpcEnvironment.SetLogger(new GrpcLogger());

            System.Console.Error.WriteLine("Creating gRPC server...");
            var server = new Srv
            {
                Services =
                {
                    MenuEntries.BindService(kernel.Get<MenuEntriesImpl>())
                },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            server.Start();

            var serverUrl = "localhost:" + server.Ports.Select(x => x.BoundPort).First();
            System.Console.Error.WriteLine("gRPC server started on {0}", serverUrl);
            
            Console.WriteLine(serverUrl);
            Console.Error.WriteLine(serverUrl);

            var wantsUpdateSignal = kernel.GetAll<IWantsUpdateSignal>();

            while (true)
            {
                if (wantsUpdateSignal != null)
                {
                    foreach (var s in wantsUpdateSignal)
                    {
                        s.Update();
                    }
                }

                Thread.Sleep(16);
            }

            return 0;
        }
    }
}
