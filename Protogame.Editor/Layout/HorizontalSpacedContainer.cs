using System.Linq;
using Microsoft.Xna.Framework;

namespace Protogame.Editor.Layout
{
    public class HorizontalSpacedContainer : HorizontalContainer
    {
        public override void Render(IRenderContext context, ISkinLayout skinLayout, ISkinDelegator skinDelegator, Rectangle layout)
        {
            skinDelegator.Render(context, layout, this);
            foreach (var kv in ChildrenWithLayouts(layout).OrderByDescending(x => x.Key.Order))
            {
                kv.Key.Render(context, skinLayout, skinDelegator, kv.Value);
            }
        }
    }
}
