using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Protogame.Editor.GameHost
{
    public class BackBufferDimensionsProxy : IBackBufferDimensions
    {
        private readonly IBackBufferDimensions _marshalledBackBufferDimensions;

        public BackBufferDimensionsProxy(IBackBufferDimensions marshalledBackBufferDimensions)
        {
            _marshalledBackBufferDimensions = marshalledBackBufferDimensions;
        }

        public BackBufferSize GetSize(GraphicsDevice graphicsDevice)
        {
            // GraphicsDevice can not be marshalled across an AppDomain boundary, so
            // we pass null instead.
            return _marshalledBackBufferDimensions.GetSize(null);
        }
    }
}
