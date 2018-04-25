using System;
using Rocket.API.DependencyInjection;

namespace Rocket.Eco.API
{
    public abstract class ContainerAccessor
    {
        protected readonly IDependencyContainer Container;

        protected ContainerAccessor(IDependencyContainer container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }
    }
}