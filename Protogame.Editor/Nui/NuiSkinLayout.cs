using Protogame.Editor.Layout;

namespace Protogame.Editor.Nui
{
    public class NuiSkinLayout : ISkinLayout
    {
        public int AdditionalMenuItemWidth => 20;

        public int HeightForTreeItem => 16;

        public int ListHorizontalPadding => 0;

        public int ListVerticalPadding => 0;

        public int MainMenuHorizontalPadding => 10;

        public int MenuItemHeight => 24;

        public int HorizontalScrollBarHeight => 16;

        public int VerticalScrollBarWidth => 16;

        public int GetLeftPadding(IContainer container, object context)
        {
            if (container is SingleContainer)
            {
                return 1;
            }

            return 0;
        }

        public int GetRightPadding(IContainer container, object context)
        {
            if (container is SingleContainer)
            {
                return 1;
            }

            return 0;
        }

        public int GetTopPadding(IContainer container, object context)
        {
            if (container is ToolbarContainer)
            {
                return 18;
            }

            if (container is SingleContainer)
            {
                return 1;
            }

            return 0;
        }

        public int GetBottomPadding(IContainer container, object context)
        {
            if (container is SingleContainer)
            {
                return 1;
            }

            return 0;
        }
    }
}
