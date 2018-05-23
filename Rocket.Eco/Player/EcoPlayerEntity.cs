using System.Numerics;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Eco.Extensions;
using EcoVector3 = Eco.Shared.Math.Vector3;

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
        public Vector3 Position => Player.InternalEcoUser.Position.ToSystemVector3();

        /// <inheritdoc />
        public bool Teleport(Vector3 position)
        {
            Player.InternalEcoPlayer.SetPosition(position.ToEcoVector3());
            return true;
        }
    }
}