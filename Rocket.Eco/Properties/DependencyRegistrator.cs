using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Permissions;
using Rocket.Eco.API.Patching;
using Rocket.Eco.Commands;
using Rocket.Eco.Patching;
using Rocket.Eco.Permissions;

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
            container.RegisterSingletonType<IPermissionProvider, EcoPermissionProvider>("ecopermissionprovider");

            //No more IChatManager :O
            //container.RegisterSingletonType<IChatManager, EcoChatManager>(null, "ecochatmanager");
        }
    }
}