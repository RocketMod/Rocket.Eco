using System;

using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.Player;
using Rocket.API.Plugin;

using Rocket.Core.Eventing;

using Rocket.Eco.Commands;
using Rocket.Eco.Player;
using Rocket.Eco.Plugins;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterType<IPlayerManager, EcoPlayerManager>();

            container.RegisterSingletonInstance<IEventManager>(container.Activate<EventManager>());

            container.RegisterSingletonType<IPatchManager, PatchManager>();
            container.RegisterSingletonType<IPluginManager, EcoPluginManager>();
            container.RegisterSingletonType<ICommandHandler, EcoCommandHandler>();
            container.RegisterSingletonType<IImplementation, Eco>();
        }
    }
}
