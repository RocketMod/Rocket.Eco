using System;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.Core.Commands;
using Rocket.Core.User;
using Rocket.Eco.Player;

namespace Rocket.Eco.Commands.EcoCommands
{
    /// <inheritdoc />
    /// <summary>
    ///     A command to override the vanilla kick command to allow the console to execute it.
    /// </summary>
    public sealed class CommandKick : ICommand
    {
        /// <inheritdoc />
        public bool SupportsUser(Type user) => true;

        /// <inheritdoc />
        public string Name => "Kick";

        /// <inheritdoc />
        public string[] Aliases => new string[0];

        /// <inheritdoc />
        public string Summary => "Kicks a player.";

        /// <inheritdoc />
        public string Description => null;

        /// <inheritdoc />
        public string Permission => "Rocket.Kick";

        /// <inheritdoc />
        public string Syntax => "<[n]ame / id> <reason>";

        /// <inheritdoc />
        public IChildCommand[] ChildCommands => new IChildCommand[0];

        /// <inheritdoc />
        public void Execute(ICommandContext context)
        {
            if (context.Parameters.Length == 0)
                throw new CommandWrongUsageException();

            IPlayerManager playerManager = context.Container.Resolve<IPlayerManager>("ecoplayermanager");

            if (!playerManager.TryGetOnlinePlayer(context.Parameters[0], out IPlayer player))
            {
                context.User.SendMessage("The requested user is not online.");
                return;
            }

            string reason = null;

            if (context.Parameters.Length > 1)
                reason = string.Join(" ", context.Parameters);

            playerManager.Kick(((EcoPlayer) player).User, context.User, reason);

            context.User.SendMessage("The requested user has been kicked.");
        }
    }
}