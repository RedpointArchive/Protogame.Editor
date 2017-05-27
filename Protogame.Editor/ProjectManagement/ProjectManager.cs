using System;

namespace Protogame.Editor.ProjectManagement
{
    public class ProjectManager : IProjectManager
    {
        public ProjectManager(
            IRawLaunchArguments launchArguments,
            ICoroutine coroutine)
        {
            var arguments = launchArguments.Arguments;
            var directoryIndex = Array.IndexOf(arguments, "--project");
            if (directoryIndex == -1 || directoryIndex == arguments.Length - 1)
            {
                throw new InvalidOperationException("No project defined for startup!  You must launch the Protogame Editor from the workbench, or pass --project <dir> on the command-line.");
            }
        }

        public bool IsLoadingProject { get; private set; }

        public bool HasProject => Project != null;

        public Project Project { get; }
    }
}
