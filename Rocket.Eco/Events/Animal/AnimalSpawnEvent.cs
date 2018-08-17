#if DEBUG
using Rocket.API.Eventing;
using Rocket.Eco.Entity;

namespace Rocket.Eco.Events.Animal
{
    public sealed class AnimalSpawnEvent : AnimalEvent, ICancellableEvent
    {
        public AnimalSpawnEvent(EcoAnimal animal) : base(animal) { }

        public bool IsCancelled { get; set; }
    }
}
#endif