using System.Collections.Generic;

namespace Protogame.Editor.GameHost
{
    public class EditorEventEngineHook : IEngineHook
    {
        private readonly List<Event> _queuedEvents;
        private readonly IEventEngine<IGameContext> _eventEngine;

        public EditorEventEngineHook(IEventEngine<IGameContext> eventEngine)
        {
            _queuedEvents = new List<Event>();
            _eventEngine = eventEngine;
        }
        
        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            foreach (var e in _queuedEvents)
            {
                _eventEngine.Fire(gameContext, e);
            }

            _queuedEvents.Clear();
        }

        public void Update(IServerContext serverContext, IUpdateContext updateContext)
        {
        }

        public void QueueEvent(Event @event)
        {
            _queuedEvents.Add(@event);
        }
    }
}