using Protogame.Editor.Layout;
using System;

namespace Protogame.Editor.EditorWindow
{
    public interface IWindowManagement
    {
        void OpenDocument<T>(object parameters) where T : EditorWindow;

        void ActivateWhere(Func<object, bool> filter);

        void SetMainDocumentContainer(DockableLayoutContainer workspaceContainer);
    }
}
