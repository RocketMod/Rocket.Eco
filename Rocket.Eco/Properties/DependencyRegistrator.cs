using System;

using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Ioc;
using Rocket.API.Player;

using Rocket.Eco.Commands;
using Rocket.Eco.Player;
using Rocket.Eco.Patching;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterType<IPlayerManager, EcoPlayerManager>();
            
            container.RegisterSingletonType<ICommandProvider, EcoCommandProvider>("ecocommandprovider");
            container.RegisterSingletonType<IPatchManager, PatchManager>();
            container.RegisterSingletonType<IImplementation, Eco>();
        }
    }
}
