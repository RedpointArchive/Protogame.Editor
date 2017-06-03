using Microsoft.Xna.Framework;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor.EditorWindow
{
    public class InspectorEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;
        private Label _label;
        private readonly IProjectManager _projectManager;

        public InspectorEditorWindow(
            IAssetManager assetManager,
            IProjectManager projectManager)
        {
            _assetManager = assetManager;
            _projectManager = projectManager;

            Title = "Inspector";
            Icon = _assetManager.Get<TextureAsset>("texture.IconInspector");

            _label = new Label { Text = "Inspector Window" };

            SetChild(_label);
        }

        public override bool Visible
        {
            get
            {
                return _projectManager.Project != null;
            }
            set { }
        }

        public override void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            base.Update(skinLayout, layout, gameTime, ref stealFocus);

            _label.Text = "Time: " + gameTime.TotalGameTime;
        }
    }
}
