using Protogame.Editor.Api.Version1;
using Protogame.Editor.Api.Version1.Menu;
using Protogame.Editor.Ext.CodeManager;
using Protogame.Editor.Api.Version1.Core;

[assembly: Extension(typeof(CodeManagerEditorExtension))]

namespace Protogame.Editor.Ext.CodeManager
{
    public class CodeManagerEditorExtension : IEditorExtension
    {
        public void RegisterServices(IServiceRegistration services)
        {
            services.BindSingleton<IMenuProvider, CodeManagerMenuProvider>();
            services.BindSingleton<IWantsUpdateSignal, CodeManagerUpdateSignal>();
            services.BindSingleton<ICodeManagerService, CodeManagerService>();
        }
    }
}
