using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Gameplay.Players;
using Rocket.API.Commands;
using Rocket.API.Player;

namespace Rocket.Eco.Player
{
    public class PlayerNotFoundException : Exception
    {
        public PlayerNotFoundException() : base($"Could not find the requested player.") { }
    }

    public class EcoPlayerManager : IPlayerManager
    {
        public IEnumerable<IPlayer> Players => UserManager.Users.Select(user => new EcoPlayer(user.Player)).Cast<IPlayer>().ToList();

        public IPlayer GetPlayer(string uniqueID)
        {
            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == uniqueID) ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && x.SteamId == uniqueID);

            if (user == null) throw new PlayerNotFoundException();

            return new EcoPlayer(user.Player);
        }

        public bool TryGetPlayer(string uniqueID, out IPlayer output)
        {
            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == uniqueID) ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && x.SteamId == uniqueID);

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
            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase)) 
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Contains(name)) 
                ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase)) 
                ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Contains(name));

            if (user == null) throw new PlayerNotFoundException();

            return new EcoPlayer(user.Player);
        }

        public bool TryGetPlayerByName(string name, out IPlayer output)
        {
            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase)) 
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Contains(name)) 
                ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase)) 
                ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Contains(name));

            if (user == null)
            {
                output = null;
                return false;
            }

            output = new EcoPlayer(user.Player);
            return true;
        }

        public bool Ban(IPlayer player, ICommandCaller caller, string reason, TimeSpan? timeSpan = null) => throw new NotImplementedException();

        public bool Kick(IPlayer player, ICommandCaller caller, string reason) => throw new NotImplementedException();
    }
}