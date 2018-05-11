using System;
using Rocket.API.Entities;
using Rocket.API.User;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IUser" />
    public sealed class EcoUser : IUser, IEntity
    {
        internal EcoUser(EcoPlayer player)
        {
            Player = player;
        }

        /// <summary>
        ///     A reference to the companion <see cref="EcoPlayer" /> this instance is related to.
        /// </summary>
        public EcoPlayer Player { get; }

        /// <inheritdoc />
        public string EntityTypeName => UserType;

        /// <inheritdoc />
        public string Id => Player.Id;

        /// <inheritdoc />
        public string Name => Player.Name;

        /// <inheritdoc />
        public IdentityType Type => Player.Type;

        /// <inheritdoc />
        public IUserManager UserManager => Player.UserManager;

        /// <inheritdoc />
        public bool IsOnline => Player.IsOnline;

        /// <inheritdoc />
        public DateTime SessionConnectTime => DateTime.MinValue;

        /// <inheritdoc />
        public DateTime? SessionDisconnectTime => null;

        /// <inheritdoc />
        public DateTime? LastSeen => null;

        /// <inheritdoc />
        public string UserType => GetType().Name;
    }
}