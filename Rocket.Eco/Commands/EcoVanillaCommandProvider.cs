using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eco.Gameplay.Systems.Chat;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.Core.Logging;

namespace Rocket.Eco.Commands
{
    /// <inheritdoc cref="ICommandProvider" />
    /// <summary>
    ///     Translates all of the commands provided by Eco and its modkit into a Rocket-useable <see cref="ICommand" />.
    /// </summary>
    public sealed class EcoVanillaCommandProvider : ICommandProvider
    {
        private readonly List<EcoNativeCommand> commands;
        private readonly IDependencyContainer container;

        /// <inheritdoc />
        public EcoVanillaCommandProvider(IEnumerable<ICommand> currentCommands, IDependencyContainer container)
        {
            this.container = container;

            Dictionary<string, MethodInfo> cmds = (Dictionary<string, MethodInfo>) typeof(ChatManager).GetField("commands", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(ChatManager.Obj);

            if (cmds == null)
                throw new Exception("A critical part of the Eco codebase has been changed; please uninstall Rocket until it is updated to support these changes.");

            ILogger logger = container.Resolve<ILogger>();

            List<EcoNativeCommand> tempCommands = new List<EcoNativeCommand>();

            foreach (KeyValuePair<string, MethodInfo> pair in cmds)
            {
                ChatCommandAttribute attribute = (ChatCommandAttribute) pair.Value.GetCustomAttributes().FirstOrDefault(x => x is ChatCommandAttribute);

                if (attribute == null)
                    continue;

                string name = attribute.UseMethodName ? pair.Value.Name : attribute.CommandName;

                bool overriden = false;

                if (currentCommands.Any(command => command.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) || command.Aliases != null && command.Aliases.Contains(name, StringComparer.InvariantCultureIgnoreCase)))
                {
                    logger.LogWarning($"The vanilla command \"{name}\" was not registered as an override exists.");
                    overriden = true;
                }

                if (!overriden)
                    tempCommands.Add(new EcoNativeCommand(pair.Value, attribute, container));
            }

            commands = tempCommands;
        }

        /// <inheritdoc />
        public string ServiceName => GetType().Name;

        /// <inheritdoc />
        public ILifecycleObject GetOwner(ICommand command) => container.Resolve<IImplementation>();

        /// <inheritdoc />
        public IEnumerable<ICommand> Commands => commands;
    }
}