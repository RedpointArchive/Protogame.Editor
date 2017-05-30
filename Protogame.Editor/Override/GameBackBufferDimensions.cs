using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Protogame.Editor.LoadedGame;
using System;

namespace Protogame.Editor.Override
{
    public class GameBackBufferDimensions : MarshalByRefObject, IBackBufferDimensions
    {
        private readonly ILoadedGame _loadedGame;

        public GameBackBufferDimensions(ILoadedGame loadedGame)
        {
            _loadedGame = loadedGame;
        }

        public Point GetSize(GraphicsDevice graphicsDevice)
        {
            var size = _loadedGame.GetRenderTargetSize();

            if (size == null)
            {
                return new Point(
                    graphicsDevice.PresentationParameters.BackBufferWidth,
                    graphicsDevice.PresentationParameters.BackBufferHeight);
            }

            return size.Value;
        }
    }
}
