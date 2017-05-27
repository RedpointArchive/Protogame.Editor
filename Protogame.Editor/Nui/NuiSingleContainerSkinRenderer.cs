using System;
using Microsoft.Xna.Framework;
using Protogame;

namespace Protogame.Editor.Nui
{
    public class NuiSingleContainerSkinRenderer : ISkinRenderer<SingleContainer>
    {
        private readonly I2DRenderUtilities _renderUtilities;

        public NuiSingleContainerSkinRenderer(I2DRenderUtilities renderUtilities)
        {
            _renderUtilities = renderUtilities;
        }

        public void Render(IRenderContext renderContext, Rectangle layout, SingleContainer container)
        {
            _renderUtilities.RenderRectangle(renderContext, layout, new Color(194, 194, 194, 255), true);
            _renderUtilities.RenderRectangle(renderContext, new Rectangle(layout.X, layout.Y, layout.Width - 1, layout.Height - 1), new Color(0, 0, 0, 72), false);
            _renderUtilities.RenderRectangle(renderContext, new Rectangle(layout.X + 1, layout.Y + 1, layout.Width - 3, layout.Height - 3), new Color(23, 23, 23, 12), false);
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, SingleContainer container)
        {
            throw new NotSupportedException();
        }
    }
}
