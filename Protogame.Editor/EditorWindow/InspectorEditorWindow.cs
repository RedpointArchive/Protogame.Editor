namespace Protogame.Editor.EditorWindow
{
    public class InspectorEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;

        public InspectorEditorWindow(
            IAssetManager assetManager)
        {
            _assetManager = assetManager;

            Title = "Inspector";
            Icon = _assetManager.Get<TextureAsset>("texture.IconInspector");
            
            SetChild(new Label { Text = "Inspector Window" });
        }
    }
}
