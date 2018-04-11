using System;

using Rocket.API.Eventing;

using Rocket.Core.Events.Player;
using Rocket.Core.Events.Plugins;

namespace Rocket.Eco.Eventing
{
    public sealed class EcoEventListener : IEventListener<EcoInitEvent>, IEventListener<PlayerConnectEvent>, IEventListener<PlayerDisconnectEvent>, IEventListener<PluginManagerLoadEvent>
    {
        internal EcoEventListener() { }

        public void HandleEvent(IEventEmitter emitter, EcoInitEvent @event)
        {
            
        }

        public void HandleEvent(IEventEmitter emitter, PlayerConnectEvent @event)
        {
            
        }

        public void HandleEvent(IEventEmitter emitter, PlayerDisconnectEvent @event)
        {
            
        }

        public void HandleEvent(IEventEmitter emitter, PluginManagerLoadEvent @event)
        {
            
        }
    }
}
