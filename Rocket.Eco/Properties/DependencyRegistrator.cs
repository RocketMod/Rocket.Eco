using System;

using Rocket.IOC;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IPatchManager, PatchManager>();
            container.RegisterInstance<IEco>(resolver.Activate<Eco>());
        }
    }
}
