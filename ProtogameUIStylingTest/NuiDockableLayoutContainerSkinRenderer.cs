using Protogame;
using Microsoft.Xna.Framework;
using System;

namespace ProtogameUIStylingTest
{
    public class NuiDockableLayoutContainerSkinRenderer : ISkinRenderer<DockableLayoutContainer>
    {
        private readonly NuiRenderer _nuiRenderer;
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly IAssetReference<FontAsset> _fontAsset;

        public NuiDockableLayoutContainerSkinRenderer(NuiRenderer nuiRenderer, I2DRenderUtilities renderUtilities, IAssetManager assetManager)
        {
            _nuiRenderer = nuiRenderer;
            _renderUtilities = renderUtilities;
            _fontAsset = assetManager.Get<FontAsset>("font.UISmall");
        }

        public void Render(IRenderContext renderContext, Rectangle layout, DockableLayoutContainer container)
        {
            foreach (var tabForRendering in container.TabWithLayouts(layout))
            {
                if (tabForRendering.IsActive)
                {
                    _nuiRenderer.RenderTab(renderContext, tabForRendering.Layout);
                }

                _renderUtilities.RenderTexture(
                    renderContext,
                    new Vector2(tabForRendering.Layout.X + 3, tabForRendering.Layout.Y + 3),
                    tabForRendering.Icon,
                    new Vector2(tabForRendering.Layout.Height - 6, tabForRendering.Layout.Height - 6));

                _renderUtilities.RenderText(
                    renderContext,
                    new Vector2(tabForRendering.Layout.X + 16, tabForRendering.Layout.Bottom - 1),
                    tabForRendering.Title,
                    _fontAsset,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Bottom,
                    textColor: Color.Black,
                    renderShadow: false);
            }
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, DockableLayoutContainer container)
        {
            throw new NotSupportedException();
        }
    }
}
