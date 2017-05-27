using Protogame.Editor.Layout;

namespace Protogame.Editor.EditorWindow
{
    public class HierarchyEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;

        public HierarchyEditorWindow(
            IAssetManager assetManager)
        {
            _assetManager = assetManager;

            Title = "Hierarchy";
            Icon = _assetManager.Get<TextureAsset>("texture.IconHierarchy");

            _hierarchyTreeView = new TreeView();

            var scrollableHierarchyContainer = new ScrollableContainer();

            scrollableHierarchyContainer.SetChild(_hierarchyTreeView);

            var toolbarContainer = new ToolbarContainer();
            toolbarContainer.SetChild(scrollableHierarchyContainer);

            SetChild(toolbarContainer);
        }
    }
}
