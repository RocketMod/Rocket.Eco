using System;

using Eco.Gameplay.Players;

using Rocket.API.Eventing;

using Rocket.Eco.Player;

namespace Rocket.Eco.Eventing
{
    public sealed class EcoInitEvent : Event
    {
        internal EcoInitEvent() : base(null, EventExecutionTargetContext.Sync, true) { }
    }
}
