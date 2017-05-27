using Protogame.Editor.Layout;

namespace Protogame.Editor.EditorWindow
{
    public class ConsoleEditorWindow : EditorWindow
    {
        private readonly IConsole _console;
        private readonly ScrollableContainer _scrollableContainer;

        public ConsoleEditorWindow(
            IAssetManager assetManager,
            IConsole console)
        {
            _console = console;

            Title = "Console";
            Icon = assetManager.Get<TextureAsset>("texture.IconTerminal");

            var consoleContainer = new ConsoleContainer { Console = console as EditorConsole };

            _scrollableContainer = new ScrollableContainer();
            _scrollableContainer.SetChild(consoleContainer);

            SetChild(_scrollableContainer);
        }

        public override void OnFocus()
        {
            // Switch focus to the scrollable container.
            _scrollableContainer.Focus();
        }
    }
}
