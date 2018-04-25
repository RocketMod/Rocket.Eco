using Rocket.API.Eventing;
using Rocket.Core.Implementation.Events;

namespace Rocket.Eco.Events
{
    public sealed class EcoReadyEvent : ImplementationReadyEvent
    {
        internal EcoReadyEvent(EcoImplementation implementation) : base(implementation, EventExecutionTargetContext.NextFrame) { }
    }
}