using Protogame.Editor.Api.Version1;
using Protogame.Editor.Api.Version1.Menu;
using Protogame.Editor.Ext.CodeManager;
using Protogame.Editor.Api.Version1.Core;
using Protoinject;
using Protogame.Editor.Api.Version1.Toolbar;

[assembly: Extension(typeof(CodeManagerEditorExtension))]

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerEditorExtension : IEditorExtension
    {
        public void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IMenuProvider>().To<CodeManagerMenuProvider>().InSingletonScope();
            kernel.Bind<IToolbarProvider>().To<CodeManagerToolbarProvider>().InSingletonScope();
            kernel.Bind<IWantsUpdateSignal>().To<CodeManagerUpdateSignal>().InSingletonScope();
            kernel.Bind<ICodeManagerService>().To<CodeManagerService>().InSingletonScope();
            kernel.Bind<IApiReferenceService>().To<ApiReferenceService>().InSingletonScope();
        }
    }
}
