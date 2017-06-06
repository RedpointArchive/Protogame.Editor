using Protogame.Editor.Api.Version1.ProjectManagement;
using Protogame.Editor.Layout;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor.EditorWindow
{
    public class HierarchyEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;
        private readonly IProjectManager _projectManager;

        public HierarchyEditorWindow(
            IAssetManager assetManager,
            IProjectManager projectManager)
        {
            _assetManager = assetManager;
            _projectManager = projectManager;

            Title = "Hierarchy";
            Icon = _assetManager.Get<TextureAsset>("texture.IconHierarchy");

            _hierarchyTreeView = new TreeView();

            var scrollableHierarchyContainer = new ScrollableContainer();

            scrollableHierarchyContainer.SetChild(_hierarchyTreeView);

            var toolbarContainer = new ToolbarContainer();
            toolbarContainer.SetChild(scrollableHierarchyContainer);

            SetChild(toolbarContainer);
        }

        public override bool Visible
        {
            get
            {
                return _projectManager.Project != null;
            }
            set { }
        }
    }
}
