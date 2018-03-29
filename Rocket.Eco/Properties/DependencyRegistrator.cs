using System;

using Rocket.API.IOC;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IPatchManager, PatchManager>();
            container.RegisterSingletonInstance<IEco>(resolver.Activate<Eco>());
        }
    }
}
