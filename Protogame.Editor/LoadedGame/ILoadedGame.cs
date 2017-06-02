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

        void UpdateGame(IGameContext gameContext, IUpdateContext updateContext);

        void RenderGame(IGameContext gameContext, IRenderContext renderContext);

        void SetRenderTargetSize(Point size);

        Point? GetRenderTargetSize();

        Texture2D GetGameRenderTarget();

        void QueueEvent(Event @event);
    }
}
