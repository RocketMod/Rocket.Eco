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
    ///     Makes you or another player eat.
    /// </summary>
    public sealed class CommandFeed : ICommand
    {
        /// <inheritdoc />
        public bool SupportsUser(Type user) => true;

        /// <inheritdoc />
        public string Name => "Feed";

        /// <inheritdoc />
        public string[] Aliases => new[] {"Eat"};

        /// <inheritdoc />
        public string Summary => "Satifies your or another player's hunger.";

        /// <inheritdoc />
        public string Description => null;

        /// <inheritdoc />
        public string Permission => "Rocket.Feed";

        /// <inheritdoc />
        public string Syntax => "<[n]ame / id>";

        /// <inheritdoc />
        public IChildCommand[] ChildCommands => new IChildCommand[0];

        /// <inheritdoc />
        public void Execute(ICommandContext context)
        {
            if (context.Parameters.Length == 0)
            {
                if (!(context.User is EcoUser ecoUser))
                    throw new CommandWrongUsageException("If this is called from the console, you need to specify a user.");

                ecoUser.Player.InternalEcoUser.Stomach.Calories = ecoUser.Player.InternalEcoUser.Stomach.MaxCalories;
                ecoUser.Player.SendMessage("You has filled your stomach!");
                return;
            }

            //99% sure this can't happen, but in makes Resharper feel better.
            if (string.IsNullOrWhiteSpace(context.Parameters[0]))
                throw new CommandWrongUsageException();

            if (!context.Container.Resolve<IPlayerManager>("ecoplayermanager").TryGetOnlinePlayer(context.Parameters[0], out IPlayer player) || !(player is EcoPlayer ecoPlayer))
            {
                context.User.SendMessage("We couldn't find that user!");
                return;
            }

            ecoPlayer.InternalEcoUser.Stomach.Calories = ecoPlayer.InternalEcoUser.Stomach.MaxCalories;
            ecoPlayer.SendMessage($"{context.User.Name} has filled your stomach!");
        }
    }
}