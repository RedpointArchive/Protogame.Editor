using Grpc.Core;
using Protogame.Editor.Grpc.Editor;
using System.Linq;
using Srv = global::Grpc.Core.Server;
using Grpc.Core.Logging;
using System;

namespace Protogame.Editor.Server
{
    public class GrpcServer : IGrpcServer
    {
        private Srv _server;
        private string _serverUrl;
        private readonly ConsoleImpl _consoleImpl;
        private readonly IConsoleHandle _consoleHandle;
        private readonly ProjectManagerImpl _projectManagerImpl;
        private readonly PresenceImpl _presenceImpl;
        private readonly GameHosterImpl _gameHosterImpl;

        public GrpcServer(
            IConsoleHandle consoleHandle,
            ConsoleImpl consoleImpl,
            ProjectManagerImpl projectManagerImpl,
            PresenceImpl presenceImpl,
            GameHosterImpl gameHosterImpl)
        {
            _consoleHandle = consoleHandle;
            _consoleImpl = consoleImpl;
            _projectManagerImpl = projectManagerImpl;
            _presenceImpl = presenceImpl;
            _gameHosterImpl = gameHosterImpl;
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
            
            GrpcEnvironment.SetLogger(new Logger(_consoleHandle));

            _server = new Srv
            {
                Services =
                {
                    Grpc.Editor.Console.BindService(_consoleImpl),
                    Grpc.Editor.ProjectManager.BindService(_projectManagerImpl),
                    Grpc.Editor.Presence.BindService(_presenceImpl),
                    Grpc.Editor.GameHoster.BindService(_gameHosterImpl),
                },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            _server.Start();

            _serverUrl = "localhost:" + _server.Ports.Select(x => x.BoundPort).First();
        }

        private class Logger : ILogger
        {
            private readonly IConsoleHandle _consoleHandle;

            public Logger(IConsoleHandle consoleHandle)
            {
                _consoleHandle = consoleHandle;
            }

            public void Debug(string message)
            {
                _consoleHandle.LogDebug(message);
            }

            public void Debug(string format, params object[] formatArgs)
            {
                _consoleHandle.LogDebug(format, formatArgs);
            }

            public void Error(string message)
            {
                _consoleHandle.LogError(message);
            }

            public void Error(string format, params object[] formatArgs)
            {
                _consoleHandle.LogError(format, formatArgs);
            }

            public void Error(Exception exception, string message)
            {
                _consoleHandle.LogError(exception);
            }

            public ILogger ForType<T>()
            {
                return this;
            }

            public void Info(string message)
            {
                _consoleHandle.LogInfo(message);
            }

            public void Info(string format, params object[] formatArgs)
            {
                _consoleHandle.LogInfo(format, formatArgs);
            }

            public void Warning(string message)
            {
                _consoleHandle.LogWarning(message);
            }

            public void Warning(string format, params object[] formatArgs)
            {
                _consoleHandle.LogWarning(format, formatArgs);
            }

            public void Warning(Exception exception, string message)
            {
                _consoleHandle.LogWarning(exception.ToString());
            }
        }
    }
}
