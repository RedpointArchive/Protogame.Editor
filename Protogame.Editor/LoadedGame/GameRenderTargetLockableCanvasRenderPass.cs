namespace Protogame.Editor.LoadedGame
{
    public class GameRenderTargetLockableCanvasRenderPass : DefaultCanvasRenderPass
    {
        private readonly ILoadedGame _loadedGame;

        public GameRenderTargetLockableCanvasRenderPass(
            IBackBufferDimensions backBufferDimensions,
            ILoadedGame loadedGame) : base(backBufferDimensions)
        {
            _loadedGame = loadedGame;
        }

        public override void EndRenderPass(IGameContext gameContext, IRenderContext renderContext, IRenderPass nextPass)
        {
            // We must acquire a lock to render the game texture.
            var renderTarget = _loadedGame.GetCurrentGameRenderTarget();
            var didAcquire = false;
            if (renderTarget != null)
            {
                didAcquire = renderTarget.AcquireLock(1234, 1000000);
            }

            base.EndRenderPass(gameContext, renderContext, nextPass);

            if (didAcquire)
            {
                renderTarget.ReleaseLock(1234);
            }

            _loadedGame.IncrementReadRenderTargetIfPossible();
        }
    }
}
