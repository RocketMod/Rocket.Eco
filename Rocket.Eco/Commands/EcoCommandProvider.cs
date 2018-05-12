using System.Collections.Generic;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.Eco.Commands.EcoCommands;

namespace Rocket.Eco.Commands
{
    /// <inheritdoc cref="ICommandProvider" />
    /// <summary>
    ///     Provides all the commands added by the Eco implementation.
    /// </summary>
    public sealed class EcoCommandProvider : ICommandProvider
    {
        private readonly IDependencyContainer container;

        /// <inheritdoc />
        public EcoCommandProvider(IDependencyContainer container)
        {
            this.container = container;

            Commands = new ICommand[]
            {
                new CommandBan(),
                new CommandKick(),
                new CommandAdmin(),
                new CommandUnAdmin(),
                new CommandSave(),
                new CommandShutdown(),
                new CommandFeed()
            };
        }

        /// <inheritdoc />
        public string ServiceName => GetType().Name;

        /// <inheritdoc />
        public ILifecycleObject GetOwner(ICommand command) => container.Resolve<IImplementation>();

        /// <inheritdoc />
        public IEnumerable<ICommand> Commands { get; }
    }
}