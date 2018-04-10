using System;

using Rocket.API.Commands;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandWrapper : ICommand
    {
        public string Name => throw new NotImplementedException();
        public string[] Permissions => new string[] { $"Eco.Base.{Name}" };

        public void Execute(ICommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}
