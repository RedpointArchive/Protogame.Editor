using Protoinject;
using Protogame.Editor.Nui;
using Protogame.Editor.Layout;
using Protogame.Editor.Menu;
using Protogame.Editor.ProjectManagement;
using Protogame.Editor.EditorWindow;
using Protogame.Editor.LoadedGame;
using Protogame.Editor.Extension;
using Protogame.Editor.Server;

namespace Protogame.Editor
{
    public class ProtogameEditorModule : IProtoinjectModule
    {
        public void Load(IKernel kernel)
        {
            kernel.Bind<IBasicSkin>().To<DefaultBasicSkin>().InSingletonScope();
            kernel.Rebind<ISkinLayout>().To<NuiSkinLayout>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<Button>>().To<NuiButtonSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<CheckBox>>().To<NuiCheckBoxSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<Canvas>>().To<NuiCanvasSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<Label>>().To<NuiLabelSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<DockableLayoutContainer>>().To<NuiDockableLayoutContainerSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<SingleContainer>>().To<NuiSingleContainerSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<ListView>>().To<NuiListViewSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<ListItem>>().To<NuiListItemSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<ScrollableContainer>>().To<NuiScrollableContainerSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<HorizontalContainer>>().To<NuiHorizontalContainerSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<HorizontalSpacedContainer>>().To<NuiHorizontalSpacedContainerSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<ToolbarContainer>>().To<NuiToolbarContainerSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<TreeView>>().To<NuiTreeViewSkinRenderer>().InSingletonScope();
            kernel.Rebind<ISkinRenderer<ConsoleContainer>>().To<NuiConsoleContainerSkinRenderer>().InSingletonScope();

#if PLATFORM_WINDOWS
            kernel.Bind<IMainMenuController>().To<WindowsMainMenuController>().InSingletonScope();
#endif

            kernel.Bind<IMenuProvider>().To<ProjectManagerMenuProvider>().InSingletonScope();
            kernel.Bind<IMenuProvider>().To<ActionManagerMenuProvider>().InSingletonScope();
            kernel.Bind<IMenuProvider>().To<ExtensionBasedMenuProvider>().InSingletonScope();
            kernel.Bind<IMenuProvider>().To<ExtensionManagerMenuProvider>().InSingletonScope();

            kernel.Bind<IProjectManager>().To<ProjectManager>().InSingletonScope();
            kernel.Bind<IProjectManagerUi>().To<ProjectManagerUi>().InSingletonScope();

            kernel.Bind<IEditorWindowFactory>().ToFactory();

            kernel.Bind<ProjectEditorWindow>().To<ProjectEditorWindow>().DiscardNodeOnResolve();

            kernel.Rebind<IConsole>().To<EditorConsole>().InSingletonScope();

            kernel.Rebind<ICanvasRenderPass>().To<EditorCanvasRenderPass>().DiscardNodeOnResolve();

            kernel.Bind<IEventBinder<IGameContext>>().To<EditorHotKeyBinder>();

            kernel.Bind<NuiRenderer>().To<NuiRenderer>().InSingletonScope();

            kernel.Bind<IEditorUserDataPathProvider>().To<EditorUserDataPathProvider>().InSingletonScope();
            kernel.Bind<IRecentProjects>().To<RecentProjects>().InSingletonScope();
            kernel.Bind<IThumbnailSampler>().To<ThumbnailSampler>().InSingletonScope();

            //kernel.Bind<IServiceRegistration>().To<ExtensionServiceRegistration>().InSingletonScope();
            kernel.Bind<IExtensionManager>().To<ExtensionManager>().InSingletonScope();
            kernel.Bind<IDynamicServiceProvider>().To<ExtensionDynamicServiceProvider>().InSingletonScope();

            kernel.Bind<IGrpcServer>().To<GrpcServer>().InSingletonScope();

            kernel.Bind<IWindowManagement>().To<DefaultWindowManagement>().InSingletonScope();

            //kernel.Bind<Protogame.Editor.Api.Version1.Core.IConsoleHandle>().To<ExtensionConsoleHandle>().InSingletonScope();
        }
    }
}

