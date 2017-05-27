using System;
using Protogame;
using Protoinject;
using Protogame.Editor.Nui;
using Protogame.Editor.Layout;
using Protogame.Editor.Menu;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor
{
    public class ProtogameEditorModule : IProtoinjectModule
    {
        public void Load(IKernel kernel)
        {
            kernel.Bind<IBasicSkin>().To<DefaultBasicSkin>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<Button>>().To<NuiButtonSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<CheckBox>>().To<NuiCheckBoxSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<Canvas>>().To<NuiCanvasSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<Label>>().To<NuiLabelSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<DockableLayoutContainer>>().To<NuiDockableLayoutContainerSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<SingleContainer>>().To<NuiSingleContainerSkinRenderer>().InSingletonScope();

#if PLATFORM_WINDOWS
            kernel.Bind<IMainMenuController>().To<WindowsMainMenuController>().InSingletonScope();
#endif

            kernel.Bind<IMenuProvider>().To<ProjectManagerMenuProvider>().InSingletonScope();
            kernel.Bind<IMenuProvider>().To<ActionManagerMenuProvider>().InSingletonScope();

            kernel.Bind<IProjectManager>().To<ProjectManager>().InSingletonScope();
        }
    }
}

