using System.Collections.Generic;
using Rocket.API.Commands;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandProvider : ICommandProvider
    {
        private readonly List<EcoCommandWrapper> commands = new List<EcoCommandWrapper>();
        public IEnumerable<ICommand> Commands => commands;

        internal void CollectCommands() { }
    }
}