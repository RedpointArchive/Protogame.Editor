using System;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Protogame.Editor.Layout
{
    public class ConsoleContainer : IContainer, IHasDesiredSize
    {
        private long _lastEntryCount = 0;

        public EditorConsole Console { get; set; }

        public IContainer[] Children => new IContainer[0];

        public bool Focused { get; set; }
        public int Order { get; set; }
        public IContainer Parent { get; set; }
        public object Userdata { get; set; }

        public int? GetDesiredHeight(ISkinLayout skin)
        {
            if (Console == null)
            {
                return null;
            }

            return Console.Entries.Sum(x => x.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length) * 16;
        }

        public int? GetDesiredWidth(ISkinLayout skin)
        {
            return null;
        }

        public bool HandleEvent(ISkinLayout skinLayout, Rectangle layout, IGameContext context, Event @event)
        {
            var mousePressEvent = @event as MousePressEvent;

            if (mousePressEvent != null)
            {
                if (layout.Contains(mousePressEvent.MouseState.Position))
                {
                    this.Parent.Focus();
                    return true;
                }
            }

            return false;
        }

        public void Render(IRenderContext context, ISkinLayout skinLayout, ISkinDelegator skinDelegator, Rectangle layout)
        {
            skinDelegator.Render(context, layout, this);
        }

        public void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            var scrollableContainer = this.Parent as ScrollableContainer;
            if (scrollableContainer != null)
            {
                if (_lastEntryCount != Console.EntryCount)
                {
                    scrollableContainer.ScrollY = 1f;
                    _lastEntryCount = Console.EntryCount;
                }
            }
        }
    }
}
