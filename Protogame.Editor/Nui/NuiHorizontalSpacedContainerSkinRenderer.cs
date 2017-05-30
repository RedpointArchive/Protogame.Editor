using System;
using Microsoft.Xna.Framework;
using Protogame;
using System.Linq;
using Protogame.Editor.Layout;

namespace Protogame.Editor.Nui
{
    public class NuiHorizontalSpacedContainerSkinRenderer : ISkinRenderer<HorizontalSpacedContainer>
    {
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly IAssetManager _assetManager;
        private readonly ISkinLayout _skinLayout;

        public NuiHorizontalSpacedContainerSkinRenderer(
            I2DRenderUtilities renderUtilities,
            IAssetManager assetManager,
            ISkinLayout skinLayout)
        {
            _renderUtilities = renderUtilities;
            _skinLayout = skinLayout;

            _assetManager = assetManager;
        }

        public void Render(IRenderContext renderContext, Rectangle layout, HorizontalSpacedContainer horizontalContainer)
        {
            var children = horizontalContainer.ChildrenWithLayouts(layout).ToArray();
            for (var i = 0; i < children.Length; i++)
            {
                if (i < children.Length - 1)
                {
                    _renderUtilities.RenderLine(
                        renderContext,
                        new Vector2(children[i].Value.Right, children[i].Value.Y),
                        new Vector2(children[i].Value.Right, children[i].Value.Bottom),
                        new Color(0, 0, 0, 72));
                }
            }
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, HorizontalSpacedContainer horizontalContainer)
        {
            throw new NotSupportedException();
        }
    }
}
