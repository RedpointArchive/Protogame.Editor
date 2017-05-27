using Microsoft.Xna.Framework;

namespace Protogame.Editor.EditorWindow
{
    public class WorldEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;

        public WorldEditorWindow(
            IAssetManager assetManager)
        {
            _assetManager = assetManager;

            Title = "World";
            Icon = _assetManager.Get<TextureAsset>("texture.IconGrid");

            var worldContainer = new RelativeContainer();
            worldContainer.AddChild(new Button { Text = "World Button" }, new Rectangle(20, 20, 120, 18));
            SetChild(worldContainer);
        }
    }
}
