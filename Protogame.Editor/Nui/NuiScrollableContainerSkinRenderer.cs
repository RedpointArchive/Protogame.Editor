using System;
using Microsoft.Xna.Framework;
using Protogame;

namespace Protogame.Editor.Nui
{
    public class NuiScrollableContainerSkinRenderer : ISkinRenderer<ScrollableContainer>
    {
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly IAssetManager _assetManager;
        private readonly ISkinLayout _skinLayout;
        private readonly IAssetReference<TextureAsset> _scrollbarVerticalTexture;
        private readonly IAssetReference<TextureAsset> _scrollbarVerticalTopTexture;
        private readonly IAssetReference<TextureAsset> _scrollbarVerticalBottomTexture;
        private readonly IAssetReference<TextureAsset> _scrollbarVerticalBackgroundTexture;
        private readonly IAssetReference<TextureAsset> _scrollbarVerticalBackgroundTopTexture;
        private readonly IAssetReference<TextureAsset> _scrollbarVerticalBackgroundBottomTexture;

        public NuiScrollableContainerSkinRenderer(
            I2DRenderUtilities renderUtilities,
            IAssetManager assetManager,
            ISkinLayout skinLayout)
        {
            _renderUtilities = renderUtilities;
            _skinLayout = skinLayout;

            _scrollbarVerticalTexture = assetManager.Get<TextureAsset>("texture.ScrollbarVertical");
            _scrollbarVerticalTopTexture = assetManager.Get<TextureAsset>("texture.ScrollbarVerticalTop");
            _scrollbarVerticalBottomTexture = assetManager.Get<TextureAsset>("texture.ScrollbarVerticalBottom");
            _scrollbarVerticalBackgroundTexture = assetManager.Get<TextureAsset>("texture.ScrollbarVerticalBackground");
            _scrollbarVerticalBackgroundTopTexture = assetManager.Get<TextureAsset>("texture.ScrollbarVerticalBackgroundTop");
            _scrollbarVerticalBackgroundBottomTexture = assetManager.Get<TextureAsset>("texture.ScrollbarVerticalBackgroundBottom");

            _assetManager = assetManager;
        }

        public void Render(IRenderContext renderContext, Rectangle layout, ScrollableContainer scrollableContainer)
        {
            var vertAdjust = (scrollableContainer.NeedsVerticalScrollbar ? _skinLayout.VerticalScrollBarWidth : 0);
            var horAdjust = (scrollableContainer.NeedsHorizontalScrollbar ? _skinLayout.HorizontalScrollBarHeight : 0);

            var layoutWidth = layout.Width - vertAdjust;
            var layoutHeight = layout.Height - horAdjust;

            var layoutFullWidth = layout.Width - _skinLayout.VerticalScrollBarWidth;
            var layoutFullHeight = layout.Height - _skinLayout.HorizontalScrollBarHeight;

            if (scrollableContainer.NeedsVerticalScrollbar)
            {
                var xPosition = layout.X + layout.Width - _skinLayout.VerticalScrollBarWidth / 2 - 3;
                var yPosition = layout.Y + _skinLayout.VerticalScrollBarWidth / 2 - 1;
                var lastYPosition = layout.Y + layout.Height - _skinLayout.VerticalScrollBarWidth / 2 - 3;

                _renderUtilities.RenderRectangle(
                    renderContext,
                    new Rectangle(
                        layout.X + layout.Width - _skinLayout.VerticalScrollBarWidth,
                        layout.Y,
                        _skinLayout.VerticalScrollBarWidth,
                        layout.Height),
                    new Color(208, 208, 208, 255),
                    true);
                _renderUtilities.RenderLine(
                    renderContext,
                    new Vector2(layout.X + layout.Width - _skinLayout.VerticalScrollBarWidth - 1, layout.Y),
                    new Vector2(layout.X + layout.Width - _skinLayout.VerticalScrollBarWidth - 1, layout.Y + layout.Height - 1),
                    new Color(182, 182, 182, 255));

                _renderUtilities.RenderTexture(
                    renderContext,
                    new Vector2(xPosition, yPosition),
                    _scrollbarVerticalBackgroundTopTexture,
                    new Vector2(6, 3));
                _renderUtilities.RenderTexture(
                    renderContext,
                    new Vector2(xPosition, yPosition + 3),
                    _scrollbarVerticalBackgroundTexture,
                    new Vector2(6, lastYPosition - (yPosition + 3)));
                _renderUtilities.RenderTexture(
                    renderContext,
                    new Vector2(xPosition, lastYPosition),
                    _scrollbarVerticalBackgroundBottomTexture,
                    new Vector2(6, 3));

                var scrollbarOffset = (int)(yPosition + scrollableContainer.ScrollY * (layoutFullHeight - layoutFullHeight / (float)scrollableContainer.ChildHeight * layoutFullHeight));
                var scrollbarLastOffset = scrollbarOffset + (int)(layoutFullHeight / (float)scrollableContainer.ChildHeight * layoutFullHeight);

                _renderUtilities.RenderTexture(
                    renderContext,
                    new Vector2(xPosition, scrollbarOffset),
                    _scrollbarVerticalTopTexture,
                    new Vector2(6, 3));
                _renderUtilities.RenderTexture(
                    renderContext,
                    new Vector2(xPosition, scrollbarOffset + 3),
                    _scrollbarVerticalTexture,
                    new Vector2(6, scrollbarLastOffset - (scrollbarOffset + 3)));
                _renderUtilities.RenderTexture(
                    renderContext,
                    new Vector2(xPosition, scrollbarLastOffset),
                    _scrollbarVerticalBottomTexture,
                    new Vector2(6, 3));
            }

            _renderUtilities.RenderTexture(
                renderContext,
                new Vector2(layout.X, layout.Y),
                scrollableContainer.ChildContent,
                new Vector2(layoutWidth, layoutHeight));
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, ScrollableContainer container)
        {
            throw new NotSupportedException();
        }
    }
}
