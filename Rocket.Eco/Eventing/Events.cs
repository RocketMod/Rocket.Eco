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

    public sealed class PlayerJoinEvent : Event
    {
        public EcoPlayer Player { get; }

        internal PlayerJoinEvent(EcoPlayer player) : base(null, EventExecutionTargetContext.Sync, true)
        {
            Player = player;
        }
    }

    public sealed class PlayerLeaveEvent : Event
    {
        public EcoPlayer Player { get; }

        internal PlayerLeaveEvent(EcoPlayer player) : base(null, EventExecutionTargetContext.Sync, true)
        {
            Player = player;
        }
    }
}
