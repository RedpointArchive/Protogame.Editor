using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Protogame.Editor.Grpc.Editor;

namespace Protogame.Editor.Server
{
    public class ConsoleImpl : Protogame.Editor.Grpc.Editor.Console.ConsoleBase
    {
        private readonly IConsoleHandle _consoleHandle;
        private readonly LogResponse _logResponse = new LogResponse();

        public ConsoleImpl(
            IConsoleHandle consoleHandle)
        {
            _consoleHandle = consoleHandle;
        }

        public override Task<LogResponse> LogDebug(LogRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                _consoleHandle.LogDebug(request.Message);
                return Task.FromResult(_logResponse);
            });
        }

        public override Task<LogResponse> LogInfo(LogRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                _consoleHandle.LogInfo(request.Message);
                return Task.FromResult(_logResponse);
            });
        }

        public override Task<LogResponse> LogWarn(LogRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                _consoleHandle.LogWarning(request.Message);
                return Task.FromResult(_logResponse);
            });
        }

        public override Task<LogResponse> LogError(LogRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                _consoleHandle.LogError(request.Message);
                return Task.FromResult(_logResponse);
            });
        }
    }
}
