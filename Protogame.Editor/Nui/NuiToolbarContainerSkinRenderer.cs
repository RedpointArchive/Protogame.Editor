using Microsoft.Xna.Framework;
using Protogame.Editor.Layout;
using System;

namespace Protogame.Editor.Nui
{
    public class NuiToolbarContainerSkinRenderer : ISkinRenderer<ToolbarContainer>
    {
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly NuiRenderer _nuiRenderer;

        public NuiToolbarContainerSkinRenderer(
            I2DRenderUtilities renderUtilities,
            NuiRenderer nuiRenderer)
        {
            _renderUtilities = renderUtilities;
            _nuiRenderer = nuiRenderer;
        }

        public void Render(IRenderContext renderContext, Rectangle layout, ToolbarContainer container)
        {
            _nuiRenderer.RenderToolbar(renderContext, new Rectangle(layout.X, layout.Y, layout.Width, 18));
            _renderUtilities.RenderLine(renderContext, new Vector2(layout.X, layout.Y + 17), new Vector2(layout.Right - 1, layout.Y + 17), new Color(0, 0, 0, 72));
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, ToolbarContainer container)
        {
            throw new NotSupportedException();
        }
    }
}
