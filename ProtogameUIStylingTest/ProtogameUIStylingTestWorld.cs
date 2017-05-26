namespace ProtogameUIStylingTest
{
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    using Protoinject;

    using Protogame;

    public class ProtogameUIStylingTestWorld : IWorld, IHasCanvases
    {
        private readonly NuiRenderer _nuiRenderer;
        private Canvas _canvas;
        private ISkinLayout _skinLayout;
        private ISkinDelegator _skinDelegator;
        private IAssetManager _assetManager;

        public ProtogameUIStylingTestWorld(
            INode worldNode,
            IHierarchy hierarchy,
            ISkinLayout skinLayout,
            ISkinDelegator skinDelegator,
            IAssetManager assetManager)
        {
            _skinLayout = skinLayout;
            _skinDelegator = skinDelegator;
            _assetManager = assetManager;

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
            var rightTabbableContainer = new SingleTabbedContainer { Title = "Inspector", Icon = _assetManager.Get<TextureAsset>("texture.IconInspector") };
            var rightContainer = new SingleContainer();
            rightTabbableContainer.SetChild(rightContainer);
            rightDockableLayoutContainer.AddInnerRegion(rightTabbableContainer);
            dockableLayoutContainer.SetRightRegion(rightDockableLayoutContainer);

            var bottomDockableLayoutContainer = new DockableLayoutContainer();
            var projectTabbableContainer = new SingleTabbedContainer { Title = "Project", Icon = _assetManager.Get<TextureAsset>("texture.IconFolder") };
            var projectContainer = new SingleContainer();
            projectTabbableContainer.SetChild(projectContainer);
            bottomDockableLayoutContainer.AddInnerRegion(projectTabbableContainer);
            var consoleTabbableContainer = new SingleTabbedContainer { Title = "Console", Icon = _assetManager.Get<TextureAsset>("texture.IconTerminal") };
            var consoleContainer = new SingleContainer();
            consoleTabbableContainer.SetChild(consoleContainer);
            bottomDockableLayoutContainer.AddInnerRegion(consoleTabbableContainer);
            dockableLayoutContainer.SetBottomRegion(bottomDockableLayoutContainer);

            var workspaceDockableLayoutContainer = new DockableLayoutContainer();
            var workspaceContainer = new DockableLayoutContainer();
            workspaceDockableLayoutContainer.AddInnerRegion(workspaceContainer);
            dockableLayoutContainer.AddInnerRegion(workspaceDockableLayoutContainer);

            var leftDockableLayoutContainer = new DockableLayoutContainer();
            var leftTabbableContainer = new SingleTabbedContainer { Title = "Hierarchy", Icon = _assetManager.Get<TextureAsset>("texture.IconHierarchy") };
            var leftContainer = new SingleContainer();
            leftTabbableContainer.SetChild(leftContainer);
            leftDockableLayoutContainer.AddInnerRegion(leftTabbableContainer);
            workspaceContainer.SetLeftRegion(leftDockableLayoutContainer);
            
            var worldTabbableContainer = new SingleTabbedContainer { Title = "World", Icon = _assetManager.Get<TextureAsset>("texture.IconGrid") };
            var worldContainer = new RelativeContainer();
            worldContainer.AddChild(new Button { Text = "World Button" }, new Rectangle(20, 20, 120, 18));
            worldTabbableContainer.SetChild(worldContainer);
            
            var gameTabbableContainer = new SingleTabbedContainer { Title = "Game", Icon = _assetManager.Get<TextureAsset>("texture.IconDirectionalPad") };
            var gameContainer = new RelativeContainer();
            gameContainer.AddChild(new Button { Text = "Game Button" }, new Rectangle(20, 20, 120, 18));
            gameTabbableContainer.SetChild(gameContainer);

            workspaceContainer.AddInnerRegion(worldTabbableContainer);
            workspaceContainer.AddInnerRegion(gameTabbableContainer);

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

        private List<Button> _toolButtons = new List<Button>();

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
        }

        public IEnumerable<KeyValuePair<Canvas, Rectangle>> Canvases { get; }
        public bool CanvasesEnabled { get; }
    }
}
