namespace Protogame.Editor.Menu
{
    public class ActionManagerMenuProvider : IMenuProvider
    {
        public MenuEntry[] GetMenuItems()
        {
            return new[]
            {
                new MenuEntry("Edit/Undo", true, 0, OnUndoAction, null) { DynamicTextHandler = OnUndoTextHandler, DynamicEnabledHandler = OnUndoEnabledHandler },
                new MenuEntry("Edit/Redo", true, 1, OnRedoAction, null) { DynamicTextHandler = OnRedoTextHandler, DynamicEnabledHandler = OnRedoEnabledHandler },
            };
        }

        private void OnUndoAction(MenuEntry obj)
        {
        }

        private string OnUndoTextHandler(MenuEntry arg)
        {
            return "Undo";
        }

        private bool OnUndoEnabledHandler(MenuEntry arg)
        {
            return false;
        }

        private void OnRedoAction(MenuEntry obj)
        {
        }

        private string OnRedoTextHandler(MenuEntry arg)
        {
            return "Redo";
        }

        private bool OnRedoEnabledHandler(MenuEntry arg)
        {
            return false;
        }
    }
}
