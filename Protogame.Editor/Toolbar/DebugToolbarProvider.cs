using System;
using Protogame.Editor.LoadedGame;
using Protogame.Editor.ProjectManagement;
using Protogame.Editor.EditorWindow;

namespace Protogame.Editor.Toolbar
{
    public class DebugToolbarProvider : IToolbarProvider
    {
        private readonly IProjectManager _projectManager;
        private readonly ILoadedGame _loadedGame;
        private readonly IWindowManagement _windowManagement;

        public DebugToolbarProvider(
            IProjectManager projectManager,
            ILoadedGame loadedGame,
            IWindowManagement windowManagement)
        {
            _projectManager = projectManager;
            _loadedGame = loadedGame;
            _windowManagement = windowManagement;
        }

        public GenericToolbarEntry[] GetToolbarItems()
        {
            var state = _loadedGame.GetPlaybackState();

            return new[]
            {
                new GenericToolbarEntry("_debug".GetHashCode(), "texture.IconDebug", false, _projectManager.Project != null && state == LoadedGameState.Loaded, LaunchDebug, null),
                new GenericToolbarEntry("_debuggpu".GetHashCode(), "texture.IconDebugGpu", false, _projectManager.Project != null && state == LoadedGameState.Loaded, LaunchDebugGpu, null),
            };
        }

        private void LaunchDebug(GenericToolbarEntry toolbarEntry)
        {
            _loadedGame.RunInDebug();
            _windowManagement.ActivateWhere(x => x is GameEditorWindow);
        }

        private void LaunchDebugGpu(GenericToolbarEntry toolbarEntry)
        {
            _loadedGame.RunInDebugGpu();
            _windowManagement.ActivateWhere(x => x is GameEditorWindow);
        }
    }
}
