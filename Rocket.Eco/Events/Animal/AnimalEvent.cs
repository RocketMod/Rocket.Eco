#if DEBUG
using Rocket.Core.Eventing;
using Rocket.Eco.Entity;

namespace Rocket.Eco.Events.Animal
{
    public abstract class AnimalEvent : Event
    {
        public EcoAnimal Animal { get; }

        protected AnimalEvent(EcoAnimal animal) : base(true)
        {
            Animal = animal;
        }
    }
}
#endif