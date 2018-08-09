using System;
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.Players;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.Core.Commands;
using Rocket.Core.User;
using Rocket.Eco.API;
using Rocket.Eco.Player;

namespace Rocket.Eco.Commands.EcoCommands
{
    /// <inheritdoc />
    /// <summary>
    ///     A command to remove a player from the admin list.
    /// </summary>
    public sealed class CommandUnAdmin : ICommand
    {
        /// <inheritdoc />
        public string Permission => "Rocket.RemoveAdmin";

        /// <inheritdoc />
        public bool SupportsUser(Type user) => true;

        /// <inheritdoc />
        public string Name => "UnAdmin";

        /// <inheritdoc />
        public string[] Aliases => new[] {"DelAdmin", "RemoveAdmin", "DeAdmin"};

        /// <inheritdoc />
        public string Summary => "Removes a player's administrator permissions.";

        /// <inheritdoc />
        public string Description => null;

        /// <inheritdoc />
        public string Syntax => "[[n]ame / id]";

        /// <inheritdoc />
        public IChildCommand[] ChildCommands => new IChildCommand[0];

        /// <inheritdoc />
        public void Execute(ICommandContext context)
        {
            if (context.Parameters.Length == 0)
                throw new CommandWrongUsageException();

            IPlayerManager playerManager = context.Container.Resolve<IPlayerManager>("ecoplayermanager");

            if (playerManager.TryGetOnlinePlayer(context.Parameters[0], out IPlayer player))
                ((EcoPlayer) player).User.SendMessage("You have been stripped of your administator permissions.");
            else
                player = playerManager.GetPlayer(context.Parameters[0]);

            if (player is EcoPlayer ecoPlayer && ecoPlayer.UserIdType == UserIdType.Both)
                UserManager.Config.Admins.Remove(ecoPlayer.InternalEcoUser.SteamId);

            UserManager.Config.Admins.Remove(player.Id);
            UserManager.Obj.SaveConfig();

            context.User.SendMessage("The requested user has been removed as an administrator.");
        }
    }
}