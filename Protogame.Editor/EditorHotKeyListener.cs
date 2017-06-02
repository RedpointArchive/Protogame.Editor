using System;
using Protogame.Editor.LoadedGame;

namespace Protogame.Editor
{
    public class EditorHotKeyListener : IEventListener<IGameContext>
    {
        private readonly ILoadedGame _loadedGame;

        public EditorHotKeyListener(ILoadedGame loadedGame)
        {
            _loadedGame = loadedGame;
        }

        public bool Handle(IGameContext context, IEventEngine<IGameContext> eventEngine, Event @event)
        {
            if (_loadedGame.State == LoadedGameState.Paused ||
                _loadedGame.State == LoadedGameState.Playing)
            {
                _loadedGame.Playing = !_loadedGame.Playing;
            }

            return true;
        }
    }
}