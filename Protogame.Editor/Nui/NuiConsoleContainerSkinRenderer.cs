using System;
using Microsoft.Xna.Framework;
using Protogame;
using Protogame.Editor.Layout;

namespace Protogame.Editor.Nui
{
    public class NuiConsoleContainerSkinRenderer : ISkinRenderer<ConsoleContainer>
    {
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly IAssetManager _assetManager;
        private readonly IAssetReference<FontAsset> _fontAsset;

        public NuiConsoleContainerSkinRenderer(
            I2DRenderUtilities renderUtilities,
            IAssetManager assetManager)
        {
            _renderUtilities = renderUtilities;
            _assetManager = assetManager;
            _fontAsset = assetManager.Get<FontAsset>("font.Console");
        }

        public void Render(IRenderContext renderContext, Rectangle layout, ConsoleContainer container)
        {
            _renderUtilities.RenderRectangle(renderContext, layout, new Color(0, 0, 0, 255), true);

            if (container.Console == null)
            {
                return;
            }

            var entries = container.Console.Entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var color = Color.White;
                switch (entries[i].LogLevel)
                {
                    case ConsoleLogLevel.Debug:
                        color = Color.White;
                        break;
                    case ConsoleLogLevel.Info:
                        color = Color.Cyan;
                        break;
                    case ConsoleLogLevel.Warning:
                        color = Color.Orange;
                        break;
                    case ConsoleLogLevel.Error:
                        color = Color.Red;
                        break;
                }

                var x = entries[i];
                var message = x.Name == string.Empty ? x.Message : $"<{x.Name,-20}> ({x.Count,5}) {x.Message}";

                _renderUtilities.RenderText(
                    renderContext,
                    new Vector2(layout.X + 2, layout.Y + i * 16),
                    message,
                    _fontAsset,
                    textColor: color,
                    renderShadow: false);
            }
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, ConsoleContainer container)
        {
            throw new NotSupportedException();
        }
    }
}
