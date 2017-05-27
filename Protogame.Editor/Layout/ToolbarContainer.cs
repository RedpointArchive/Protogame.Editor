using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Protogame.Editor.Layout
{
    public class ToolbarContainer : SingleContainer
    {
        public ToolbarContainer()
        {
            Buttons = new List<ToolbarButton>();
        }

        public List<ToolbarButton> Buttons { get; }

        public override void Render(IRenderContext context, ISkinLayout skinLayout, ISkinDelegator skinDelegator, Rectangle layout)
        {
            skinDelegator.Render(context, layout, this);
            Children[0]?.Render(context, skinLayout, skinDelegator, GetChildLayout(layout, skinLayout));
        }
    }
}
