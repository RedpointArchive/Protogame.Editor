using System;
using Microsoft.Xna.Framework;

namespace Protogame.Editor.GameHost
{
    public class EditorGameWindow : IGameWindow
    {
        private readonly EditorHostGame _hostGame;

        public EditorGameWindow(EditorHostGame hostGame)
        {
            _hostGame = hostGame;
        }

        public Rectangle ClientBounds { get; internal set; }

        public string Title { get; set; }

        public bool AllowUserResizing
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public bool IsFullscreen => false;

        public void Maximize()
        {
        }

        public void Minimize()
        {
        }

        public void Resize(int width, int height)
        {
        }

        public void Restore()
        {
        }

        public void SetFullscreen(bool fullscreen)
        {
        }

        public void SetCursorPosition(int x, int y)
        {
            _hostGame.SetMousePositionToSet(x, y);
        }

        public void GetCursorPosition(out int x, out int y)
        {
            _hostGame.GetMousePosition(out x, out y);
        }
    }
}
