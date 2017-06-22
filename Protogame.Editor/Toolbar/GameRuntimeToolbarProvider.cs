using Protogame.Editor.EditorWindow;
using Protogame.Editor.LoadedGame;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor.Toolbar
{
    public class GameRuntimeToolbarProvider : IToolbarProvider
    {
        private readonly IProjectManager _projectManager;
        private readonly ILoadedGame _loadedGame;
        private readonly IWindowManagement _windowManagement;

        public GameRuntimeToolbarProvider(
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

            var playToggled = state == LoadedGameState.Playing || state == LoadedGameState.Paused;
            var pausedToggled = state == LoadedGameState.Paused;

            var playEnabled = _projectManager.Project != null && state != LoadedGameState.Loading;
            var pauseEnabled = state == LoadedGameState.Playing || state == LoadedGameState.Paused;
            var stopEnabled = state == LoadedGameState.Playing || state == LoadedGameState.Paused;

            return new[]
            {
                new GenericToolbarEntry("_play".GetHashCode(), "texture.IconPlay", playToggled, playEnabled, HandlePlay, null),
                new GenericToolbarEntry("_pause".GetHashCode(), "texture.IconPause", pausedToggled, pauseEnabled, HandlePause, null),
                new GenericToolbarEntry("_stop".GetHashCode(), "texture.IconStop", false, stopEnabled, HandleStop, null),
            };
        }

        private void HandlePlay(GenericToolbarEntry toolbarEntry)
        {
            _loadedGame.SetPlaybackMode(true);
            _windowManagement.ActivateWhere(x => x is GameEditorWindow);
        }

        private void HandlePause(GenericToolbarEntry toolbarEntry)
        {
            if (_loadedGame.GetPlaybackState() == LoadedGameState.Playing)
            {
                _loadedGame.SetPlaybackMode(false);
            }
            else if (_loadedGame.GetPlaybackState() == LoadedGameState.Paused)
            {
                _loadedGame.SetPlaybackMode(true);
            }
        }

        private void HandleStop(GenericToolbarEntry toolbarEntry)
        {
            if (_loadedGame.GetPlaybackState() == LoadedGameState.Playing ||
                _loadedGame.GetPlaybackState() == LoadedGameState.Paused)
            {
                _loadedGame.RequestRestart();
                _windowManagement.ActivateWhere(x => x is WorldEditorWindow);
            }
        }
    }
}
