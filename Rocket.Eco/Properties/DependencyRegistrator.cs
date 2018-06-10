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
            container.RegisterSingletonType<IPatchManager, PatchManager>(null, "eco", "game");
            container.RegisterSingletonType<IHost, EcoHost>(null, "eco", "game");
            container.RegisterSingletonType<ICommandProvider, EcoCommandProvider>("eco_vanilla_commands");
            container.RegisterSingletonType<IPermissionProvider, EcoPermissionProvider>("eco_vanilla_permissions");
        }
    }
}