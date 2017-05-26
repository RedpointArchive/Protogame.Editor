using System;
using System.Collections.Generic;

namespace ProtogameUIStylingTest
{
    public class ActionManager : IMenuProvider
    {
        public IEnumerable<MenuEntry> GetMenuItems()
        {
            yield return new MenuEntry("Edit/Undo", true, 0, OnUndoAction, null) { DynamicTextHandler = OnUndoTextHandler, DynamicEnabledHandler = OnUndoEnabledHandler };
            yield return new MenuEntry("Edit/Redo", true, 1, OnRedoAction, null) { DynamicTextHandler = OnRedoTextHandler, DynamicEnabledHandler = OnRedoEnabledHandler };
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
