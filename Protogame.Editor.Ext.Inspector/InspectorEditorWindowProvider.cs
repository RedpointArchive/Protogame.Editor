using Protogame.Editor.Api.Version1.EditorWindow;
using System.Collections.Generic;

namespace Protogame.Editor.Ext.Inspector
{
    public class InspectorEditorWindowProvider : IEditorWindowProvider
    {
        public IEnumerable<EditorWindowDeclaration> GetAvailableWindows()
        {
            yield return new EditorWindowDeclaration(typeof(InspectorEditorWindow), "Inspector", EditorWindowLevel.ProjectLevel);
        }
    }
}
