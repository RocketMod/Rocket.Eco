#if DEBUG
using Rocket.Eco.Entity;

namespace Rocket.Eco.Events.Animal
{
    public sealed class AnimalDeathEvent : AnimalEvent
    {
        public AnimalDeathEvent(EcoAnimal animal) : base(animal) { }
    }
}
#endif