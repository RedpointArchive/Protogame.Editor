using Microsoft.Xna.Framework;
using Protogame.Editor.LoadedGame;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor.EditorWindow
{
    public class GameEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly ILoadedGame _loadedGame;
        private readonly RawTextureContainer _rawTextureContainer;
        private readonly IProjectManager _projectManager;

        public GameEditorWindow(
            IAssetManager assetManager,
            ILoadedGame loadedGame,
            I2DRenderUtilities renderUtilities,
            IProjectManager projectManager)
        {
            _assetManager = assetManager;
            _loadedGame = loadedGame;
            _projectManager = projectManager;

            Title = "Game";
            Icon = _assetManager.Get<TextureAsset>("texture.IconDirectionalPad");

            _rawTextureContainer = new RawTextureContainer(renderUtilities);
            _rawTextureContainer.TextureFit = "ratio";
            SetChild(_rawTextureContainer);
        }
        
        public override bool Visible
        {
            get
            {
                return _projectManager.Project != null;
            }
            set { }
        }

        public override void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            base.Update(skinLayout, layout, gameTime, ref stealFocus);

            // Because of padding the size should be slightly smaller.
            _loadedGame.SetPositionOffset(new Point(layout.X + 1, layout.Y + 1));
            _loadedGame.SetRenderTargetSize(new Point(layout.Size.X - 2, layout.Size.Y - 2));
            _rawTextureContainer.Texture = _loadedGame.GetCurrentGameRenderTarget();
        }

        public override bool HandleEvent(ISkinLayout skinLayout, Rectangle layout, IGameContext context, Event @event)
        {
            var mouseEvent = @event as MouseEvent;
            var keyboardEvent = @event as KeyboardEvent;

            if (mouseEvent != null && layout.Contains(mouseEvent.Position))
            {
                if (mouseEvent is MousePressEvent)
                {
                    // Focus on the game to allow keyboard capture to occur.
                    this.Focus();
                }

                // Pass a copy of the mouse event to the game.
                var copyMouseEvent = mouseEvent.Clone();
                copyMouseEvent.X -= layout.X;
                copyMouseEvent.Y -= layout.Y;
                var copyMouseMoveEvent = copyMouseEvent as MouseMoveEvent;
                if (copyMouseMoveEvent != null)
                {
                    copyMouseMoveEvent.LastX -= layout.X;
                    copyMouseMoveEvent.LastY -= layout.Y;
                }

                _loadedGame.QueueEvent(copyMouseEvent);

                return true;
            }

            if (keyboardEvent != null && Focused)
            {
                var keyPressEvent = keyboardEvent as KeyPressEvent;
                if (keyPressEvent != null && keyPressEvent.Key == Microsoft.Xna.Framework.Input.Keys.OemTilde)
                {
                    // The ~ key allows you to stop keyboard and mouse capture.
                    return false;
                }
                else
                {
                    _loadedGame.QueueEvent(keyboardEvent);
                    return true;
                }
            }

            return false;
        }
    }
}
