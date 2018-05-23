using System;
using System.Numerics;
using Rocket.API.Entities;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Eco.Extensions;
using EcoVector3 = Eco.Shared.Math.Vector3;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IUser" />
    public sealed class EcoUser : IPlayerUser<EcoPlayer>, IPlayerEntity<EcoPlayer>
    {
        internal EcoUser(EcoPlayer player)
        {
            Player = player;
        }

        /// <inheritdoc cref="IPlayerUser"/>
        public EcoPlayer Player { get; }

        /// <inheritdoc />
        public string EntityTypeName => UserType;

        /// <inheritdoc />
        public Vector3 Position => Player.InternalEcoUser.Position.ToSystemVector3();

        /// <inheritdoc />
        public bool Teleport(Vector3 position)
        {
            Player.InternalEcoPlayer.SetPosition(position.ToEcoVector3());
            return true;
        }

        /// <inheritdoc />
        public string Id => Player.Id;

        /// <inheritdoc />
        public string Name => Player.Name;

        /// <inheritdoc />
        public string IdentityType => "User";

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