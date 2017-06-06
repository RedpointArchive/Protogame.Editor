using System.Collections.Generic;

namespace Protogame.Editor.Api.Version1.EditorWindow
{
    public interface IEditorWindowProvider
    {
        IEnumerable<EditorWindowDeclaration> GetAvailableWindows();
    }
}
