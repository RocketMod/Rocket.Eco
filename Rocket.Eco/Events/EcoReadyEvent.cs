using System;

using Rocket.Core.Events.Implementation;

namespace Rocket.Eco.Events
{
    public sealed class EcoReadyEvent : ImplementationReadyEvent
    {
        internal EcoReadyEvent(EcoImplementation implementation) : base(implementation) { }
    }
}
