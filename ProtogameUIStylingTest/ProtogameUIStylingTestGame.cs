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
            window.AllowUserResizing = true;
            window.Title = "Protogame 7.0.0 (Build c510ef6)";
            window.Maximize();
        }

        protected override void ConfigureRenderPipeline(IRenderPipeline pipeline, IKernel kernel)
        {
            pipeline.AddFixedRenderPass(kernel.Get<ICanvasRenderPass>());
        }
    }
}
