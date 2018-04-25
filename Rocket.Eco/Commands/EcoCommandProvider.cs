using System.Collections.Generic;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.Eco.API;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandProvider : ContainerAccessor, ICommandProvider
    {
        private readonly List<EcoCommandWrapper> commands = new List<EcoCommandWrapper>();

        internal EcoCommandProvider(IDependencyContainer container) : base(container) { }
        public IEnumerable<ICommand> Commands => commands;

        internal void CollectCommands() { }
    }
}