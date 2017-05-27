using System;
using Microsoft.Xna.Framework;
using Protogame;

namespace Protogame.Editor.Nui
{
    public class NuiLabelSkinRenderer : ISkinRenderer<Label>
    {
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly IAssetReference<FontAsset> _fontAsset;

        public NuiLabelSkinRenderer(I2DRenderUtilities renderUtilities, IAssetManager assetManager)
        {
            _renderUtilities = renderUtilities;
            _fontAsset = assetManager.Get<FontAsset>("font.UISmall");
        }

        public void Render(IRenderContext renderContext, Rectangle layout, Label label)
        {
            var textColor = Color.Black;
            if (label.AttachTo != null)
            {
                if (label.AttachTo.Focused)
                {
                    textColor = new Color(2, 141, 31, 255);
                }
            }

            _renderUtilities.RenderText(
                renderContext,
                new Vector2(layout.Center.X, layout.Center.Y + 1),
                label.Text,
                _fontAsset,
                HorizontalAlignment.Center,
                VerticalAlignment.Center,
                textColor: textColor,
                renderShadow: false);
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, Label label)
        {
            throw new NotSupportedException();
        }
    }
}
