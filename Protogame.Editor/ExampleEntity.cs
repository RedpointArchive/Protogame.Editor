using System;
using Protogame;
using Microsoft.Xna.Framework;

namespace ProtogameUIStylingTest
{
    public class ExampleEntity : Entity
    {
        private readonly string _name;

        private readonly I2DRenderUtilities _renderUtilities;

        private readonly IAssetReference<FontAsset> _defaultFont;

        public ExampleEntity(I2DRenderUtilities renderUtilities, IAssetManager assetManager, string name)
        {
            _renderUtilities = renderUtilities;
            _name = name;
            _defaultFont = assetManager.Get<FontAsset>("font.Default");
        }

        public override void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            base.Render(gameContext, renderContext);

            if (renderContext.IsCurrentRenderPass<I2DBatchedRenderPass>())
            {
                _renderUtilities.RenderText(
                    renderContext,
                    new Vector2(this.FinalTransform.AbsolutePosition.X, this.FinalTransform.AbsolutePosition.Y),
                    _name,
                    _defaultFont);
            }
        }
    }
}
