using System;
using System.Runtime.CompilerServices;

using Eco.Shared.Services;

using Rocket.API.Player;

using BasePlayer = Eco.Gameplay.Players.Player;
using BaseUser = Eco.Gameplay.Players.User;

namespace Rocket.Eco.Player
{
    public sealed class EcoPlayer : IPlayer, IComparable, IComparable<BasePlayer>, IEquatable<BasePlayer>, IComparable<BaseUser>, IEquatable<BaseUser>
    {
        public string UniqueID => User.SteamId;
        public string Name => User.Name;
        public bool IsAdmin => User.IsAdmin;
        
        public BaseUser User => Player.User;
        public bool IsDev => User.IsDev;
        public bool IsOnline => User.LoggedIn;

        public BasePlayer Player { get; }
        
        public string Id { get => UniqueID; set => throw new NotImplementedException(); }

        internal EcoPlayer(BasePlayer player)
        {
            Player = player;
        }

        public void SendMessage(string message)
        {
            Player.SendTemporaryMessage(FormattableStringFactory.Create(message), ChatCategory.Info);
        }

        public int CompareTo(IPlayer other)
        {
            if (other == null)
            {
                return 1;
            }

            return UniqueID.CompareTo(other.UniqueID);
        }

        public bool Equals(IPlayer other)
        {
            if (other == null)
            {
                return false;
            }

            return UniqueID.Equals(other.UniqueID);
        }

        public int CompareTo(BasePlayer other)
        {
            if (other == null)
            {
                return 1;
            }

            return UniqueID.CompareTo(other.User.SteamId);
        }

        public bool Equals(BasePlayer other)
        {
            if (other == null)
            {
                return false;
            }

            return UniqueID.Equals(other.User.SteamId);
        }

        public int CompareTo(BaseUser other)
        {
            if (other == null)
            {
                return 1;
            }

            return UniqueID.CompareTo(other.SteamId);
        }

        public bool Equals(BaseUser other)
        {
            if (other == null)
            {
                return false;
            }

            return UniqueID.Equals(other.SteamId);
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

        public int CompareTo(string other) => UniqueID.CompareTo(other);
        public bool Equals(string other) => UniqueID.Equals(other);

        public override string ToString() => UniqueID;
        public override int GetHashCode() => BitConverter.ToInt32(BitConverter.GetBytes(ulong.Parse(UniqueID)), 4);

        public static bool operator ==(EcoPlayer p1, string p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, string p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, IPlayer p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, IPlayer p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, BasePlayer p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, BasePlayer p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, BaseUser p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, BaseUser p2) => !p1.Equals(p2);

        public static bool operator ==(EcoPlayer p1, object p2) => p1.Equals(p2);
        public static bool operator !=(EcoPlayer p1, object p2) => !p1.Equals(p2);
        
        public static bool operator >(EcoPlayer p1, string p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, string p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, IPlayer p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, IPlayer p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, BasePlayer p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, BasePlayer p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, BaseUser p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, BaseUser p2) => p1.CompareTo(p2) < 0;

        public static bool operator >(EcoPlayer p1, object p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(EcoPlayer p1, object p2) => p1.CompareTo(p2) < 0;

        public static bool operator >=(EcoPlayer p1, string p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, string p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, IPlayer p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, IPlayer p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, BasePlayer p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, BasePlayer p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, BaseUser p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, BaseUser p2) => p1.CompareTo(p2) <= 0;

        public static bool operator >=(EcoPlayer p1, object p2) => p1.CompareTo(p2) >= 0;
        public static bool operator <=(EcoPlayer p1, object p2) => p1.CompareTo(p2) <= 0;
    }
}