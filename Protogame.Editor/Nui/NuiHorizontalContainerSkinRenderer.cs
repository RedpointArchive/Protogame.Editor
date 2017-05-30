using System;
using Microsoft.Xna.Framework;
using Protogame;
using System.Linq;

namespace Protogame.Editor.Nui
{
    public class NuiHorizontalContainerSkinRenderer : ISkinRenderer<HorizontalContainer>
    {
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly IAssetManager _assetManager;
        private readonly ISkinLayout _skinLayout;

        public NuiHorizontalContainerSkinRenderer(
            I2DRenderUtilities renderUtilities,
            IAssetManager assetManager,
            ISkinLayout skinLayout)
        {
            _renderUtilities = renderUtilities;
            _skinLayout = skinLayout;

            _assetManager = assetManager;
        }

        public void Render(IRenderContext renderContext, Rectangle layout, HorizontalContainer horizontalContainer)
        {
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, HorizontalContainer horizontalContainer)
        {
            throw new NotSupportedException();
        }
    }
}
