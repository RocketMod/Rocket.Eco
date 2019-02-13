using System;
using System.Numerics;
using System.Threading.Tasks;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Eco.Extensions;
using Quaternion = Eco.Shared.Math.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IUser" />
    public sealed class EcoPlayerEntity : IPlayerEntity<EcoPlayer>
    {
        internal EcoPlayerEntity(EcoPlayer player)
        {
            Player = player;
        }

        /// <inheritdoc />
        public EcoPlayer Player { get; }

        /// <inheritdoc />
        public string EntityTypeName => "Player";

        /// <inheritdoc />
        public Vector3 Position => Player.IsOnline ? Player.InternalEcoUser.Position.ToSystemVector3() : throw new InvalidOperationException("The player must be online.");

        /// <inheritdoc />
        public Task<bool> TeleportAsync(Vector3 position, float rotation)
        {
            if (!Player.IsOnline)
                throw new InvalidOperationException("The player must be online.");

            Player.InternalEcoPlayer.SetPositionAndRotation(position.ToEcoVector3(), Quaternion.ToQuaternion(new Vector3(0, 0, rotation).ToEcoVector3()));
            return Task.FromResult(true);
        }
    }
}