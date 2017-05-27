using System;
using Microsoft.Xna.Framework;
using Protogame;

namespace Protogame.Editor.Nui
{
    public class NuiTreeViewSkinRenderer : ISkinRenderer<TreeView>
    {
        private readonly I2DRenderUtilities _renderUtilities;

        public NuiTreeViewSkinRenderer(I2DRenderUtilities renderUtilities)
        {
            _renderUtilities = renderUtilities;
        }

        public void Render(IRenderContext renderContext, Rectangle layout, TreeView container)
        {
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, TreeView container)
        {
            throw new NotSupportedException();
        }
    }
}
