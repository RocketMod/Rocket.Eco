using System;
using Rocket.API.DependencyInjection;
using Rocket.API.Entities;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Core.Player;
using Rocket.Eco.API;
using InternalEcoUser = Eco.Gameplay.Players.User;
using InternalEcoPlayer = Eco.Gameplay.Players.Player;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IPlayer" />
    public sealed class EcoPlayer : BasePlayer, IUserInfo
    {
        private readonly string unbuiltId;
        private EcoUser ecoUser;

        internal EcoPlayer(InternalEcoUser user, IDependencyContainer container) : base(container)
        {
            InternalEcoUser = user ?? throw new ArgumentNullException(nameof(user));

            ecoUser = new EcoUser(this);

            UserManager = Container.Resolve<IUserManager>("ecousermanager");
        }

        internal EcoPlayer(string id, IDependencyContainer container) : base(container)
        {
            unbuiltId = id;

            UserManager = Container.Resolve<IUserManager>("ecousermanager");
        }

        /// <summary>
        ///     The internal Eco represntation of a player attached to this object.
        /// </summary>
        /// <exception cref="InvalidOperationException"> when the player is not online.</exception>
        public InternalEcoPlayer InternalEcoPlayer => IsOnline ? InternalEcoUser.Player : throw new InvalidOperationException("The player must be online to access this field.");

        /// <summary>
        ///     The internal Eco represntation of a user attached to this object.
        /// </summary>
        public InternalEcoUser InternalEcoUser { get; private set; }

        /// <summary>
        ///     Checks if the player is an admin according to vanilla Eco's admin list.
        /// </summary>
        public bool IsAdmin => InternalEcoUser?.IsAdmin ?? false;

        /// <summary>
        ///     Returns a <see cref="EUserIdType" /> based on what type of account the user is using.
        /// </summary>
        public EUserIdType UserIdType
        {
            get
            {
                if (InternalEcoUser == null)
                    return EUserIdType.Unknown;

                bool hasSlg = string.IsNullOrWhiteSpace(InternalEcoUser.SlgId);
                bool hasSteam = string.IsNullOrWhiteSpace(InternalEcoUser.SteamId);

                if (hasSlg && hasSteam)
                    return EUserIdType.Both;

                return hasSteam ? EUserIdType.Steam : EUserIdType.Slg;
            }
        }

        /// <inheritdoc />
        public override IUser User => IsOnline ? null : ecoUser;

        /// <inheritdoc />
        public override IEntity Entity => IsOnline ? null : ecoUser;

        /// <inheritdoc />
        public override bool IsOnline => InternalEcoUser?.LoggedIn ?? false;

        /// <inheritdoc />
        /// <returns>
        ///     Will return the players Slg ID, if that is not available, their Steam ID will be returned.
        /// </returns>
        public override string Id
        {
            get
            {
                if (InternalEcoUser == null)
                    return unbuiltId;

                return string.IsNullOrWhiteSpace(InternalEcoUser.SlgId) ? InternalEcoUser.SteamId : InternalEcoUser.SlgId;
            }
        }

        /// <inheritdoc />
        public override string Name => InternalEcoUser?.Name;

        /// <inheritdoc />
        public IUserManager UserManager { get; }

        internal void BuildReference(InternalEcoUser user)
        {
            InternalEcoUser = user ?? throw new ArgumentNullException(nameof(user));

            ecoUser = new EcoUser(this);
        }

        /// <summary>
        ///     Sends a message to the player.
        /// </summary>
        /// <param name="message">The <see cref="String" /> to send to the player.</param>
        /// <param name="arguments">
        ///     Any arguments passed to the <paramref name="message" /> using
        ///     <see cref="string.Format(string, object[])" />.
        /// </param>
        public void SendMessage(string message, params object[] arguments)
        {
            if (!IsOnline)
                throw new InvalidOperationException("The player must be online.");

            InternalEcoPlayer.SendTemporaryMessageAlreadyLocalized(string.Format(message, arguments));
        }

        /// <summary>
        ///     Sends an error message to the player.
        /// </summary>
        /// <param name="message">The <see cref="String" /> to send to the player.</param>
        /// <param name="arguments">
        ///     Any arguments passed to the <paramref name="message" /> using
        ///     <see cref="string.Format(string, object[])" />.
        /// </param>
        public void SendErrorMessage(string message, params object[] arguments)
        {
            if (!IsOnline)
                throw new InvalidOperationException("The player must be online.");

            InternalEcoPlayer.SendTemporaryErrorAlreadyLocalized(string.Format(message, arguments));
        }
    }
}