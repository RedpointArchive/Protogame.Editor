using System;
using Microsoft.Xna.Framework;

namespace Protogame.Editor.Layout
{
    public class ClickableHorizontalContainer : HorizontalContainer
    {
        public EventHandler<ProtogameEventArgs> Click;

        public override bool HandleEvent(ISkinLayout skinLayout, Rectangle layout, IGameContext context, Event @event)
        {
            foreach (var kv in ChildrenWithLayouts(layout))
            {
                if (kv.Key is Button)
                {
                    if (kv.Key.HandleEvent(skinLayout, kv.Value, context, @event))
                    {
                        return true;
                    }
                }
            }

            var mouseReleaseEvent = @event as MouseReleaseEvent;
            if (mouseReleaseEvent != null && mouseReleaseEvent.Button == MouseButton.Left)
            {
                if (layout.Contains(mouseReleaseEvent.Position))
                {
                    Click?.Invoke(this, new ProtogameEventArgs(context));
                    return true;
                }
            }

            return base.HandleEvent(skinLayout, layout, context, @event);
        }
    }
}
