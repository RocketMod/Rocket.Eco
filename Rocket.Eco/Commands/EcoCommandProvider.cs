using System;
using System.Collections.Generic;
using System.Reflection;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.Eco.API;
using Eco.Gameplay.Systems.Chat;
using Rocket.API.Logging;
using Rocket.Core.Logging;

namespace Rocket.Eco.Commands
{
    /// <inheritdoc cref="ICommandProvider" />
    /// <summary>
    ///     Translates all of the commands provided by Eco and its modkit into a Rocket-useable <see cref="ICommand" />.
    /// </summary>
    public sealed class EcoCommandProvider : ContainerAccessor, ICommandProvider
    {
        private readonly List<EcoCommandWrapper> commands = new List<EcoCommandWrapper>();

        /// <inheritdoc />
        public EcoCommandProvider(IDependencyContainer container) : base(container)
        {
            Dictionary<string, MethodInfo> cmds = (Dictionary<string, MethodInfo>)typeof(ChatManager).GetField("commands", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(ChatManager.Obj);

            if (cmds == null)
                throw new Exception("A critical part of the Eco codebase has been changed; please uninstall Rocket until it is updated to support these changes.");

            ILogger logger = Container.Resolve<ILogger>();

            foreach (KeyValuePair<string, MethodInfo> pair in cmds)
            {
                commands.Add(new EcoCommandWrapper(pair.Value, Container));
            }
        }

        /// <inheritdoc />
        public ILifecycleObject GetOwner(ICommand command) => Container.Resolve<IImplementation>();

        /// <inheritdoc />
        public IEnumerable<ICommand> Commands => commands;
    }
}