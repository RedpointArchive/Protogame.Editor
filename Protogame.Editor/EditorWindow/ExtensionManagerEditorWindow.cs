using Microsoft.Xna.Framework;
using Protogame.Editor.Extension;
using System.Linq;

namespace Protogame.Editor.EditorWindow
{
    public class ExtensionManagerEditorWindow : EditorWindow
    {
        private readonly ScrollableContainer _scrollableContainer;
        private readonly VerticalContainer _verticalContainer;
        private readonly IExtensionManager _extensionManager;

        public ExtensionManagerEditorWindow(
            IAssetManager assetManager,
            IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager;

            Title = "Extensions";
            Icon = assetManager.Get<TextureAsset>("texture.IconTerminal");

            _verticalContainer = new VerticalContainer();

            //_scrollableContainer = new ScrollableContainer();
            //_scrollableContainer.SetChild(_verticalContainer);

            SetChild(_verticalContainer);
        }

        /*public override void OnFocus()
        {
            // Switch focus to the scrollable container.
            _verticalContainer.Focus();
        }*/

        public override void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            foreach (var ext in _extensionManager.Extensions)
            {
                if (_verticalContainer.Children.Any(x => x.Userdata == ext))
                {
                    continue;
                }

                var label = new Label
                {
                    Text = ext.Name
                };
                var debugButton = new Button
                {
                    Text = "Debug"
                };
                var restartButton = new Button
                {
                    Text = "Restart"
                };

                debugButton.Click += (sender, e) =>
                {
                    _extensionManager.DebugExtension(ext);
                };
                restartButton.Click += (sender, e) =>
                {
                    _extensionManager.RestartExtension(ext);
                };


                var buttonContainer = new VerticalContainer();
                buttonContainer.AddChild(debugButton, "24");
                buttonContainer.AddChild(restartButton, "24");

                var horizontalContainer = new HorizontalContainer();
                horizontalContainer.Userdata = ext;
                horizontalContainer.AddChild(label, "*");
                horizontalContainer.AddChild(buttonContainer, "120");

                _verticalContainer.AddChild(horizontalContainer, "48");
            }

            foreach (var vert in _verticalContainer.Children.ToArray())
            {
                if (!_extensionManager.Extensions.Any(x => x == vert.Userdata))
                {
                    _verticalContainer.RemoveChild(vert);
                }
            }

            base.Update(skinLayout, layout, gameTime, ref stealFocus);
        }
    }
}
