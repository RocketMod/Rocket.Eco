using System.Collections.Generic;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.Eco.API;
using Rocket.Eco.Commands.EcoCommands;

namespace Rocket.Eco.Commands
{
    /// <inheritdoc cref="ICommandProvider" />
    /// <summary>
    ///     Provides all the commands added by the Eco implementation.
    /// </summary>
    public sealed class EcoCommandProvider : ContainerAccessor, ICommandProvider
    {
        /// <inheritdoc />
        public EcoCommandProvider(IDependencyContainer container) : base(container)
        {
            Commands = new ICommand[]
            {
                new CommandBan(),
                new CommandKick(),
                new CommandAdmin(),
                new CommandRemoveAdmin(),
                new CommandSave(),
                new CommandShutdown()
            };
        }

        /// <inheritdoc />
        public ILifecycleObject GetOwner(ICommand command) => Container.Resolve<IImplementation>();

        /// <inheritdoc />
        public IEnumerable<ICommand> Commands { get; }
    }
}