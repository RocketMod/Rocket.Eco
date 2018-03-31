using System;

using Rocket.API;
using Rocket.API.DependencyInjection;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IPatchManager, PatchManager>();
            container.RegisterSingletonType<IImplementation, Eco>();
        }
    }
}
