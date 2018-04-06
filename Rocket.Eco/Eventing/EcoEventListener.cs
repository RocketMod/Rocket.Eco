using System;

using Rocket.API.Eventing;

namespace Rocket.Eco.Eventing
{
    public sealed class EcoEventListener : IEventListener<EcoInitEvent>, IEventListener<PlayerJoinEvent>, IEventListener<PlayerLeaveEvent>
    {
        internal EcoEventListener() { }

        public void HandleEvent(IEventEmitter emitter, EcoInitEvent @event)
        {
            
        }

        public void HandleEvent(IEventEmitter emitter, PlayerJoinEvent @event)
        {
            
        }

        public void HandleEvent(IEventEmitter emitter, PlayerLeaveEvent @event)
        {
            
        }
    }
}
