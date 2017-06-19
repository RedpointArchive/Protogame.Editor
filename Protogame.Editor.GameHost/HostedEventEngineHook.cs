using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Protogame.Editor.GameHost
{
    public class HostedEventEngineHook : IEngineHook
    {
        private readonly ConcurrentQueue<Event> _queuedEvents;
        private readonly IEventEngine<IGameContext> _eventEngine;

        public HostedEventEngineHook(IEventEngine<IGameContext> eventEngine)
        {
            _queuedEvents = new ConcurrentQueue<Event>();
            _eventEngine = eventEngine;
        }
        
        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            Event e;
            while (_queuedEvents.TryDequeue(out e))
            {
                _eventEngine.Fire(gameContext, e);
            }
        }

        public void Update(IServerContext serverContext, IUpdateContext updateContext)
        {
        }

        public void QueueEvent(Event @event)
        {
            _queuedEvents.Enqueue(@event);
        }
    }
}