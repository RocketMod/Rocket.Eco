using System;
using System.Collections.Generic;
using System.Reflection;
using Eco.Gameplay.Systems.Chat;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.Core.Logging;
using Rocket.Core.ServiceProxies;
using Rocket.Eco.API.Configuration;

namespace Rocket.Eco.Commands
{
    /// <inheritdoc cref="ICommandProvider" />
    /// <summary>
    ///     Translates all of the commands provided by Eco and its modkit into a Rocket-useable <see cref="ICommand" />.
    /// </summary>
    [ServicePriority(Priority = ServicePriority.Lowest)]
    public sealed class EcoNativeCommandProvider : ICommandProvider
    {
        private readonly List<EcoNativeCommand> commands = new List<EcoNativeCommand>();
        private readonly IHost host;

        /// <inheritdoc />
        public EcoNativeCommandProvider(IDependencyContainer container, IEcoSettingsProvider settingsProvider)
        {
            ILogger logger = container.Resolve<ILogger>();

            if (!settingsProvider.Settings.EnableVanillaCommands)
            {
                logger.LogInformation("Native commmands are disabled in the settings, none will be loaded.");
                return;
            }

            host = container.Resolve<IHost>();
            Dictionary<string, MethodInfo> cmds = (Dictionary<string, MethodInfo>) typeof(ChatManager).GetField("commands", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(ChatManager.Obj);

            if (cmds == null)
                throw new Exception("A critical part of the Eco codebase has been changed; please uninstall Rocket until it is updated to support these changes.");

            foreach (KeyValuePair<string, MethodInfo> pair in cmds)
                commands.Add(new EcoNativeCommand(pair.Value, logger));
        }

        /// <inheritdoc />
        public string ServiceName => GetType().Name;

        /// <inheritdoc />
        public ILifecycleObject GetOwner(ICommand command) => host;

        /// <inheritdoc />
        public void Init() { }

        /// <inheritdoc />
        public IEnumerable<ICommand> Commands => commands;
    }
}