using Protogame.Editor.Api.Version1;
using Protoinject;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace Protogame.Editor.ExtHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            int? trackProcessId = null;
            if (args.Length == 2)
            {
                if (args[0] == "--track")
                {
                    var process = Process.GetProcessById(int.Parse(args[1]));
                    process.Exited += (sender, e) =>
                    {
                        Console.WriteLine("Parent process " + args[1] + " has exited, closing extension host process.");
                        Environment.Exit(0);
                    };
                    process.EnableRaisingEvents = true;
                }
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.Error.WriteLine(e.ExceptionObject);
            };
            
            var hostKernel = new StandardKernel();
            hostKernel.Bind<ExtensionHostServerImpl>().To<ExtensionHostServerImpl>().InSingletonScope();
            hostKernel.Bind<IGrpcServer>().To<GrpcServer>().InSingletonScope();
            hostKernel.Bind<ExtensionHost>().To<ExtensionHost>().InSingletonScope();
            hostKernel.Bind<IEditorClientProvider>().To<EditorClientProvider>().InSingletonScope();

            var grpcServer = hostKernel.Get<IGrpcServer>();
            var hostUri = grpcServer.GetServerUrl();
            var extensionHost = hostKernel.Get<ExtensionHost>();
            
            Console.WriteLine(hostUri);
            Console.Error.WriteLine(hostUri);

            while (extensionHost.Running)
            {
                // TODO: Do a proper tick loop.

                extensionHost.Update();

                Thread.Sleep(16);
            }
        }
    }
}
