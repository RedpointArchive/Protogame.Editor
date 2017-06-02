using Microsoft.Xna.Framework;

namespace Protogame.Editor.EditorWindow
{
    public class InspectorEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;
        private Label _label;

        public InspectorEditorWindow(
            IAssetManager assetManager)
        {
            _assetManager = assetManager;

            Title = "Inspector";
            Icon = _assetManager.Get<TextureAsset>("texture.IconInspector");

            _label = new Label { Text = "Inspector Window" };

            SetChild(_label);
        }

        public override void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            base.Update(skinLayout, layout, gameTime, ref stealFocus);

            _label.Text = "Time: " + gameTime.TotalGameTime;
        }
    }
}
