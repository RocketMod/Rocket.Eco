using System;

using Rocket.API.Eventing;

using Rocket.Core.Events.Player;

namespace Rocket.Eco.Eventing
{
    public sealed class EcoEventListener : IEventListener<EcoInitEvent>, IEventListener<PlayerConnectEvent>, IEventListener<PlayerDisconnectEvent>
    {
        public EcoEventListener() { }

        public void HandleEvent(IEventEmitter emitter, EcoInitEvent @event)
        {
            
        }

        public void HandleEvent(IEventEmitter emitter, PlayerConnectEvent @event)
        {
            
        }

        public void HandleEvent(IEventEmitter emitter, PlayerDisconnectEvent @event)
        {
            
        }
    }
}
