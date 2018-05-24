﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Eco.Gameplay.Systems.Chat;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.Core.ServiceProxies;

namespace Rocket.Eco.Commands
{
    /// <inheritdoc cref="ICommandProvider" />
    /// <summary>
    ///     Translates all of the commands provided by Eco and its modkit into a Rocket-useable <see cref="ICommand" />.
    /// </summary>
    [ServicePriority(Priority = ServicePriority.Lowest)]
    public sealed class EcoVanillaCommandProvider : ICommandProvider
    {
        private readonly List<EcoNativeCommand> commands = new List<EcoNativeCommand>();
        private readonly IImplementation implementation;

        /// <inheritdoc />
        public EcoVanillaCommandProvider(IImplementation implementation, IDependencyContainer container)
        {
            this.implementation = implementation;
            Dictionary<string, MethodInfo> cmds = (Dictionary<string, MethodInfo>) typeof(ChatManager).GetField("commands", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(ChatManager.Obj);

            if (cmds == null)
                throw new Exception("A critical part of the Eco codebase has been changed; please uninstall Rocket until it is updated to support these changes.");

            foreach (KeyValuePair<string, MethodInfo> pair in cmds)
                commands.Add(new EcoNativeCommand(pair.Value, container));
        }

        /// <inheritdoc />
        public string ServiceName => GetType().Name;

        /// <inheritdoc />
        public ILifecycleObject GetOwner(ICommand command) => implementation;

        /// <inheritdoc />
        public IEnumerable<ICommand> Commands => commands;
    }
}