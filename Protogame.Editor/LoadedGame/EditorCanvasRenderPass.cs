using Microsoft.Xna.Framework.Graphics;
using Protogame.Editor.ProjectManagement;

namespace Protogame.Editor.LoadedGame
{
    public class EditorCanvasRenderPass : DefaultCanvasRenderPass
    {
        private readonly ILoadedGame _loadedGame;
        private readonly IThumbnailSampler _thumbnailSampler;

        public EditorCanvasRenderPass(
            IBackBufferDimensions backBufferDimensions,
            IInterlacedBatchingDepthProvider interlacedBatchingDepthProvider,
            ILoadedGame loadedGame,
            IThumbnailSampler thumbnailSampler) : base(backBufferDimensions, interlacedBatchingDepthProvider)
        {
            _loadedGame = loadedGame;
            _thumbnailSampler = thumbnailSampler;
        }

        public override void BeginRenderPass(IGameContext gameContext, IRenderContext renderContext, IRenderPass previousPass, RenderTarget2D postProcessingSource)
        {
            base.BeginRenderPass(gameContext, renderContext, previousPass, postProcessingSource);
        }

        public override void EndRenderPass(IGameContext gameContext, IRenderContext renderContext, IRenderPass nextPass)
        {
            var renderTarget = _loadedGame.GetCurrentGameRenderTarget();
            var didAcquire = false;
            if (renderTarget != null)
            {
                didAcquire = renderTarget.AcquireLock(1234, 1);
            }

            base.EndRenderPass(gameContext, renderContext, nextPass);

            _thumbnailSampler.WriteThumbnailIfNecessary(gameContext, renderContext);

            if (didAcquire)
            {
                renderTarget.ReleaseLock(1234);
            }

            _loadedGame.IncrementReadRenderTargetIfPossible();
        }
    }
}
