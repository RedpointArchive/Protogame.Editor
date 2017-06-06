using Protogame.Editor.Api.Version1.EditorWindow;
using System;

namespace Protogame.Editor.Ext.Inspector
{
    public class InspectorEditorWindow : MarshalByRefObject, IEditorWindow
    {
        public void Update(IEditorWindowApi api)
        {
            //api.DeclareFieldValue();
        }
    }
}
