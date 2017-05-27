using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Protogame;

namespace Protogame.Editor.Nui
{
    public class NuiCheckBoxSkinRenderer : ISkinRenderer<CheckBox>
    {
        private readonly I2DRenderUtilities _renderUtilities;
        private readonly Dictionary<string, IAssetReference<TextureAsset>> _textures;

        public NuiCheckBoxSkinRenderer(I2DRenderUtilities renderUtilities, IAssetManager assetManager)
        {
            _renderUtilities = renderUtilities;
            _textures = new Dictionary<string, IAssetReference<TextureAsset>>
            {
                { "Unticked", assetManager.Get<TextureAsset>("texture.UICheckboxUnticked") },
                { "UntickedDown", assetManager.Get<TextureAsset>("texture.UICheckboxUntickedDown") },
                { "UntickedFocused", assetManager.Get<TextureAsset>("texture.UICheckboxUntickedFocused") },
                { "UntickedFocusedDown", assetManager.Get<TextureAsset>("texture.UICheckboxUntickedFocusedDown") },
                { "Ticked", assetManager.Get<TextureAsset>("texture.UICheckboxTicked") },
                { "TickedDown", assetManager.Get<TextureAsset>("texture.UICheckboxTickedDown") },
                { "TickedFocused", assetManager.Get<TextureAsset>("texture.UICheckboxTickedFocused") },
                { "TickedFocusedDown", assetManager.Get<TextureAsset>("texture.UICheckboxTickedFocusedDown") },
            };
        }

        public void Render(IRenderContext renderContext, Rectangle layout, CheckBox checkBox)
        {
            var textureName = checkBox.Checked ? "Ticked" : "Unticked";
            if (checkBox.Focused)
            {
                textureName += "Focused";
            }
            if (checkBox.IsDown)
            {
                textureName += "Down";
            }

            var texture = _textures[textureName];
            if (!texture.IsReady)
            {
                return;
            }

            _renderUtilities.RenderTexture(
                renderContext,
                new Vector2(
                    layout.X + layout.Width / 2 - texture.Asset.OriginalWidth / 2,
                    layout.Y + layout.Height / 2 - texture.Asset.OriginalHeight / 2),
                texture);
        }

        public Vector2 MeasureText(IRenderContext renderContext, string text, CheckBox container)
        {
            throw new NotSupportedException();
        }
    }
}
