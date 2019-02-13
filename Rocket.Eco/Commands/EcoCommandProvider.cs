using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.Core.ServiceProxies;
using Rocket.Eco.Commands.EcoCommands;

namespace Rocket.Eco.Commands
{
    /// <inheritdoc cref="ICommandProvider" />
    /// <summary>
    ///     Provides all the commands added by the Eco implementation.
    /// </summary>
    [ServicePriority(Priority = ServicePriority.Low)]
    public sealed class EcoCommandProvider : ICommandProvider
    {
        private readonly IHost host;

        /// <inheritdoc />
        public EcoCommandProvider(IHost host)
        {
            this.host = host;

            Commands = new ICommand[]
            {
                new CommandBan(),
                new CommandKick(),
                new CommandAdmin(),
                new CommandUnAdmin(),
                new CommandSave(),
                new CommandShutdown()
            };
        }

        /// <inheritdoc />
        public string ServiceName => GetType().Name;

        /// <inheritdoc />
        public ILifecycleObject GetOwner(ICommand command) => host;

        /// <inheritdoc />
        public Task InitAsync() => Task.CompletedTask;

        /// <inheritdoc />
        public IEnumerable<ICommand> Commands { get; }
    }
}