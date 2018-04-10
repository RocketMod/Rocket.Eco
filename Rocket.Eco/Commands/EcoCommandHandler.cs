using System;

using Rocket.API.Commands;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandHandler : ICommandHandler
    {
        public ICommand GetCommand(ICommandContext ctx)
        {
            throw new NotImplementedException();
        }

        public bool HandleCommand(ICommandCaller caller, string commandLine)
        {
            throw new NotImplementedException();
        }
    }
}
 