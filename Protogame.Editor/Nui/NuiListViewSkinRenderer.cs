using System;
using Microsoft.Xna.Framework;
using Protogame;

namespace Protogame.Editor.Nui
{
    public class NuiListViewSkinRenderer : ISkinRenderer<ListView>
    {
        private readonly I2DRenderUtilities _renderUtilities;

        public NuiListViewSkinRenderer(I2DRenderUtilities renderUtilities)
        {
            _renderUtilities = renderUtilities;
        }

        public void Render(IRenderContext renderContext, Rectangle layout, ListView container)
        {
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, ListView container)
        {
            throw new NotSupportedException();
        }
    }
}
