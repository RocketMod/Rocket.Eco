using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;
using Rocket.Eco.Commands;
using Rocket.Eco.Patching;
using Rocket.Eco.Player;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IPlayerManager, EcoPlayerManager>(null, "ecoplayermanager");
            container.RegisterSingletonType<ICommandProvider, EcoCommandProvider>("ecocommandprovider");
            container.RegisterSingletonType<IImplementation, EcoImplementation>(null, "eco");

            container.RegisterSingletonType<IPatchManager, PatchManager>();
        }
    }
}