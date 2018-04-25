using System.Collections.Generic;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.Eco.API;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandProvider : RuntimeObject, ICommandProvider
    {
        private readonly List<EcoCommandWrapper> commands = new List<EcoCommandWrapper>();

        internal EcoCommandProvider(IRuntime runtime) : base(runtime) { }
        public IEnumerable<ICommand> Commands => commands;

        internal void CollectCommands() { }
    }
}