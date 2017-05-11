using System;
using Microsoft.Xna.Framework;
using Protogame;

namespace ProtogameUIStylingTest
{
    public class NuiButtonSkinRenderer : ISkinRenderer<Button>
    {
        private readonly NuiRenderer _nuiRenderer;
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly IAssetReference<FontAsset> _fontAsset;

        public NuiButtonSkinRenderer(NuiRenderer nuiRenderer, I2DRenderUtilities renderUtilities, IAssetManager assetManager)
        {
            _nuiRenderer = nuiRenderer;
            _renderUtilities = renderUtilities;
            _fontAsset = assetManager.Get<FontAsset>("font.UISmall");
        }

        public void Render(IRenderContext renderContext, Rectangle layout, Button button)
        {
            if (button.State == ButtonUIState.Clicked)
            {
                _nuiRenderer.RenderPressedButton(renderContext, layout);
            }
            else
            {
                _nuiRenderer.RenderButton(renderContext, layout);
            }

            _renderUtilities.RenderText(
                renderContext,
                new Vector2(layout.Center.X, layout.Center.Y + 1),
                button.Text,
                _fontAsset,
                HorizontalAlignment.Center,
                VerticalAlignment.Center,
                textColor: Color.Black,
                renderShadow: false);
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, Button container)
        {
            throw new NotSupportedException();
        }
    }
}
