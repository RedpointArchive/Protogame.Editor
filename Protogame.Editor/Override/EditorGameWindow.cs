using System;
using Microsoft.Xna.Framework;
using Protogame.Editor.LoadedGame;

namespace Protogame.Editor.Override
{
    public class EditorGameWindow : IGameWindow
    {
        private readonly ILoadedGame _loadedGame;
        private readonly GameWindow _gameWindow;

        public EditorGameWindow(
            ILoadedGame loadedGame,
            GameWindow gameWindow)
        {
            _loadedGame = loadedGame;
            _gameWindow = gameWindow;

            Title = "";
        }

        public Rectangle ClientBounds
        {
            get
            {
                var size = _loadedGame.GetRenderTargetSize();
                return new Rectangle(
                    0,
                    0,
                    size.HasValue ? size.Value.X : 640,
                    size.HasValue ? size.Value.Y : 480);
            }
        }

        public string Title { get; set; }

        public bool AllowUserResizing
        {
            get
            {
                // The editor always allows resizing of the game window.
                return true;
            }
            set
            {
            }
        }

        public GameWindow PlatformWindow => _gameWindow;

        public void Maximize()
        {
        }

        public void Minimize()
        {
        }

        public void Restore()
        {
        }
    }
}
