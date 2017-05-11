namespace ProtogameUIStylingTest
{
    using Protoinject;

    using Protogame;
    using Microsoft.Xna.Framework;

    public class ProtogameUIStylingTestGame : CoreGame<ProtogameUIStylingTestWorld>
    {
        public ProtogameUIStylingTestGame(IKernel kernel)
            : base(kernel)
        {
        }

        public override void PrepareGameWindow(IGameWindow window)
        {
            IsMouseVisible = true;
        }

        protected override void ConfigureRenderPipeline(IRenderPipeline pipeline, IKernel kernel)
        {
            pipeline.AddFixedRenderPass(kernel.Get<ICanvasRenderPass>());
        }
    }
}
