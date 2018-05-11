using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.Eco.API.Patching;
using Rocket.Eco.Commands;
using Rocket.Eco.Patching;

namespace Rocket.Eco.Properties
{
    /// <inheritdoc />
    public class DependencyRegistrator : IDependencyRegistrator
    {
        /// <inheritdoc />
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IPatchManager, PatchManager>(null, "ecopatchmanager");
            container.RegisterSingletonType<IImplementation, EcoImplementation>(null, "eco");
            container.RegisterSingletonType<ICommandProvider, EcoCommandProvider>("ecocommandprovider");

            //No more IChatManager :O
            //container.RegisterSingletonType<IChatManager, EcoChatManager>(null, "ecochatmanager");
        }
    }
}