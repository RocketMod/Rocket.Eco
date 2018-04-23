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

        public IEnumerable<IOnlinePlayer> OnlinePlayers => UserManager.Users.Where(x => x.LoggedIn).Select(user => new OnlineEcoPlayer(user.Player, runtime.Container)).ToList();

        public IPlayer GetPlayer(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            User user = UserManager.Users.FirstOrDefault(x => x.SteamId == id);

            if (user == null) return new EcoPlayer(id, runtime.Container);

            return user.LoggedIn ? new OnlineEcoPlayer(user.Player, runtime.Container) : new EcoPlayer(user, runtime.Container);
        }

        [Obsolete("Use `IEnumerable<IOnlinePlayer> OnlinePlayers` instead.")]
        public IEnumerable<IPlayer> Players => UserManager.Users.Where(x => x.LoggedIn).Select(user => new OnlineEcoPlayer(user.Player, runtime.Container)).Cast<IPlayer>().ToList();

        public IOnlinePlayer GetOnlinePlayer(string idOrName)
        {
            if (idOrName == null) throw new ArgumentNullException(nameof(idOrName));

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == idOrName)
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.Name.Equals(idOrName, StringComparison.InvariantCultureIgnoreCase))
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.Name.ComparerContains(idOrName));

            if (user == null) throw new PlayerNotFoundException(idOrName);

            return new OnlineEcoPlayer(user.Player, runtime.Container);
        }

        public bool TryGetOnlinePlayer(string idOrName, out IOnlinePlayer output)
        {
            if (idOrName == null)
            {
                output = null;
                return false;
            }

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == idOrName)
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.Name.Equals(idOrName, StringComparison.InvariantCultureIgnoreCase))
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.Name.ComparerContains(idOrName));

            if (user == null)
            {
                output = null;
                return false;
            }

            output = new OnlineEcoPlayer(user.Player, runtime.Container);
            return true;
        }

        public IOnlinePlayer GetOnlinePlayerById(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == id);

            if (user == null) throw new PlayerNotFoundException(id);

            return new OnlineEcoPlayer(user.Player, runtime.Container);
        }

        public bool TryGetOnlinePlayerById(string id, out IOnlinePlayer output)
        {
            if (id == null)
            {
                output = null;
                return false;
            }

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == id);

            if (user == null)
            {
                output = null;
                return false;
            }

            output = new OnlineEcoPlayer(user.Player, runtime.Container);
            return true;
        }

        public IOnlinePlayer GetOnlinePlayerByName(string displayName)
        {
            if (displayName == null) throw new ArgumentNullException(nameof(displayName));

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Equals(displayName, StringComparison.InvariantCultureIgnoreCase))
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).ComparerContains(displayName));

            if (user == null) throw new PlayerNotFoundException(displayName);

            return new OnlineEcoPlayer(user.Player, runtime.Container);
        }

        public bool TryGetOnlinePlayerByName(string displayName, out IOnlinePlayer output)
        {
            if (displayName == null)
            {
                output = null;
                return false;
            }

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Equals(displayName, StringComparison.InvariantCultureIgnoreCase))
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).ComparerContains(displayName));

            if (user == null)
            {
                output = null;
                return false;
            }

            output = new OnlineEcoPlayer(user.Player, runtime.Container);
            return true;
        }

        public bool Kick(IOnlinePlayer player, ICommandCaller caller = null, string reason = null) => throw new NotImplementedException();

        public bool Ban(IPlayer player, ICommandCaller caller, string reason, TimeSpan? timeSpan = null) => throw new NotImplementedException();

        public bool Kick(IPlayer player, ICommandCaller caller, string reason) => throw new NotImplementedException();
    }
}