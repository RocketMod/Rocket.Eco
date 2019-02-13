using System;
using Eco.Gameplay.Economy;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Core.Player;
using Rocket.Eco.API;
using InternalEcoUser = Eco.Gameplay.Players.User;
using InternalEcoPlayer = Eco.Gameplay.Players.Player;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IPlayer" />
    public sealed class EcoPlayer : BasePlayer<EcoPlayerUser, EcoPlayerEntity, EcoPlayer>
    {
        private readonly string unbuiltId;
        private EcoPlayerEntity ecoEntity;

        internal EcoPlayer(InternalEcoUser user, IPlayerManager playerManager, IDependencyContainer container) : base(container, playerManager)
        {
            InternalEcoUser = user ?? throw new ArgumentNullException(nameof(user));

            //PlayerManager = playerManager;

            User = new EcoPlayerUser(this, container, playerManager);
            ecoEntity = new EcoPlayerEntity(this);
        }

        internal EcoPlayer(string id, IPlayerManager playerManager, IDependencyContainer container) : base(container, playerManager)
        {
            unbuiltId = id;

            User = new EcoPlayerUser(this, container, playerManager);
        }

        /// <summary>
        ///     The internal Eco representation of a player attached to this object.
        /// </summary>
        /// <exception cref="InvalidOperationException"> when the player is not online.</exception>
        public InternalEcoPlayer InternalEcoPlayer => IsOnline ? InternalEcoUser.Player : throw new InvalidOperationException("The player must be online to access this field.");

        /// <summary>
        ///     The internal Eco representation of a user attached to this object.
        /// </summary>
        public InternalEcoUser InternalEcoUser { get; private set; }

        /// <summary>
        ///     Checks if the player is an admin according to vanilla Eco's admin list.
        /// </summary>
        public bool IsAdmin => InternalEcoUser?.IsAdmin ?? false;

        /// <summary>
        ///     Returns a <see cref="float" /> representing the player's in-game reputation.
        /// </summary>
        public float Reputation => ReputationManager.Obj.Rep(Name).CachedTotalReputation;

        /// <summary>
        ///     Returns a <see cref="API.UserIdType" /> based on what type of account the user is using.
        /// </summary>
        public UserIdType UserIdType
        {
            get
            {
                if (InternalEcoUser == null)
                    return UserIdType.Unknown;

                bool hasSlg = string.IsNullOrWhiteSpace(InternalEcoUser.SlgId);
                bool hasSteam = string.IsNullOrWhiteSpace(InternalEcoUser.SteamId);

                if (hasSlg && hasSteam)
                    return UserIdType.Both;

                return hasSteam ? UserIdType.Steam : UserIdType.Slg;
            }
        }

        /// <inheritdoc />
        public override EcoPlayerUser User { get; }

        /// <inheritdoc />
        public override EcoPlayerEntity Entity => IsOnline ? ecoEntity : null;

        /// <inheritdoc />
        public override bool IsOnline => InternalEcoUser?.LoggedIn ?? false;

        /// <inheritdoc />
        public override DateTime SessionConnectTime => throw new NotImplementedException();

        /// <inheritdoc />
        public override DateTime? SessionDisconnectTime => throw new NotImplementedException();
        
        /// <returns>
        ///     Will return the players Slg ID, if that is not available, their Steam ID will be returned.
        /// </returns>
        public string Id
        {
            get
            {
                if (InternalEcoUser == null)
                    return unbuiltId;

                return string.IsNullOrWhiteSpace(InternalEcoUser.SlgId) ? InternalEcoUser.SteamId : InternalEcoUser.SlgId;
            }
        }
        
        public string Name => InternalEcoUser?.Name;

        //TODO: This is not required anymore and needs to be phased out.
        internal void BuildReference(InternalEcoUser user)
        {
            InternalEcoUser = user ?? throw new ArgumentNullException(nameof(user));

            ecoEntity = new EcoPlayerEntity(this);
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