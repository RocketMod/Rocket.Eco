using System;
using System.Reflection;

using Eco.Gameplay.Systems.Chat;

using Rocket.API.Commands;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandWrapper : ICommand
    {
        public string Name => command.CommandName;
        public string[] Permissions => new string[] { $"Eco.Base.{Name}" };

        readonly ChatCommandAttribute command;

        internal EcoCommandWrapper(ChatCommandAttribute command)
        {
            this.command = command;
        }

        public void Execute(ICommandContext context)
        {
        }
    }
}
