using System;

namespace Protogame.Editor.Extension
{
    public class ExtensionConsoleHandle : MarshalByRefObject, Protogame.Editor.Api.Version1.Core.IConsoleHandle
    {
        private readonly IConsoleHandle _consoleHandle;

        public ExtensionConsoleHandle(IConsoleHandle consoleHandle)
        {
            _consoleHandle = consoleHandle;
        }

        public void LogDebug(string messageFormat)
        {
            _consoleHandle.LogDebug(messageFormat);
        }

        public void LogDebug(string messageFormat, params object[] objects)
        {
            _consoleHandle.LogDebug(messageFormat, objects);
        }

        public void LogError(string messageFormat)
        {
            _consoleHandle.LogError(messageFormat);
        }

        public void LogError(string messageFormat, params object[] objects)
        {
            _consoleHandle.LogError(messageFormat);
        }

        public void LogError(Exception exception)
        {
            _consoleHandle.LogError(exception);
        }

        public void LogInfo(string messageFormat)
        {
            _consoleHandle.LogInfo(messageFormat);
        }

        public void LogInfo(string messageFormat, params object[] objects)
        {
            _consoleHandle.LogInfo(messageFormat);
        }

        public void LogWarning(string messageFormat)
        {
            _consoleHandle.LogWarning(messageFormat);
        }

        public void LogWarning(string messageFormat, params object[] objects)
        {
            _consoleHandle.LogWarning(messageFormat);
        }
    }
}
