using System;

using Rocket.API.Eventing;

namespace Rocket.Eco.Events
{
    public sealed class EcoInitEvent : Event
    {
        internal EcoInitEvent() : base(null, EventExecutionTargetContext.Sync, true) { }
    }
}
