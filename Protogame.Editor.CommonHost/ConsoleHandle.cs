using Protogame.Editor.Api.Version1.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Protogame.Editor.Grpc.Editor.Console;
using Protogame.Editor.Grpc.Editor;

namespace Protogame.Editor.CommonHost
{
    public class ConsoleHandle : Editor.Api.Version1.Core.IConsoleHandle
    {
        private readonly ConsoleClient _consoleHandle;

        public ConsoleHandle(IEditorClientProvider editorClientProvider)
        {
            _consoleHandle = editorClientProvider.GetClient<ConsoleClient>();
        }

        public void LogDebug(string messageFormat)
        {
            Task.Run(async() => await _consoleHandle.LogDebugAsync(new LogRequest
            {
                Message = messageFormat
            }));
        }

        public void LogDebug(string messageFormat, params object[] objects)
        {
            Task.Run(async () => await _consoleHandle.LogDebugAsync(new LogRequest
            {
                Message = string.Format(messageFormat, objects)
            }));
        }

        public void LogError(string messageFormat)
        {
            Task.Run(async () => await _consoleHandle.LogErrorAsync(new LogRequest
            {
                Message = messageFormat
            }));
        }

        public void LogError(string messageFormat, params object[] objects)
        {
            Task.Run(async () => await _consoleHandle.LogErrorAsync(new LogRequest
            {
                Message = string.Format(messageFormat, objects)
            }));
        }

        public void LogError(Exception exception)
        {
            Task.Run(async () => await _consoleHandle.LogErrorAsync(new LogRequest
            {
                Message = exception.Message + Environment.NewLine + exception.StackTrace
            }));
        }

        public void LogInfo(string messageFormat)
        {
            Task.Run(async () => await _consoleHandle.LogInfoAsync(new LogRequest
            {
                Message = messageFormat
            }));
        }

        public void LogInfo(string messageFormat, params object[] objects)
        {
            Task.Run(async () => await _consoleHandle.LogInfoAsync(new LogRequest
            {
                Message = string.Format(messageFormat, objects)
            }));
        }

        public void LogWarning(string messageFormat)
        {
            Task.Run(async () => await _consoleHandle.LogWarnAsync(new LogRequest
            {
                Message = messageFormat
            }));
        }

        public void LogWarning(string messageFormat, params object[] objects)
        {
            Task.Run(async () => await _consoleHandle.LogWarnAsync(new LogRequest
            {
                Message = string.Format(messageFormat, objects)
            }));
        }
    }
}
