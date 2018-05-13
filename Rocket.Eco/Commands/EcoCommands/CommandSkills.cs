using System;
using Eco.Core.Controller;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.Core.Commands;
using Rocket.Core.User;
using Rocket.Eco.Player;

namespace Rocket.Eco.Commands.EcoCommands
{
    /// <inheritdoc />
    /// <summary>
    ///     Give yourself or someone else skill points.
    /// </summary>
    public sealed class CommandSkills : ICommand
    {
        /// <inheritdoc />
        public bool SupportsUser(Type user) => true;

        /// <inheritdoc />
        public string Name => "Skills";

        /// <inheritdoc />
        public string[] Aliases => new[] {"Exp", "Experience"};

        /// <inheritdoc />
        public string Summary => "Give yourself or someone else skill points.";

        /// <inheritdoc />
        public string Description => null;

        /// <inheritdoc />
        public string Permission => "Rocket.Skills";

        /// <inheritdoc />
        public string Syntax => "<[n]ame / id> <points> | <points>";

        /// <inheritdoc />
        public IChildCommand[] ChildCommands => new IChildCommand[0];

        /// <inheritdoc />
        public void Execute(ICommandContext context)
        {
            if (context.Parameters.Length == 0)
                throw new CommandWrongUsageException();

            EcoPlayer target;
            float skills;

            if (context.Parameters.Length == 1)
            {
                if (!(context.User is EcoUser ecoUser))
                    throw new CommandWrongUsageException("Only in-game players may give skill points to themselves.");

                if (!float.TryParse(context.Parameters[0], out skills))
                    throw new CommandWrongUsageException("Please specify a valid amount.");

                target = ecoUser.Player;
            }
            else
            {
                if (!float.TryParse(context.Parameters[1], out skills))
                    throw new CommandWrongUsageException("Please specify a valid amount.");

                if (!context.Container.Resolve<IPlayerManager>("ecoplayermanager").TryGetOnlinePlayer(context.Parameters[0], out IPlayer player))
                    throw new CommandWrongUsageException("The target player couldn't be found or isn't online!");

                target = (EcoPlayer) player;
            }

            target.InternalEcoUser.XP += skills;
            ControllerManager.NotifyChanged(target.InternalEcoUser, "XP");

            string message = target.User == context.User ? $"You have given yourself {skills} skill points." : $"{context.User.Name} has given you {skills} skills points.";

            context.User.SendMessage(message);
        }
    }
}