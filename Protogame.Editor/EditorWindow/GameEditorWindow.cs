using Microsoft.Xna.Framework;
using Protogame.Editor.LoadedGame;

namespace Protogame.Editor.EditorWindow
{
    public class GameEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly ILoadedGame _loadedGame;
        private readonly RawTextureContainer _rawTextureContainer;

        public GameEditorWindow(
            IAssetManager assetManager,
            ILoadedGame loadedGame,
            I2DRenderUtilities renderUtilities)
        {
            _assetManager = assetManager;
            _loadedGame = loadedGame;

            Title = "Game";
            Icon = _assetManager.Get<TextureAsset>("texture.IconDirectionalPad");

            _rawTextureContainer = new RawTextureContainer(renderUtilities);
            _rawTextureContainer.TextureFit = "ratio";
            SetChild(_rawTextureContainer);
        }

        public override void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            base.Update(skinLayout, layout, gameTime, ref stealFocus);

            // Because of padding the size should be slightly smaller.
            _loadedGame.SetRenderTargetSize(new Point(layout.Size.X - 2, layout.Size.Y - 2));
            _rawTextureContainer.Texture = _loadedGame.GetGameRenderTarget();
        }

        public override bool HandleEvent(ISkinLayout skinLayout, Rectangle layout, IGameContext context, Event @event)
        {
            var mouseEvent = @event as MouseEvent;

            if (layout.Contains(mouseEvent.Position))
            {
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

            return false;
        }
    }
}
