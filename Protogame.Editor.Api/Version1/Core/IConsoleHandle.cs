using System;

namespace Protogame.Editor.Api.Version1.Core
{
    public interface IConsoleHandle
    {
        void LogDebug(string messageFormat);

        void LogDebug(string messageFormat, params object[] objects);

        void LogInfo(string messageFormat);

        void LogInfo(string messageFormat, params object[] objects);

        void LogWarning(string messageFormat);

        void LogWarning(string messageFormat, params object[] objects);

        void LogError(string messageFormat);

        void LogError(string messageFormat, params object[] objects);

        void LogError(Exception exception);
    }
}
