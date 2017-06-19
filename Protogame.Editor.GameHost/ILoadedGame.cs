using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Protogame.Editor.GameHost
{
    public interface ILoadedGame
    {
        LoadedGameState State { get; }

        bool Playing { get; set; }

        TimeSpan PlayingFor { get; }

        void Restart();

        void Update(IGameContext gameContext, IUpdateContext updateContext);

        void Render(IGameContext gameContext, IRenderContext renderContext);

        void SetPositionOffset(Point offset);

        void SetRenderTargetSize(Point size);

        Point? GetRenderTargetSize();

        void IncrementReadRenderTargetIfPossible();

        RenderTarget2D GetCurrentGameRenderTarget();

        void QueueEvent(Event @event);

        Tuple<bool, bool> GetStallState();
    }
}
