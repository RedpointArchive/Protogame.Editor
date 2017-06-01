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
    }
}
