using Grpc.Core;
using Protogame.Editor.Grpc.ExtensionHost;
using System.Linq;
using Srv = global::Grpc.Core.Server;
using System;
using Grpc.Core.Logging;
using Protoinject;
using System.Threading.Tasks;

namespace Protogame.Editor.ExtHost
{
    public class GrpcServer : IGrpcServer
    {
        private readonly ExtensionHostServerImpl _extensionHostServerImpl;

        private Srv _server;
        private Srv _runtimeServer;
        private string _serverUrl;
        private string _runtimeServerUrl;

        public GrpcServer(
            ExtensionHostServerImpl extensionHostServerImpl)
        {
            _extensionHostServerImpl = extensionHostServerImpl;
        }

        public string GetServerUrl()
        {
            StartServerIfNotStarted();

            return _serverUrl;
        }

        private void StartServerIfNotStarted()
        {
            if (_server != null)
            {
                return;
            }

            GrpcEnvironment.SetLogger(new Logger());

            _server = new Srv
            {
                Services =
                {
                    ExtensionHostServer.BindService(_extensionHostServerImpl)
                },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            _server.Start();

            _serverUrl = "localhost:" + _server.Ports.Select(x => x.BoundPort).First();
        }

        public async Task<string> StartAndGetRuntimeServerUrlAsync(IKernel localKernel)
        {
            if (_runtimeServer != null)
            {
                return _runtimeServerUrl;
            }

            _runtimeServer = new Srv
            {
                Services =
                {
                    MenuEntries.BindService(await localKernel.GetAsync<MenuEntriesImpl>())
                },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            _runtimeServer.Start();

            _runtimeServerUrl = "localhost:" + _server.Ports.Select(x => x.BoundPort).First();
            return _runtimeServerUrl;
        }

        private class Logger : ILogger
        {
            public void Debug(string message)
            {
                System.Console.Error.WriteLine("GRPC DEBUG: " + message);
            }

            public void Debug(string format, params object[] formatArgs)
            {
                System.Console.Error.WriteLine("GRPC DEBUG: " + string.Format(format, formatArgs));
            }

            public void Error(string message)
            {
                System.Console.Error.WriteLine("GRPC ERROR: " + message);
            }

            public void Error(string format, params object[] formatArgs)
            {
                System.Console.Error.WriteLine("GRPC ERROR: " + string.Format(format, formatArgs));
            }

            public void Error(Exception exception, string message)
            {
                System.Console.Error.WriteLine("GRPC ERROR: " + exception.ToString());
            }

            public ILogger ForType<T>()
            {
                return this;
            }

            public void Info(string message)
            {
                System.Console.Error.WriteLine("GRPC INFO: " + message);
            }

            public void Info(string format, params object[] formatArgs)
            {
                System.Console.Error.WriteLine("GRPC INFO: " + string.Format(format, formatArgs));
            }

            public void Warning(string message)
            {
                System.Console.Error.WriteLine("GRPC WARNING: " + message);
            }

            public void Warning(string format, params object[] formatArgs)
            {
                System.Console.Error.WriteLine("GRPC WARNING: " + string.Format(format, formatArgs));
            }

            public void Warning(Exception exception, string message)
            {
                System.Console.Error.WriteLine("GRPC WARNING: " + exception.ToString());
            }
        }
    }
}
