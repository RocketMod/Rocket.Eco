#if DEBUG
using System.Numerics;
using Eco.Simulation;
using Rocket.API.Entities;
using Eco.Simulation.Agents;
using Rocket.API.User;
using Rocket.Eco.Extensions;

namespace Rocket.Eco.Entity
{
    public sealed class EcoAnimal : ILivingEntity
    {
        /// <summary>
        ///     The internal Eco represntation of the animal attached to this object.
        /// </summary>
        public Animal InternalAnimal { get; }

        /// <summary>
        ///     The ID given by Eco's AnimalSimulation manager.
        ///     This is guaranteed to be unique.
        /// </summary>
        public int AnimalId => InternalAnimal.ID;

        /// <inheritdoc />
        public string EntityTypeName => InternalAnimal.Species.Name;

        /// <inheritdoc />
        public Vector3 Position => InternalAnimal.Position.ToSystemVector3();

        /// <inheritdoc />
        public void Kill()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Kill(IEntity killer)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Kill(IUser killer)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public double MaxHealth => InternalAnimal.Species.Health;

        /// <inheritdoc />
        public double Health
        {
            get => InternalAnimal.Health;

            set
            {
                InternalAnimal.GetType().GetProperty("Health").SetValue(InternalAnimal, (float)value);

                if (InternalAnimal.Health <= 0)
                    InternalAnimal.Kill(DeathType.None);
            }
        }

        internal EcoAnimal(Animal animal)
        {
            InternalAnimal = animal;
        }
    }
}
#endif