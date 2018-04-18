using System;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;
using Rocket.Core.Player;
using BaseEcoUser = Eco.Gameplay.Players.User;

namespace Rocket.Eco.Player
{
    public class EcoPlayer : BasePlayer, IComparable<BaseEcoUser>, IEquatable<BaseEcoUser>
    {
        internal EcoPlayer(BaseEcoUser user, IDependencyContainer container) : base(container)
        {
            User = user;
        }

        internal EcoPlayer(string id, IDependencyContainer container) : base(container)
        {
            this.id = id;
        }

        public BaseEcoUser User { get; }
        private readonly string id;

        public bool IsAdmin => User?.IsAdmin ?? false;
        public bool IsDev => User?.IsDev ?? false;
        public override bool IsOnline => User?.LoggedIn ?? false;

        public override string Id => User?.SteamId ?? id ?? string.Empty;
        public override string Name => User?.Name ?? string.Empty;

        public override Type CallerType => typeof(EcoPlayer);

        public int CompareTo(BaseEcoUser other) => other == null ? 1 : string.Compare(Id, other.SteamId, StringComparison.InvariantCulture);
        public bool Equals(BaseEcoUser other) => other != null && Id.Equals(other.SteamId, StringComparison.InvariantCulture);

        public int CompareTo(object other)
        {
            if (other == null) return 1;

            Type type = other.GetType();

            if (type == typeof(string)) return CompareTo((string) other);

            if (type == typeof(IPlayer)) return CompareTo((IPlayer) other);

            if (type == typeof(BaseEcoUser)) return CompareTo((BaseEcoUser) other);

            throw new ArgumentException($"Cannot compare the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;

            Type type = other.GetType();

            if (type == typeof(string)) return Equals((string) other);

            if (type == typeof(IPlayer)) return Equals((IPlayer) other);

            if (type == typeof(BaseEcoUser)) return Equals((BaseEcoUser) other);

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public override string ToString() => Id;
        public override int GetHashCode() => BitConverter.ToInt32(BitConverter.GetBytes(ulong.Parse(Id)), 4);
    }
}