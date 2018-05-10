using System;
using Rocket.API.DependencyInjection;

namespace Rocket.Eco.API
{
    /// <summary>
    ///     A small class to inherit from to allow easy access to an <see cref="IDependencyContainer" />.
    /// </summary>
    public abstract class ContainerAccessor
    {
        /// <summary>
        ///     A reference to the <see cref="IDependencyContainer" /> this instance was constructed from.
        /// </summary>
        protected readonly IDependencyContainer Container;

        /// <summary>
        ///     Initializes all the properties relating to <see cref="ContainerAccessor" />.
        /// </summary>
        protected ContainerAccessor(IDependencyContainer container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }
    }
}