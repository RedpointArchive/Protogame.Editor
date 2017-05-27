using Protoinject;

using Protogame;
using Microsoft.Xna.Framework;

namespace Protogame.Editor
{
    public class ProtogameEditorGame : CoreGame<ProtogameEditorWorld>
    {
        public ProtogameEditorGame(IKernel kernel)
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
