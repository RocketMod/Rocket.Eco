﻿using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Scheduling;
using Rocket.API.User;
using Rocket.Core.User;
using Rocket.Eco.API.Configuration;
using Rocket.Eco.Commands;
using Rocket.Eco.Configuration;
using Rocket.Eco.Permissions;
using Rocket.Eco.Player;
using Rocket.Eco.Scheduling;

#if DEBUG
using Rocket.Eco.API.Legislation;
using Rocket.Eco.Economy;
using Rocket.Eco.Legislation;
using Rocket.API.Economy;
#endif

namespace Rocket.Eco.Properties
{
    /// <inheritdoc />
    public class DependencyRegistrator : IDependencyRegistrator
    {
        /// <inheritdoc />
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IHost, EcoHost>(null, "eco", "game");
            container.RegisterSingletonType<IEcoSettingsProvider, EcoSettingsProvider>();

            IEcoSettingsProvider settingsProvider = container.Resolve<IEcoSettingsProvider>();
            settingsProvider.Load();

            container.RegisterSingletonType<ICommandProvider, EcoCommandProvider>("eco_commands");
            container.RegisterSingletonInstance<ICommandProvider>(
                new EcoNativeCommandProvider(container, settingsProvider), "eco_vanilla_commands");
            container.RegisterSingletonType<IPermissionProvider, EcoPermissionProvider>("eco_vanilla_permissions");
            container.RegisterSingletonType<ITaskScheduler, EcoTaskScheduler>(null, "eco", "game");

            IPlayerManager playerManager = new EcoPlayerManager(container);
            container.RegisterSingletonInstance(playerManager, null, "eco", "game");
            container.RegisterSingletonInstance<IUserManager>(playerManager, "eco", "game");
            container.RegisterSingletonType<IUserManager, StdConsoleUserManager>("stdconsole");

#if DEBUG
            container.RegisterSingletonType<IGovernment, EcoGovernment>(null, "eco", "game");
            container.RegisterSingletonType<IEconomyProvider, EcoEconomyProvider>(null, "eco", "game");
#endif
        }
    }
}