using System;

using Rocket.API.Player;

using BasePlayer = Eco.Gameplay.Players.Player;

namespace Rocket.Eco.Player
{
    public sealed class EcoPlayer : IPlayer, IComparable<BasePlayer>, IEquatable<BasePlayer>
    {
        public string UniqueID => Player.User.SteamId;
        public string DisplayName => Player.User.Name;
        public bool IsAdmin => Player.User.IsAdmin;

        public bool IsDev => Player.User.IsDev;
        public bool IsOnline => Player.User.LoggedIn;

        public BasePlayer Player { get; private set; }

        internal EcoPlayer(BasePlayer player)
        {
            Player = player;
        }

        public int CompareTo(IPlayer other)
        {
            return UniqueID.CompareTo(other.UniqueID);
        }

        public int CompareTo(string other)
        {
            return UniqueID.CompareTo(other);
        }

        public bool Equals(IPlayer other)
        {
            return UniqueID.Equals(other.UniqueID);
        }

        public bool Equals(string other)
        {
            return UniqueID.Equals(other);
        }

        public int CompareTo(BasePlayer other)
        {
            return UniqueID.CompareTo(other.User.SteamId);
        }

        public bool Equals(BasePlayer other)
        {
            return UniqueID.Equals(other.User.SteamId);
        }
    }
}
