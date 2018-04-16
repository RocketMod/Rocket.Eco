using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Gameplay.Players;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Player;

namespace Rocket.Eco.Player
{
    public class EcoPlayerManager : IPlayerManager
    {
        private readonly IRuntime runtime;

        public EcoPlayerManager(IRuntime runtime)
        {
            this.runtime = runtime;
        }

        public IEnumerable<IPlayer> Players => UserManager.Users.Select(user => new EcoPlayer(user.Player, runtime.Container)).Cast<IPlayer>().ToList();

        public IPlayer GetPlayer(string uniqueId)
        {
            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == uniqueId) ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && x.SteamId == uniqueId);

            return user == null ? null : new EcoPlayer(user.Player, runtime.Container);
        }

        public bool TryGetPlayer(string uniqueId, out IPlayer output)
        {
            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == uniqueId) ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && x.SteamId == uniqueId);

            if (user == null)
            {
                output = null;
                return false;
            }

            output = new EcoPlayer(user.Player, runtime.Container);
            return true;
        }

        public IPlayer GetPlayerByName(string name)
        {
            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Contains(name))
                ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?? UserManager.Users.FirstOrDefault(x => !x.LoggedIn && (x.Name ?? string.Empty).Contains(name));

            return user == null ? null : new EcoPlayer(user.Player, runtime.Container);
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

            output = new EcoPlayer(user.Player, runtime.Container);
            return true;
        }

        public bool Ban(IPlayer player, ICommandCaller caller, string reason, TimeSpan? timeSpan = null) => throw new NotImplementedException();

        public bool Kick(IPlayer player, ICommandCaller caller, string reason) => throw new NotImplementedException();
    }
}