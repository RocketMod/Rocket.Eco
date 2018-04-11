using System;
using System.Linq;
using System.Reflection;
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
            if (!Eco.isExtraction)
            {
                AppDomain.CurrentDomain.GetAssemblies()
                    .First(x => x.GetName().Name.Equals("EcoServer"))
                    .GetType("Eco.Server.Startup")
                    .GetMethod("Start", BindingFlags.Static | BindingFlags.Public)
                    .Invoke(null, new object[] { Eco.launchArgs });
            }
        }
    }
}
