using Microsoft.Xna.Framework;
using Protogame.Editor.Layout;
using Protogame.Editor.ProjectManagement;
using System.Linq;

namespace Protogame.Editor.EditorWindow
{
    public class ProjectEditorWindow : EditorWindow
    {
        private readonly IAssetManager _assetManager;
        private readonly IProjectManager _projectManager;
        private readonly ListView _projectListView;
        private readonly ListView _projectContentView;

        public ProjectEditorWindow(
            IAssetManager assetManager,
            IProjectManager projectManager)
        {
            _assetManager = assetManager;
            _projectManager = projectManager;

            Title = "Project";
            Icon = _assetManager.Get<TextureAsset>("texture.IconFolder");

            _projectListView = new ListView();
            _projectContentView = new ListView();

            var scrollableProjectContainer = new ScrollableContainer();
            var scrollableContentContainer = new ScrollableContainer();

            scrollableProjectContainer.SetChild(_projectListView);
            scrollableContentContainer.SetChild(_projectContentView);

            var horizontalContainer = new HorizontalContainer();
            horizontalContainer.AddChild(scrollableProjectContainer, "350");
            horizontalContainer.AddChild(scrollableContentContainer, "*");

            var toolbarContainer = new ToolbarContainer();
            toolbarContainer.SetChild(horizontalContainer);

            SetChild(toolbarContainer);
        }

        public override void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            base.Update(skinLayout, layout, gameTime, ref stealFocus);

            if (_projectManager.Project.Definitions != null)
            {
                var definitions = _projectManager.Project.Definitions.ToList();

                foreach (var child in _projectListView.Children.OfType<DefinitionListItem>().ToArray())
                {
                    if (!definitions.Contains(child.DefinitionInfo))
                    {
                        _projectListView.RemoveChild(child);
                    }
                }

                var currentItems = _projectListView.Children.OfType<DefinitionListItem>().ToArray();

                foreach (var definition in definitions)
                {
                    if (!currentItems.Any(x => x.DefinitionInfo != definition))
                    {
                        _projectListView.AddChild(new DefinitionListItem(_assetManager, definition));
                    }
                }
            }
        }

        private class DefinitionListItem : ListItem
        {
            private IAssetReference<TextureAsset> _icon;

            public DefinitionListItem(IAssetManager assetManager, IDefinitionInfo definitionInfo)
            {
                DefinitionInfo = definitionInfo;

                switch (definitionInfo.LoadedDocument.DocumentElement.Name)
                {
                    case "ContentProject":
                        _icon = assetManager.Get<TextureAsset>("texture.IconImage");
                        break;
                    default:
                        _icon = assetManager.Get<TextureAsset>("texture.IconCode");
                        break;
                }
            }

            public IDefinitionInfo DefinitionInfo { get; }

            public override string Text
            {
                get
                {
                    return DefinitionInfo.Name;
                }
                set
                {
                }
            }

            public override IAssetReference<TextureAsset> Icon
            {
                get
                {
                    return _icon;
                }
                set
                {
                }
            }
        }
    }
}
