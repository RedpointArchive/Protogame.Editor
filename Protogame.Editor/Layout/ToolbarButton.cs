using System;

namespace Protogame.Editor.Layout
{
    public class ToolbarButton
    {
        public ToolbarButton()
        {
        }

        public ToolbarButton(string text, Action<IGameContext> onClick)
        {
            Text = text;
            OnClick = onClick;
        }

        public string Text { get; set; }

        public Action<IGameContext> OnClick { get; set; }
    }
}
