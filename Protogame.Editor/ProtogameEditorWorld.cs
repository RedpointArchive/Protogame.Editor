using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Protoinject;

using Protogame;
using Protogame.Editor.Nui;
using Protogame.Editor.Menu;
using Protogame.Editor.Layout;
using Protogame.Editor.EditorWindow;
using Protogame.Editor.ProjectManagement;
using Protogame.Editor.LoadedGame;
using System;

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
        private readonly ILoadedGame _loadedGame;
        private Button _playButton;
        private Button _pauseButton;
        private Button _stopButton;
        private DockableLayoutContainer _workspaceContainer;
        private WorldEditorWindow _worldEditorWindow;
        private GameEditorWindow _gameEditorWindow;
        private readonly IRecentProjects _recentProjects;
        private readonly IThumbnailSampler _thumbnailSampler;

        public ProtogameEditorWorld(
            INode worldNode,
            IHierarchy hierarchy,
            ISkinLayout skinLayout,
            ISkinDelegator skinDelegator,
            IAssetManager assetManager,
            IMainMenuController mainMenuController,
            IEditorWindowFactory editorWindowFactory,
            IProjectManager projectManager,
            ILoadedGame loadedGame,
            IRecentProjects recentProjects,
            IThumbnailSampler thumbnailSampler)
        {
            _skinLayout = skinLayout;
            _skinDelegator = skinDelegator;
            _assetManager = assetManager;
            _mainMenuController = mainMenuController;
            _editorWindowFactory = editorWindowFactory;
            _projectManager = projectManager;
            _loadedGame = loadedGame;
            _recentProjects = recentProjects;
            _thumbnailSampler = thumbnailSampler;

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

            var profilerDockableLayoutContainer = new DockableLayoutContainer();
            profilerDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateProfilerEditorWindow());
            
            var rightDockableLayoutContainer = new DockableLayoutContainer();
            rightDockableLayoutContainer.SetTopRegion(profilerDockableLayoutContainer);
            rightDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateInspectorEditorWindow());
            dockableLayoutContainer.SetRightRegion(rightDockableLayoutContainer);

            var bottomDockableLayoutContainer = new DockableLayoutContainer();
            bottomDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateProjectEditorWindow());
            bottomDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateConsoleEditorWindow());
            dockableLayoutContainer.SetBottomRegion(bottomDockableLayoutContainer);

            var workspaceDockableLayoutContainer = new DockableLayoutContainer();
            _workspaceContainer = new DockableLayoutContainer();
            workspaceDockableLayoutContainer.AddInnerRegion(_workspaceContainer);
            dockableLayoutContainer.AddInnerRegion(workspaceDockableLayoutContainer);

            var leftDockableLayoutContainer = new DockableLayoutContainer();
            leftDockableLayoutContainer.AddInnerRegion(_editorWindowFactory.CreateHierarchyEditorWindow());
            _workspaceContainer.SetLeftRegion(leftDockableLayoutContainer);

            _workspaceContainer.AddInnerRegion(_editorWindowFactory.CreateStartEditorWindow());
            _workspaceContainer.AddInnerRegion(_worldEditorWindow = _editorWindowFactory.CreateWorldEditorWindow());
            _workspaceContainer.AddInnerRegion(_gameEditorWindow = _editorWindowFactory.CreateGameEditorWindow());

            var panButton = CreateToolButton("texture.IconToolPan", "pan");
            panButton.Toggled = true;

            var toolContainer = new RelativeContainer();
            toolContainer.AddChild(panButton, new Rectangle(16, 8, 28, 28));
            toolContainer.AddChild(CreateToolButton("texture.IconToolMove", "move"), new Rectangle(16 + 30 * 1, 8, 28, 28));
            toolContainer.AddChild(CreateToolButton("texture.IconToolRotate", "rotate"), new Rectangle(16 + 30 * 2, 8, 28, 28));
            toolContainer.AddChild(CreateToolButton("texture.IconToolResize", "resize"), new Rectangle(16 + 30 * 3, 8, 28, 28));
            toolContainer.AddChild(CreateToolButton("texture.IconToolSelect", "select"), new Rectangle(16 + 30 * 4, 8, 28, 28));

            var gameControlContainer = new RelativeContainer();
            gameControlContainer.AddChild(_playButton = CreatePlayButton("texture.IconPlay"), new Rectangle(30 * 0, 8, 28, 28));
            gameControlContainer.AddChild(_pauseButton = CreatePauseButton("texture.IconPause"), new Rectangle(30 * 1, 8, 28, 28));
            gameControlContainer.AddChild(_stopButton = CreateStopButton("texture.IconStop"), new Rectangle(30 * 2, 8, 28, 28));

            var unusedContainer = new RelativeContainer();

            var horizontalContainer = new HorizontalContainer();
            horizontalContainer.AddChild(toolContainer, "*");
            horizontalContainer.AddChild(gameControlContainer, "88");
            horizontalContainer.AddChild(unusedContainer, "*");

            var verticalContainer = new VerticalContainer();
            verticalContainer.AddChild(horizontalContainer, "40");
            verticalContainer.AddChild(dockableLayoutContainer, "*");

            _canvas.SetChild(verticalContainer);
        }

        private Button CreatePlayButton(string texture)
        {
            var button = new Button
            {
                Icon = _assetManager.Get<TextureAsset>(texture)
            };
            button.Click += (sender, e) =>
            {
                _loadedGame.Playing = true;
                button.Toggled = true;
                _workspaceContainer.ActivateWhere(x => x is GameEditorWindow);
            };
            return button;
        }

        private Button CreatePauseButton(string texture)
        {
            var button = new Button
            {
                Icon = _assetManager.Get<TextureAsset>(texture)
            };
            button.Click += (sender, e) =>
            {
                if (_loadedGame.State == LoadedGameState.Playing)
                {
                    _loadedGame.Playing = false;
                    button.Toggled = true;
                }
                else if (_loadedGame.State == LoadedGameState.Paused)
                {
                    _loadedGame.Playing = true;
                    button.Toggled = false;
                }
            };
            return button;
        }

        private Button CreateStopButton(string texture)
        {
            var button = new Button
            {
                Icon = _assetManager.Get<TextureAsset>(texture)
            };
            button.Click += (sender, e) =>
            {
                if (_loadedGame.State == LoadedGameState.Playing ||
                    _loadedGame.State == LoadedGameState.Paused)
                {
                    _loadedGame.Restart();
                    _workspaceContainer.ActivateWhere(x => x is WorldEditorWindow);
                }
            };
            return button;
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
                renderContext.GraphicsDevice.Clear(new Color(162, 162, 162, 255));
            }
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            _mainMenuController.Update(gameContext, updateContext);

            _playButton.Toggled = _loadedGame.State == LoadedGameState.Playing || _loadedGame.State == LoadedGameState.Paused;
            _pauseButton.Toggled = _loadedGame.State == LoadedGameState.Paused;

            _playButton.Enabled = _projectManager.Project != null && _loadedGame.State != LoadedGameState.Loading;
            _pauseButton.Enabled = _loadedGame.State == LoadedGameState.Playing || _loadedGame.State == LoadedGameState.Paused;
            _stopButton.Enabled = _loadedGame.State == LoadedGameState.Playing || _loadedGame.State == LoadedGameState.Paused;

            foreach (var t in _toolButtons)
            {
                t.Enabled = _projectManager.Project != null;
            }

            if (_projectManager.Project != null)
            {
                _recentProjects.DisposeAllLoadedTextures();
            }

            gameContext.Window.Title = "Protogame 7.0.0 (" + (_projectManager?.Project?.Name ?? "<No Project>") + "; Build c510ef6)";
        }

        public IEnumerable<KeyValuePair<Canvas, Rectangle>> Canvases { get; }

        public bool CanvasesEnabled { get; }
    }
}
