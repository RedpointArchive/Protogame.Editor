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
using Protogame.Editor.Extension;
using System.Diagnostics;
using Protogame.Editor.Toolbar;
using System.Linq;

namespace Protogame.Editor
{
    public class ProtogameEditorWorld : IWorld, IHasCanvases
    {
        private readonly NuiRenderer _nuiRenderer;
        private Canvas _canvas;
        private ISkinLayout _skinLayout;
        private ISkinDelegator _skinDelegator;
        private IAssetManager _assetManager;
        //private List<Button> _toolButtons = new List<Button>();
        private readonly IMainMenuController _mainMenuController;
        private readonly IEditorWindowFactory _editorWindowFactory;
        private readonly IProjectManager _projectManager;
        private readonly ILoadedGame _loadedGame;
        /*private Button _vsButton;
        private Button _debugButton;
        private Button _debugGpuButton;
        private Button _playButton;
        private Button _pauseButton;
        private Button _stopButton;*/
        private DockableLayoutContainer _workspaceContainer;
        private WorldEditorWindow _worldEditorWindow;
        private GameEditorWindow _gameEditorWindow;
        private readonly IRecentProjects _recentProjects;
        private readonly IThumbnailSampler _thumbnailSampler;
        private readonly IExtensionManager _extensionManager;
        private readonly IWindowManagement _windowManagement;
        private RelativeContainer _gameControlContainer;
        private readonly IToolbarProvider[] _toolbarProviders;
        private HorizontalContainer _horizontalContainer;

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
            IThumbnailSampler thumbnailSampler,
            IExtensionManager extensionManager,
            IWindowManagement windowManagement,
            IToolbarProvider[] toolbarProviders)
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
            _extensionManager = extensionManager;
            _windowManagement = windowManagement;
            _toolbarProviders = toolbarProviders;

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

            var toolContainer = new RelativeContainer();
            /*var panButton = CreateToolButton("texture.IconToolPan", "pan");
            panButton.Toggled = true;

            toolContainer.AddChild(panButton, new Rectangle(16, 8, 28, 28));
            toolContainer.AddChild(CreateToolButton("texture.IconToolMove", "move"), new Rectangle(16 + 30 * 1, 8, 28, 28));
            toolContainer.AddChild(CreateToolButton("texture.IconToolRotate", "rotate"), new Rectangle(16 + 30 * 2, 8, 28, 28));
            toolContainer.AddChild(CreateToolButton("texture.IconToolResize", "resize"), new Rectangle(16 + 30 * 3, 8, 28, 28));
            toolContainer.AddChild(CreateToolButton("texture.IconToolSelect", "select"), new Rectangle(16 + 30 * 4, 8, 28, 28));*/

            _gameControlContainer = new RelativeContainer();

            var unusedContainer = new RelativeContainer();

            _horizontalContainer = new HorizontalContainer();
            _horizontalContainer.AddChild(toolContainer, "*");
            _horizontalContainer.AddChild(_gameControlContainer, "0");
            _horizontalContainer.AddChild(unusedContainer, "*");

            var verticalContainer = new VerticalContainer();
            verticalContainer.AddChild(_horizontalContainer, "40");
            verticalContainer.AddChild(dockableLayoutContainer, "*");

            _canvas.SetChild(verticalContainer);

            _windowManagement.SetMainDocumentContainer(_workspaceContainer);
        }

        /*
        private Button CreateVisualStudioButton(string texture)
        {
            var button = new Button
            {
                Icon = _assetManager.Get<TextureAsset>(texture)
            };
            button.Click += (sender, e) =>
            {
                // TODO: Need to find extension and call into it.
                //_loadedGame.RunInDebug();
                _workspaceContainer.ActivateWhere(x => x is GameEditorWindow);
            };
            return button;
        }

        private Button CreateDebugButton(string texture)
        {
            var button = new Button
            {
                Icon = _assetManager.Get<TextureAsset>(texture)
            };
            button.Click += (sender, e) =>
            {
            };
            return button;
        }

        private Button CreateDebugGpuButton(string texture)
        {
            var button = new Button
            {
                Icon = _assetManager.Get<TextureAsset>(texture)
            };
            button.Click += (sender, e) =>
            {
                _loadedGame.RunInDebugGpu();
                _workspaceContainer.ActivateWhere(x => x is GameEditorWindow);
            };
            return button;
        }

        private Button CreatePlayButton(string texture)
        {
            var button = new Button
            {
                Icon = _assetManager.Get<TextureAsset>(texture)
            };
            button.Click += (sender, e) =>
            {
                _loadedGame.SetPlaybackMode(true);
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
                if (_loadedGame.GetPlaybackState() == LoadedGameState.Playing)
                {
                    _loadedGame.SetPlaybackMode(false);
                    button.Toggled = true;
                }
                else if (_loadedGame.GetPlaybackState() == LoadedGameState.Paused)
                {
                    _loadedGame.SetPlaybackMode(true);
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
                if (_loadedGame.GetPlaybackState() == LoadedGameState.Playing ||
                    _loadedGame.GetPlaybackState() == LoadedGameState.Paused)
                {
                    _loadedGame.RequestRestart();
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
        */

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

            UpdateToolbar();

           /* var state = _loadedGame.GetPlaybackState();

            _playButton.Toggled = state == LoadedGameState.Playing || state == LoadedGameState.Paused;
            _pauseButton.Toggled = state == LoadedGameState.Paused;

            _playButton.Enabled = _projectManager.Project != null && state != LoadedGameState.Loading;
            _pauseButton.Enabled = state == LoadedGameState.Playing || state == LoadedGameState.Paused;
            _stopButton.Enabled = state == LoadedGameState.Playing || state == LoadedGameState.Paused;

            _vsButton.Enabled = _projectManager.Project != null;
            _debugButton.Enabled = _projectManager.Project != null && state == LoadedGameState.Loaded;
            _debugGpuButton.Enabled = _projectManager.Project != null && state == LoadedGameState.Loaded;*/

            /*foreach (var t in _toolButtons)
            {
                t.Enabled = _projectManager.Project != null;
            }*/

            if (_projectManager.Project != null)
            {
                _recentProjects.DisposeAllLoadedTextures();
            }

            gameContext.Window.Title = "Protogame 7.0.0 (" + (_projectManager?.Project?.Name ?? "<No Project>") + "; Build c510ef6)";

            _extensionManager.Update();
        }

        private void UpdateToolbar()
        {
            var recreateButtons = false;
            var existingButtons = _gameControlContainer.Children.OfType<Button>().ToList();
            var toolbarItems = _toolbarProviders.SelectMany(x => x.GetToolbarItems());
            foreach (var te in toolbarItems)
            {
                if (!existingButtons.Any(x => (long)x.Userdata == te.Id))
                {
                    // Button doesn't exist, need to recreate.
                    recreateButtons = true;
                    break;
                }
            }
            foreach (var eb in existingButtons)
            {
                if (!toolbarItems.Any(x => x.Id == (long)eb.Userdata))
                {
                    // Button doesn't exist, need to recreate.
                    recreateButtons = true;
                    break;
                }
            }

            if (!recreateButtons)
            {
                // Just sync properties.
                foreach (var eb in existingButtons)
                {
                    var toolbarItem = toolbarItems.First(x => x.Id == (long)eb.Userdata);
                    eb.Icon = _assetManager.Get<TextureAsset>(toolbarItem.Icon);
                    eb.Enabled = toolbarItem.Enabled;
                    eb.Toggled = toolbarItem.Toggled;
                }
            }
            else
            {
                // TODO: Optimize this to only change controls that need to be changed.
                foreach (var c in _gameControlContainer.Children.ToArray())
                {
                    _gameControlContainer.RemoveChild(c);
                }

                var size = 0;
                foreach (var tp in _toolbarProviders)
                {
                    if (size > 0)
                    {
                        size += 30;
                    }

                    foreach (var te in tp.GetToolbarItems())
                    {
                        var button = new Button
                        {
                            Icon = _assetManager.Get<TextureAsset>(te.Icon),
                            Enabled = te.Enabled,
                            Toggled = te.Toggled,
                            Userdata = te.Id
                        };
                        button.Click += (sender, e) =>
                        {
                            te.Handler?.Invoke(te);
                        };
                        _gameControlContainer.AddChild(button, new Rectangle(size, 8, 28, 28));
                        size += 30;
                    }
                }

                _horizontalContainer.SetChildSize(_gameControlContainer, (size - 2).ToString());
            }
        }

        private class ToolbarButtonUserdata
        {
            IToolbarProvider _toolbarProvider;
        }

        public IEnumerable<KeyValuePair<Canvas, Rectangle>> Canvases { get; }

        public bool CanvasesEnabled { get; }
    }
}
