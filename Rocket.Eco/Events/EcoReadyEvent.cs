using System;

using Rocket.Core.Events.Implementation;

namespace Rocket.Eco.Events
{
    public sealed class EcoReadyEvent : ImplementationReadyEvent
    {
        internal EcoReadyEvent() : base(Eco.Instance) { }
    }
}
