using System;
using System.Runtime.CompilerServices;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.Core.Plugins;
using BaseEcoPlayer = Eco.Gameplay.Players.Player;
using BaseEcoUser = Eco.Gameplay.Players.User;

namespace Rocket.Eco.Player
{
    public sealed class EcoPlayer : BasePlayer, IComparable<BaseEcoPlayer>, IEquatable<BaseEcoPlayer>, IComparable<BaseEcoUser>, IEquatable<BaseEcoUser>
    {
        internal EcoPlayer(BaseEcoPlayer player, IDependencyContainer container) : base(container)
        {
            Player = player;
        }

        public bool IsAdmin => User.IsAdmin;

        public BaseEcoUser User => Player.User;
        public bool IsDev => User.IsDev;
        public bool IsOnline => User.LoggedIn;

        public BaseEcoPlayer Player { get; }

        public override string Id
        {
            get => User.SteamId;
            protected set => throw new NotSupportedException();
        }

        public override string Name
        {
            get => User.Name;
            protected set => throw new NotSupportedException();
        }

        public override double Health
        {
            get => -1;
            set => Container.Get<ILogger>().LogWarning("Setting player health is not supported in Eco!");
        }

        public override double MaxHealth
        {
            get => -1;
            set => Container.Get<ILogger>().LogWarning("Setting player health is not supported in Eco!");
        }

        public override Type CallerType => typeof(EcoPlayer);

        public int CompareTo(BaseEcoPlayer other) => other == null ? 1 : string.Compare(Id, other.User.SteamId, StringComparison.InvariantCulture);
        public int CompareTo(BaseEcoUser other) => other == null ? 1 : string.Compare(Id, other.SteamId, StringComparison.InvariantCulture);
        public bool Equals(BaseEcoPlayer other) => other != null && Id.Equals(other.User.SteamId, StringComparison.InvariantCulture);
        public bool Equals(BaseEcoUser other) => other != null && Id.Equals(other.SteamId, StringComparison.InvariantCulture);

        public override void SendMessage(string message)
        {
            Player.SendTemporaryMessage(FormattableStringFactory.Create(message));
        }

        public void SendErrorMessage(string message)
        {
            Player.SendTemporaryError(FormattableStringFactory.Create(message));
        }

        public int CompareTo(string other) => other == null ? 1 : string.Compare(Id, other, StringComparison.InvariantCulture);
        public bool Equals(string other) => other != null && Id.Equals(other, StringComparison.InvariantCulture);
        public int CompareTo(IIdentifiable other) => other == null ? 1 : string.Compare(Id, other.Id, StringComparison.InvariantCulture);
        public bool Equals(IIdentifiable other) => other != null && Id.Equals(other.Id, StringComparison.InvariantCulture);

        public int CompareTo(object other)
        {
            if (other == null) return 1;

            Type type = other.GetType();

            if (type == typeof(string)) return CompareTo((string) other);

            if (type == typeof(IPlayer)) return CompareTo((IPlayer) other);

            if (type == typeof(BaseEcoPlayer)) return CompareTo((BaseEcoPlayer) other);

            if (type == typeof(BaseEcoUser)) return CompareTo((BaseEcoUser) other);

            throw new ArgumentException($"Cannot compare the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;

            Type type = other.GetType();

            if (type == typeof(string)) return Equals((string) other);

            if (type == typeof(IPlayer)) return Equals((IPlayer) other);

            if (type == typeof(BaseEcoPlayer)) return Equals((BaseEcoPlayer) other);

            if (type == typeof(BaseEcoUser)) return Equals((BaseEcoUser) other);

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public override string ToString() => Id;
        public override int GetHashCode() => BitConverter.ToInt32(BitConverter.GetBytes(ulong.Parse(Id)), 4);

        public static bool operator ==(EcoPlayer p1, string p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, string p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, IIdentifiable p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, IIdentifiable p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, BaseEcoPlayer p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, BaseEcoPlayer p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, BaseEcoUser p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, BaseEcoUser p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, object p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, object p2) => !p1.Equals(p2);

        public static bool operator >(EcoPlayer p1, string p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, string p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, IIdentifiable p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, IIdentifiable p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, BaseEcoPlayer p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, BaseEcoPlayer p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, BaseEcoUser p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, BaseEcoUser p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, object p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, object p2) => p1.CompareTo(p2) < 0;

        public static bool operator >=(EcoPlayer p1, string p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, string p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, IIdentifiable p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, IIdentifiable p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, BaseEcoPlayer p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, BaseEcoPlayer p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, BaseEcoUser p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, BaseEcoUser p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, object p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, object p2) => p1.CompareTo(p2) <= 0;
    }
}