using System.Threading.Tasks;
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.Players;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Core.Commands;
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
        public bool SupportsUser(IUser user) => true;

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
        public async Task ExecuteAsync(ICommandContext context)
        {
            if (context.Parameters.Length == 0)
                throw new CommandWrongUsageException();

            IPlayerManager playerManager = context.Container.Resolve<IPlayerManager>("eco");

            if (playerManager.TryGetOnlinePlayer(context.Parameters[0], out IPlayer player))
                await context.User.UserManager.SendMessageAsync(null, ((EcoPlayer) player).User,
                    "You have been stripped of your administrator permissions.");
            else
                player = await playerManager.GetPlayerAsync(context.Parameters[0]);

            if (player is EcoPlayer ecoPlayer && ecoPlayer.UserIdType == UserIdType.Both)
                UserManager.Config.Admins.Remove(ecoPlayer.InternalEcoUser.SteamId);

            UserManager.Config.Admins.Remove(player.User.Id);
            UserManager.Obj.SaveConfig();

            await context.User.UserManager.SendMessageAsync(null, context.User,
                "The requested user has been removed as an administrator.");
        }
    }
}