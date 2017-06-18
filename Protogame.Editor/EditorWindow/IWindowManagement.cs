using Protogame.Editor.Layout;

namespace Protogame.Editor.EditorWindow
{
    public interface IWindowManagement
    {
        void OpenDocument<T>(object parameters) where T : EditorWindow;

        void SetMainDocumentContainer(DockableLayoutContainer workspaceContainer);
    }
}
