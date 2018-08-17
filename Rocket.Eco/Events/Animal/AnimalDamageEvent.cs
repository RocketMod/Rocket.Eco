#if DEBUG
using Rocket.API.Eventing;
using Rocket.Eco.Entity;

namespace Rocket.Eco.Events.Animal
{
    public sealed class AnimalDamageEvent : AnimalEvent, ICancellableEvent
    {
        public bool IsCancelled { get; set; }

        public float Damage { get; set; }

        public float CurrentHealth => (float)Animal.Health;

        public AnimalDamageEvent(EcoAnimal animal, float damage) : base(animal)
        {
            Damage = damage;
        }
    }
}
#endif