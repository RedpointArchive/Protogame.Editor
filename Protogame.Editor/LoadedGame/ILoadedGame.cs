using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Protogame.Editor.LoadedGame
{
    public interface ILoadedGame
    {
        void Update(IGameContext gameContext, IUpdateContext updateContext);

        void Render(IGameContext gameContext, IRenderContext renderContext);

        void IncrementReadRenderTargetIfPossible();

        RenderTarget2D GetCurrentGameRenderTarget();

        void SetPositionOffset(Point offset);

        void SetRenderTargetSize(Point size);

        Point? GetRenderTargetSize();

        void QueueEvent(Event @event);

        string GetBaseDirectory();

        /*
        LoadedGameState State { get; }

        bool Playing { get; set; }

        TimeSpan PlayingFor { get; }

        void Restart();

        Tuple<bool, bool> GetStallState();
        */
    }
}
