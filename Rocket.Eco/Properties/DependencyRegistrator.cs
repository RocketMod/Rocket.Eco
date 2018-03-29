using System;

using Rocket.API;
using Rocket.API.DependencyInjection;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonInstance<IPatchManager>(new PatchManager());
            container.RegisterSingletonInstance<IImplementation>(resolver.Activate<Eco>());
        }
    }
}
