using System;
using System.Collections.Generic;
using System.Linq;

using Rocket.API.Player;

using Eco.Gameplay.Players;
using BasePlayer = Eco.Gameplay.Players.Player;
using Rocket.API.Commands;

namespace Rocket.Eco.Player
{
    public class PlayerNotFoundException : Exception
    {
        public PlayerNotFoundException() : base($"Could not find the requested player.") { }
    }

    public class EcoPlayerManager : IPlayerManager
    {
        public IEnumerable<IPlayer> Players
        {
            get
            {
                List<IPlayer> players = new List<IPlayer>();

                foreach (User user in UserManager.Users)
                {
                    players.Add(new EcoPlayer(user.Player));
                }

                return players;
            }
        }

        public bool Ban(IPlayer player, ICommandCaller caller, string reason, TimeSpan? timeSpan = null)
        {
            throw new NotImplementedException();
            //UserManager.Ban()
        }

        public bool Kick(IPlayer player, ICommandCaller caller, string reason)
        {
            if (player is BasePlayer ecoPlayer)
            {
                //TODO: I'll need to find a replacement for that second user.
                //UserManager.Kick(ecoPlayer.User, ecoPlayer.User, reason);
            }

            return false;
        }

        public IPlayer GetPlayer(string uniqueID)
        {
            User user =
                UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == uniqueID) ??
                UserManager.Users.FirstOrDefault(x => !x.LoggedIn && x.SteamId == uniqueID);

            if (user == null)
            {
                throw new PlayerNotFoundException();
            }

            return new EcoPlayer(user.Player);
        }

        public bool TryGetPlayer(string uniqueID, out IPlayer output)
        {
            User user =
                UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == uniqueID) ??
                UserManager.Users.FirstOrDefault(x => !x.LoggedIn && x.SteamId == uniqueID);

            if (user == null)
            {
                output = null;
                return false;
            }

            output = new EcoPlayer(user.Player);
            return true;
        }

        public IPlayer GetPlayerByName(string name)
        {
            User user =
                UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase)) ??
                UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Contains(name)) ??
                UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase)) ??
                UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Contains(name));

            if (user == null)
            {
                throw new PlayerNotFoundException();
            }

            return new EcoPlayer(user.Player);
        }

        public bool TryGetPlayerByName(string name, out IPlayer output)
        {
            User user =
                 UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase)) ??
                 UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Contains(name)) ??
                 UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase)) ??
                 UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Contains(name));

            if (user == null)
            {
                output = null;
                return false;
            }

            output = new EcoPlayer(user.Player);
            return true;
        }
    }
}
