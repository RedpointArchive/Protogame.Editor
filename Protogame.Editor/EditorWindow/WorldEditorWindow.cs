using Microsoft.Xna.Framework;
using Protogame.Editor.ProjectManagement;
using System.Threading.Tasks;

namespace Protogame.Editor.EditorWindow
{
    public class WorldEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;
        private readonly IProjectManager _projectManager;

        public WorldEditorWindow(
            IAssetManager assetManager,
            IProjectManager projectManager)
        {
            _assetManager = assetManager;
            _projectManager = projectManager;

            Title = "World";
            Icon = _assetManager.Get<TextureAsset>("texture.IconGrid");

            var worldContainer = new RelativeContainer();
            worldContainer.AddChild(new Button { Text = "World Button" }, new Rectangle(20, 20, 120, 18));
            SetChild(worldContainer);
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
