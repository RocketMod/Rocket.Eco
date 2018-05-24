using System;
using Rocket.API.Player;
using Rocket.API.User;
using EcoVector3 = Eco.Shared.Math.Vector3;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IUser" />
    public sealed class EcoPlayerUser : IPlayerUser<EcoPlayer>
    {
        internal EcoPlayerUser(EcoPlayer player, IUserManager userManager)
        {
            Player = player;
        }

        /// <inheritdoc cref="IPlayerUser" />
        public EcoPlayer Player { get; }

        /// <inheritdoc />
        public string Id => Player.Id;

        /// <inheritdoc />
        public string Name => Player.Name;

        /// <inheritdoc />
        public string IdentityType => IdentityTypes.Player;

        /// <inheritdoc />
        public IUserManager UserManager { get; }
   
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