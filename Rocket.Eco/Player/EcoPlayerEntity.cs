using System;
using System.Numerics;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Eco.Extensions;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IUser" />
    public sealed class EcoPlayerEntity : IPlayerEntity<EcoPlayer>
    {
        internal EcoPlayerEntity(EcoPlayer player)
        {
            Player = player;
        }

        /// <inheritdoc cref="IPlayerUser" />
        public EcoPlayer Player { get; }

        /// <inheritdoc />
        public string EntityTypeName => IdentityTypes.Player;

        /// <inheritdoc />
        public Vector3 Position => (Player.IsOnline) ? Player.InternalEcoUser.Position.ToSystemVector3() : throw new InvalidOperationException("The player must be online.");

        /// <inheritdoc />
        public bool Teleport(Vector3 position)
        {
            if (!Player.IsOnline)
                throw new InvalidOperationException("The player must be online.");

            Player.InternalEcoPlayer.SetPosition(position.ToEcoVector3());
            return true;
        }
    }
}