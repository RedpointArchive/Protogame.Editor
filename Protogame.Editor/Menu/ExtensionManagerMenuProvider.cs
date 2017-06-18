using Protogame.Editor.EditorWindow;

namespace Protogame.Editor.Menu
{
    public class ExtensionManagerMenuProvider : IMenuProvider
    {
        private readonly IWindowManagement _windowManagement;

        public ExtensionManagerMenuProvider(
            IWindowManagement windowManagement)
        {
            _windowManagement = windowManagement;
        }

        public MenuEntry[] GetMenuItems()
        {
            return new[]
            {
                new MenuEntry("Help/Extension Manager...", true, 50, OpenExtensionManager, null)
            };
        }

        private void OpenExtensionManager(MenuEntry menuEntry)
        {
            _windowManagement.OpenDocument<ExtensionManagerEditorWindow>(null);
        }
    }
}
