using System;

using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;

using Rocket.Eco.Commands;
using Rocket.Eco.Player;
using Rocket.Eco.Patching;
using Rocket.Core.Events.Implementation;

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
