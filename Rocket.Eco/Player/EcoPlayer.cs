using System;
using Rocket.API.DependencyInjection;
using Rocket.API.Entities;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Core.Player;
using InternalEcoUser = Eco.Gameplay.Players.User;
using InternalEcoPlayer = Eco.Gameplay.Players.Player;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IPlayer" />
    public class EcoPlayer : BasePlayer, IUserInfo
    {
        private readonly EcoUser ecoUser;

        internal EcoPlayer(InternalEcoUser user, IDependencyContainer container) : base(container)
        {
            InternalEcoUser = user ?? throw new ArgumentNullException(nameof(user));

            ecoUser = new EcoUser(this);
        }

        /// <summary>
        ///     The internal Eco represntation of a player attached to this object.
        /// </summary>
        /// <exception cref="InvalidOperationException"> when the player is not online.</exception>
        /// >
        public InternalEcoPlayer InternalEcoPlayer => IsOnline ? InternalEcoUser.Player : throw new InvalidOperationException("The player must be online to access this field.");

        /// <summary>
        ///     The internal Eco represntation of a user attached to this object.
        /// </summary>
        public InternalEcoUser InternalEcoUser { get; }

        /// <summary>
        ///     Checks if the player is an admin according to vanilla Eco's admin list.
        /// </summary>
        public bool IsAdmin => InternalEcoUser.IsAdmin;

        /// <inheritdoc />
        public override IUser User => ecoUser;

        /// <inheritdoc />
        public override IEntity Entity => ecoUser;

        /// <inheritdoc />
        public override bool IsOnline => InternalEcoUser.LoggedIn;

        /// <inheritdoc />
        public override string Id => InternalEcoUser.SteamId;

        /// <inheritdoc />
        public override string Name => InternalEcoUser.Name;

        /// <inheritdoc />
        public IUserManager UserManager => Container.Resolve<IUserManager>("ecousermanager");

        /// <summary>
        ///     Sends a message to the player.
        /// </summary>
        /// <param name="message">The <see cref="String" /> to send to the player.</param>
        /// <param name="arguments">
        ///     Any arguments passed to the <paramref name="message" /> using
        ///     <see cref="string.Format(string, object[])" />.
        /// </param>
        public void SendMessage(string message, params object[] arguments) => InternalEcoPlayer.SendTemporaryMessageAlreadyLocalized(string.Format(message, arguments));

        /// <summary>
        ///     Sends an error message to the player.
        /// </summary>
        /// <param name="message">The <see cref="String" /> to send to the player.</param>
        /// <param name="arguments">
        ///     Any arguments passed to the <paramref name="message" /> using
        ///     <see cref="string.Format(string, object[])" />.
        /// </param>
        public void SendErrorMessage(string message, params object[] arguments) => InternalEcoPlayer.SendTemporaryErrorAlreadyLocalized(string.Format(message, arguments));
    }
}