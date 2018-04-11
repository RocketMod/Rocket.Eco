using System;
using System.Runtime.CompilerServices;

using Rocket.API.Permissions;
using Rocket.API.Player;

using Eco.Shared.Services;

using BasePlayer = Eco.Gameplay.Players.Player;
using BaseUser = Eco.Gameplay.Players.User;

namespace Rocket.Eco.Player
{
    public sealed class EcoPlayer : IPlayer, IComparable<BasePlayer>, IEquatable<BasePlayer>, IComparable<BaseUser>, IEquatable<BaseUser>
    {
        public string Id => User.SteamId; 
        public string Name => User.Name;
        public bool IsAdmin => User.IsAdmin;
        
        public BaseUser User => Player.User;
        public bool IsDev => User.IsDev;
        public bool IsOnline => User.LoggedIn;

        public BasePlayer Player { get; }

        public Type PlayerType => typeof(EcoPlayer);

        internal EcoPlayer(BasePlayer player)
        {
            Player = player;
        }

        public void SendMessage(string message)
        {
            Player.SendTemporaryMessage(FormattableStringFactory.Create(message), ChatCategory.Info);
        }

        public int CompareTo(IIdentifiable other)
        {
            if (other == null)
            {
                return 1;
            }

            return Id.CompareTo(other.Id);
        }

        public bool Equals(IIdentifiable other)
        {
            if (other == null)
            {
                return false;
            }

            return Id.Equals(other.Id);
        }

        public int CompareTo(BasePlayer other)
        {
            if (other == null)
            {
                return 1;
            }

            return Id.CompareTo(other.User.SteamId);
        }

        public bool Equals(BasePlayer other)
        {
            if (other == null)
            {
                return false;
            }

            return Id.Equals(other.User.SteamId);
        }

        public int CompareTo(BaseUser other)
        {
            if (other == null)
            {
                return 1;
            }

            return Id.CompareTo(other.SteamId);
        }

        public bool Equals(BaseUser other)
        {
            if (other == null)
            {
                return false;
            }

            return Id.Equals(other.SteamId);
        }

        public override bool Equals(object other)
        {
            Type type = other.GetType();

            if (type == typeof(string))
            {
                return Equals((string)other);
            }

            if (type == typeof(IPlayer))
            {
                return Equals((IPlayer)other);
            }

            if (type == typeof(BasePlayer))
            {
                return Equals((BasePlayer)other);
            }

            if (type == typeof(BaseUser))
            {
                return Equals((BaseUser)other);
            }

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public int CompareTo(object other)
        {
            Type type = other.GetType();

            if (type == typeof(string))
            {
                return CompareTo((string)other);
            }

            if (type == typeof(IPlayer))
            {
                return CompareTo((IPlayer)other);
            }

            if (type == typeof(BasePlayer))
            {
                return CompareTo((BasePlayer)other);
            }

            if (type == typeof(BaseUser))
            {
                return CompareTo((BaseUser)other);
            }

            throw new ArgumentException($"Cannot compare the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public int CompareTo(string other) => Id.CompareTo(other);
        public bool Equals(string other) => Id.Equals(other);

        public override string ToString() => Id;
        public override int GetHashCode() => BitConverter.ToInt32(BitConverter.GetBytes(ulong.Parse(Id)), 4);

        public static bool operator ==(EcoPlayer p1, string p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, string p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, IIdentifiable p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, IIdentifiable p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, BasePlayer p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, BasePlayer p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, BaseUser p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, BaseUser p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, object p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, object p2) => !p1.Equals(p2);
        
        public static bool operator >(EcoPlayer p1, string p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, string p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, IIdentifiable p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, IIdentifiable p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, BasePlayer p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, BasePlayer p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, BaseUser p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, BaseUser p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, object p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, object p2) => p1.CompareTo(p2) < 0;

        public static bool operator >=(EcoPlayer p1, string p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, string p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, IIdentifiable p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, IIdentifiable p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, BasePlayer p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, BasePlayer p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, BaseUser p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, BaseUser p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, object p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, object p2) => p1.CompareTo(p2) <= 0;
    }
}