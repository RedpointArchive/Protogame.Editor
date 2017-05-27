using System;
using Microsoft.Xna.Framework;
using Protogame;

namespace Protogame.Editor.Nui
{
    public class NuiListItemSkinRenderer : ISkinRenderer<ListItem>
    {
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly NuiRenderer _nuiRenderer;
        private readonly IAssetReference<FontAsset> _fontAsset;

        public NuiListItemSkinRenderer(
            I2DRenderUtilities renderUtilities,
            NuiRenderer nuiRenderer,
            IAssetManager assetManager)
        {
            _renderUtilities = renderUtilities;
            _nuiRenderer = nuiRenderer;
            _fontAsset = assetManager.Get<FontAsset>("font.UISmall");
        }

        public void Render(IRenderContext renderContext, Rectangle layout, ListItem listItem)
        {
            var textHighlighted = false;

            if (listItem.Parent is ListView)
            {
                var view = listItem.Parent as ListView;
                if (view.SelectedItem == listItem)
                {
                    _renderUtilities.RenderRectangle(
                        renderContext,
                        new Rectangle(layout.X, layout.Y, layout.Width, layout.Height),
                        (listItem.Focused || view.Focused) ? new Color(22, 170, 66, 255) : new Color(66, 66, 66, 255),
                        true);
                    textHighlighted = true;
                }
            }

            if (listItem.Icon != null)
            {
                _renderUtilities.RenderTexture(
                    renderContext,
                    new Vector2(layout.X + 2, layout.Y + 1),
                    listItem.Icon,
                    new Vector2(layout.Height - 2, layout.Height - 2),
                    textHighlighted ? Color.White : Color.Black);
            }

            _renderUtilities.RenderText(
                renderContext,
                new Vector2(layout.X + 5 + layout.Height, layout.Center.Y),
                listItem.Text,
                _fontAsset,
                verticalAlignment: VerticalAlignment.Center,
                textColor: textHighlighted ? Color.White : Color.Black,
                renderShadow: false);
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, ListItem listItem)
        {
            throw new NotSupportedException();
        }
    }
}
