using Microsoft.Xna.Framework;
using Protogame.Editor.LoadedGame;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor.EditorWindow
{
    public class ProfilerEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;
        private Label _readStallLabel;
        private Label _writeStallLabel;
        private readonly ILoadedGame _loadedGame;
        private readonly IProjectManager _projectManager;

        public ProfilerEditorWindow(
            IAssetManager assetManager,
            ILoadedGame loadedGame,
            IProjectManager projectManager)
        {
            _assetManager = assetManager;
            _loadedGame = loadedGame;
            _projectManager = projectManager;

            Title = "Profiler";
            Icon = _assetManager.Get<TextureAsset>("texture.IconProfiler");

            var verticalContainer = new VerticalContainer();
            _readStallLabel = new Label();
            _writeStallLabel = new Label();
            verticalContainer.AddChild(_readStallLabel, "20");
            verticalContainer.AddChild(_writeStallLabel, "20");
            verticalContainer.AddChild(new EmptyContainer(), "*");

            SetChild(verticalContainer);
        }

        public override bool Visible
        {
            get
            {
                return _projectManager.Project != null/* && _loadedGame.Playing*/;
            }
            set { }
        }

        public override void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            base.Update(skinLayout, layout, gameTime, ref stealFocus);

            /*var t = _loadedGame.GetStallState();
            _readStallLabel.Text = "Read Stall: " + (t.Item1 ? "Yes" : "No");
            _readStallLabel.TextColor = t.Item1 ? Color.Red : Color.Black;
            _readStallLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _writeStallLabel.Text = "Write Stall: " + (t.Item2 ? "Yes" : "No");
            _writeStallLabel.TextColor = t.Item2 ? Color.Red : Color.Black;
            _writeStallLabel.HorizontalAlignment = HorizontalAlignment.Left;*/
        }
    }
}
