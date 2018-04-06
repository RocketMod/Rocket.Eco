using System;

using Rocket.API;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;
using Rocket.API.Plugin;

using Rocket.Eco.Player;
using Rocket.Eco.Plugins;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterType<IPatchManager, PatchManager>();
            container.RegisterType<IPlayerManager, EcoPlayerManager>();

            container.RegisterSingletonInstance<IPluginManager>(container.Activate<EcoPluginManager>());
            container.RegisterSingletonType<IImplementation, Eco>();
        }
    }
}
