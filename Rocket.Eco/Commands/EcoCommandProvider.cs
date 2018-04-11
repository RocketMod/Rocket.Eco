using System;
using System.Collections.Generic;

using Rocket.API.Commands;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandProvider : ICommandProvider
    {
        public IEnumerable<ICommand> Commands => commands;

        private readonly List<EcoCommandWrapper> commands = new List<EcoCommandWrapper>();

        internal void CollectCommands()
        {

        }
    }
}
