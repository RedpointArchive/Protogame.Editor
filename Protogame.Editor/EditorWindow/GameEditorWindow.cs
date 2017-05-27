using Microsoft.Xna.Framework;

namespace Protogame.Editor.EditorWindow
{
    public class GameEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;

        public GameEditorWindow(
            IAssetManager assetManager)
        {
            _assetManager = assetManager;

            Title = "Game";
            Icon = _assetManager.Get<TextureAsset>("texture.IconDirectionalPad");

            var gameContainer = new RelativeContainer();
            gameContainer.AddChild(new Button { Text = "Game Button" }, new Rectangle(20, 20, 120, 18));
            SetChild(gameContainer);
        }
    }
}
