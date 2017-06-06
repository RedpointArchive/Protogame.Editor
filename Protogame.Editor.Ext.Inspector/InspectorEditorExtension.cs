using System;
using Protogame.Editor.Api.Version1;
using Protogame.Editor.Ext.Inspector;
using Protogame.Editor.Api.Version1.EditorWindow;

[assembly: Extension(typeof(InspectorEditorExtension))]

namespace Protogame.Editor.Ext.Inspector
{
    public class InspectorEditorExtension : MarshalByRefObject, IEditorExtension
    {
        public void RegisterServices(IServiceRegistration services)
        {
            services.BindSingleton<IEditorWindowProvider, InspectorEditorWindowProvider>();
        }
    }
}
