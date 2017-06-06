using Microsoft.Xna.Framework;
using Protogame.Editor.Api.Version1.ProjectManagement;
using Protogame.Editor.Layout;
using Protogame.Editor.ProjectManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Protogame.Editor.EditorWindow
{
    public class StartEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly TreeView _hierarchyTreeView;
        private readonly IProjectManager _projectManager;
        private Task<List<RecentProject>> _recentProjectsTask;
        private readonly ICoroutine _coroutine;
        private readonly IRecentProjects _recentProjects;
        private readonly VerticalContainer _recentProjectsContainer;
        private bool _hasAppliedRecentProjects;
        private readonly I2DRenderUtilities _renderUtilities;

        public StartEditorWindow(
            IAssetManager assetManager,
            IProjectManager projectManager,
            IProjectManagerUi projectManagerUi,
            ICoroutine coroutine,
            IRecentProjects recentProjects,
            I2DRenderUtilities renderUtilities)
        {
            _assetManager = assetManager;
            _projectManager = projectManager;
            _coroutine = coroutine;
            _recentProjects = recentProjects;
            _renderUtilities = renderUtilities;

            Title = "Welcome!";
            Icon = _assetManager.Get<TextureAsset>("texture.IconWelcome");

            var newProject = new Button
            {
                Text = "New Project"
            };
            var openProject = new Button
            {
                Text = "Open Project"
            };

            openProject.Click += (sender, e) =>
            {
                _coroutine.Run(() => projectManagerUi.LoadProject());
            };

            var buttonContainer = new HorizontalContainer();
            buttonContainer.AddChild(newProject, "*");
            buttonContainer.AddChild(new EmptyContainer(), "20");
            buttonContainer.AddChild(openProject, "*");

            _recentProjectsContainer = new VerticalContainer();
            //var scrollableContainer = new ScrollableContainer();
            //scrollableContainer.SetChild(_recentProjectsContainer);

            var verticalContainer = new VerticalContainer();
            verticalContainer.AddChild(new Label {
                Font = _assetManager.Get<FontAsset>("font.Welcome"),
                Text = "Welcome to Protogame!",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            }, "80");
            verticalContainer.AddChild(buttonContainer, "60");
            verticalContainer.AddChild(new EmptyContainer(), "24");
            verticalContainer.AddChild(_recentProjectsContainer, "*");

            var outerVertContainer = new VerticalContainer();
            outerVertContainer.AddChild(new EmptyContainer(), "15%");
            outerVertContainer.AddChild(verticalContainer, "*");
            outerVertContainer.AddChild(new EmptyContainer(), "15%");

            var outerHorContainer = new HorizontalContainer();
            outerHorContainer.AddChild(new EmptyContainer(), "15%");
            outerHorContainer.AddChild(outerVertContainer, "*");
            outerHorContainer.AddChild(new EmptyContainer(), "15%");

            SetChild(outerHorContainer);
        }

        public override void Render(IRenderContext context, ISkinLayout skinLayout, ISkinDelegator skinDelegator, Rectangle layout)
        {
            if (_recentProjectsTask == null)
            {
                _recentProjectsTask = _coroutine.Run(() => _recentProjects.GetRecentProjects(context));
            }

            if (_recentProjectsTask.IsCompleted && !_hasAppliedRecentProjects)
            {
                foreach (var recentProject in _recentProjectsTask.Result)
                {
                    var horizontalContainer = new ClickableHorizontalContainer();
                    if (recentProject.Thumbnail != null)
                    {
                        var textureContainer = new RawTextureContainer(_renderUtilities);
                        textureContainer.Texture = recentProject.Thumbnail;
                        textureContainer.TextureFit = "stretch";
                        horizontalContainer.AddChild(textureContainer, "60");
                    }
                    else
                    {
                        horizontalContainer.AddChild(new EmptyContainer(), "60");
                    }

                    var verticalContainer = new VerticalContainer();
                    verticalContainer.AddChild(new EmptyContainer(), "*");
                    verticalContainer.AddChild(new Label { Text = recentProject.Name, HorizontalAlignment = HorizontalAlignment.Left }, "16");
                    verticalContainer.AddChild(new EmptyContainer(), "*");
                    verticalContainer.AddChild(new Label { Text = recentProject.Path, HorizontalAlignment = HorizontalAlignment.Left, TextColor = new Color(63, 63, 63, 255) }, "16");
                    verticalContainer.AddChild(new EmptyContainer(), "*");

                    var button = new Button
                    {
                        Text = "Load",
                    };
                    button.Click += (sender, e) =>
                    {
                        _projectManager.LoadProject(recentProject.Path);
                    };
                    horizontalContainer.Click += (sender, e) =>
                    {
                        _projectManager.LoadProject(recentProject.Path);
                    };

                    horizontalContainer.AddChild(new EmptyContainer(), "16");
                    horizontalContainer.AddChild(verticalContainer, "*");
                    horizontalContainer.AddChild(new EmptyContainer(), "16");
                    horizontalContainer.AddChild(button, "60");

                    _recentProjectsContainer.AddChild(horizontalContainer, "60");
                    _recentProjectsContainer.AddChild(new EmptyContainer(), "10");
                }

                _hasAppliedRecentProjects = true;
            }

            base.Render(context, skinLayout, skinDelegator, layout);
        }

        public override bool Visible
        {
            get
            {
                return _projectManager.Project == null;
            }
            set { }
        }
    }
}
