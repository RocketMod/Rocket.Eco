using System.Collections.Generic;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.Eco.API;

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
        public EcoCommandProvider(IDependencyContainer container) : base(container) { }

        /// <inheritdoc />
        public ILifecycleObject GetOwner(ICommand command) => Container.Resolve<IImplementation>();

        /// <inheritdoc />
        public IEnumerable<ICommand> Commands => commands;

        internal void CollectCommands() { }
    }
}