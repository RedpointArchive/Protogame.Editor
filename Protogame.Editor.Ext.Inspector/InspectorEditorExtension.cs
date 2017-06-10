using System;
using Protogame.Editor.Api.Version1;
using Protogame.Editor.Ext.Inspector;
using Protogame.Editor.Api.Version1.EditorWindow;
using Protoinject;

[assembly: Extension(typeof(InspectorEditorExtension))]

namespace Protogame.Editor.Ext.Inspector
{
    public class InspectorEditorExtension : IEditorExtension
    {
        public void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IEditorWindowProvider>().To<InspectorEditorWindowProvider>().InSingletonScope();
        }
    }
}
