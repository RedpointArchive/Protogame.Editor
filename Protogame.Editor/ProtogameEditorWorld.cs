using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Protoinject;

using Protogame;
using Protogame.Editor.Nui;
using Protogame.Editor.Menu;
using Protogame.Editor.Layout;
using Protogame.Editor.EditorWindow;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor
{
    public class ProtogameEditorWorld : IWorld, IHasCanvases
    {
        private readonly NuiRenderer _nuiRenderer;
        private Canvas _canvas;
        private ISkinLayout _skinLayout;
        private ISkinDelegator _skinDelegator;
        private IAssetManager _assetManager;
        private List<Button> _toolButtons = new List<Button>();
        private readonly IMainMenuController _mainMenuController;
        private readonly IEditorWindowFactory _editorWindowFactory;
        private readonly IProjectManager _projectManager;

        public ProtogameEditorWorld(
            INode worldNode,
            IHierarchy hierarchy,
            ISkinLayout skinLayout,
            ISkinDelegator skinDelegator,
            IAssetManager assetManager,
            IMainMenuController mainMenuController,
            IEditorWindowFactory editorWindowFactory,
            IProjectManager projectManager)
        {
            _skinLayout = skinLayout;
            _skinDelegator = skinDelegator;
            _assetManager = assetManager;
            _mainMenuController = mainMenuController;
            _editorWindowFactory = editorWindowFactory;
            _projectManager = projectManager;

            SetupCanvas();

            var entity = new CanvasEntity(_skinLayout, _skinDelegator);
            entity.Canvas = _canvas;
            hierarchy.AddChildNode(worldNode, hierarchy.CreateNodeForObject(entity));
        }

        private void SetupCanvas()
        {
            _canvas = new Canvas();

            var dockableLayoutContainer = new DockableLayoutContainer();
            dockableLayoutContainer.RightWidth = 250;
            dockableLayoutContainer.BottomHeight = 250;

            var rightDockableLayoutContainer = new DockableLayoutContainer();
            rightDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateInspectorEditorWindow());
            dockableLayoutContainer.SetRightRegion(rightDockableLayoutContainer);

            var bottomDockableLayoutContainer = new DockableLayoutContainer();
            bottomDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateProjectEditorWindow());
            bottomDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateConsoleEditorWindow());
            dockableLayoutContainer.SetBottomRegion(bottomDockableLayoutContainer);

            var workspaceDockableLayoutContainer = new DockableLayoutContainer();
            var workspaceContainer = new DockableLayoutContainer();
            workspaceDockableLayoutContainer.AddInnerRegion(workspaceContainer);
            dockableLayoutContainer.AddInnerRegion(workspaceDockableLayoutContainer);

            var leftDockableLayoutContainer = new DockableLayoutContainer();
            leftDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateHierarchyEditorWindow());
            workspaceContainer.SetLeftRegion(leftDockableLayoutContainer);
            
            workspaceContainer.AddInnerRegion(_editorWindowFactory.CreateWorldEditorWindow());
            workspaceContainer.AddInnerRegion(_editorWindowFactory.CreateGameEditorWindow());

            var panButton = CreateToolButton("texture.IconToolPan", "pan");
            panButton.Toggled = true;

            var relativeContainer = new RelativeContainer();
            relativeContainer.AddChild(panButton, new Rectangle(16, 8, 28, 28));
            relativeContainer.AddChild(CreateToolButton("texture.IconToolMove", "move"), new Rectangle(16 + 30 * 1, 8, 28, 28));
            relativeContainer.AddChild(CreateToolButton("texture.IconToolRotate", "rotate"), new Rectangle(16 + 30 * 2, 8, 28, 28));
            relativeContainer.AddChild(CreateToolButton("texture.IconToolResize", "resize"), new Rectangle(16 + 30 * 3, 8, 28, 28));
            relativeContainer.AddChild(CreateToolButton("texture.IconToolSelect", "select"), new Rectangle(16 + 30 * 4, 8, 28, 28));

            var verticalContainer = new VerticalContainer();
            verticalContainer.AddChild(relativeContainer, "40");
            verticalContainer.AddChild(dockableLayoutContainer, "*");

            _canvas.SetChild(verticalContainer);
        }

        private Button CreateToolButton(string texture, string tool)
        {
            var button = new Button
            {
                Icon = _assetManager.Get<TextureAsset>(texture)
            };
            button.Click += (sender, e) =>
            {
                foreach (var b in _toolButtons)
                {
                    b.Toggled = false;
                }

                button.Toggled = true;
            };
            _toolButtons.Add(button);
            return button;
        }

        public void Dispose()
        {
        }

        public void RenderAbove(IGameContext gameContext, IRenderContext renderContext)
        {
        }

        public void RenderBelow(IGameContext gameContext, IRenderContext renderContext)
        {
            if (renderContext.IsFirstRenderPass())
            {
                gameContext.Graphics.GraphicsDevice.Clear(new Color(162, 162, 162, 255));
            }
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            _mainMenuController.Update(gameContext, updateContext);

            gameContext.Window.Title = "Protogame 7.0.0 (" + _projectManager?.Project?.Name + "; Build c510ef6)";
        }

        public IEnumerable<KeyValuePair<Canvas, Rectangle>> Canvases { get; }

        public bool CanvasesEnabled { get; }
    }
}
