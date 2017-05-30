using Microsoft.Xna.Framework;
using Protogame.Editor.Layout;
using Protogame.Editor.ProjectManagement;
using System.Collections.Generic;
using System.IO;
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

            var horizontalContainer = new HorizontalSpacedContainer();
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

            var selectedDefinition = _projectListView.SelectedItem as DefinitionListItem;
            
            if (selectedDefinition != null)
            {
                List<FileInfo> items = null;

                if (selectedDefinition.DefinitionInfo.Type == "Content")
                {
                    items = selectedDefinition.DefinitionInfo.ScannedContent;
                }
                else
                {

                }

                if (items == null)
                {
                    items = new List<FileInfo>();
                }

                foreach (var child in _projectContentView.Children.OfType<FileInfoListItem>().ToArray())
                {
                    if (!items.Contains(child.FileInfo))
                    {
                        _projectContentView.RemoveChild(child);
                    }
                }

                var currentItems = _projectContentView.Children.OfType<FileInfoListItem>().ToArray();

                foreach (var item in items)
                {
                    if (!currentItems.Any(x => x.FileInfo != item))
                    {
                        _projectContentView.AddChild(new FileInfoListItem(_assetManager, item));
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

                if (definitionInfo.Type == "Content")
                {
                    _icon = assetManager.Get<TextureAsset>("texture.IconImage");
                }
                else if (definitionInfo.Role == "Server")
                {
                    _icon = assetManager.Get<TextureAsset>("texture.IconServer");
                }
                else if (definitionInfo.Role == "Game")
                {
                    _icon = assetManager.Get<TextureAsset>("texture.IconGame");
                }
                else
                {
                    _icon = assetManager.Get<TextureAsset>("texture.IconCode");
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

        private class FileInfoListItem : ListItem
        {
            private IAssetReference<TextureAsset> _icon;

            public FileInfoListItem(IAssetManager assetManager, FileInfo fileInfo)
            {
                FileInfo = fileInfo;
            }

            public FileInfo FileInfo { get; }

            public override string Text
            {
                get
                {
                    return FileInfo.Name;
                }
                set
                {
                }
            }

            public override IAssetReference<TextureAsset> Icon
            {
                get
                {
                    return null;
                }
                set
                {
                }
            }
        }
    }
}
