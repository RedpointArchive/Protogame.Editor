using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Protogame.Editor.LoadedGame
{
    public interface ILoadedGame
    {
        LoadedGameState State { get; }

        bool Playing { get; set; }

        void Restart();

        void Update(IGameContext gameContext, IUpdateContext updateContext);

        void Render(IGameContext gameContext, IRenderContext renderContext);

        void SetPositionOffset(Point offset);

        void SetRenderTargetSize(Point size);

        Point? GetRenderTargetSize();

        void IncrementReadRenderTargetIfPossible();

        RenderTarget2D GetCurrentGameRenderTarget();

        void QueueEvent(Event @event);
    }
}
