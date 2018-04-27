using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Shared.Utils;
using Eco.Gameplay.Players;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;
using Rocket.Eco.API;
using Eco.Core.Plugins.Interfaces;

namespace Rocket.Eco.Player
{
    public sealed class EcoPlayerManager : ContainerAccessor, IPlayerManager
    {
        public EcoPlayerManager(IDependencyContainer container) : base(container) { }

        public IEnumerable<IOnlinePlayer> OnlinePlayers => UserManager.Users.Where(x => x.LoggedIn).Select(user => new OnlineEcoPlayer(user.Player, Container)).ToList();

        public IPlayer GetPlayer(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            User user = UserManager.Users.FirstOrDefault(x => x.SteamId == id);

            if (user == null) return new EcoPlayer(id, Container);

            return user.LoggedIn ? new OnlineEcoPlayer(user.Player, Container) : new EcoPlayer(user, Container);
        }

        public IOnlinePlayer GetOnlinePlayer(string idOrName)
        {
            if (idOrName == null) throw new ArgumentNullException(nameof(idOrName));

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == idOrName)
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.Name.Equals(idOrName, StringComparison.InvariantCultureIgnoreCase))
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.Name.ComparerContains(idOrName));

            if (user == null) throw new PlayerNotFoundException(idOrName);

            return new OnlineEcoPlayer(user.Player, Container);
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

            output = new OnlineEcoPlayer(user.Player, Container);
            return true;
        }

        public IOnlinePlayer GetOnlinePlayerById(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && x.SteamId == id);

            if (user == null) throw new PlayerNotFoundException(id);

            return new OnlineEcoPlayer(user.Player, Container);
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

            output = new OnlineEcoPlayer(user.Player, Container);
            return true;
        }

        public IOnlinePlayer GetOnlinePlayerByName(string displayName)
        {
            if (displayName == null) throw new ArgumentNullException(nameof(displayName));

            User user = UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).Equals(displayName, StringComparison.InvariantCultureIgnoreCase))
                ?? UserManager.Users.FirstOrDefault(x => x.LoggedIn && (x.Name ?? string.Empty).ComparerContains(displayName));

            if (user == null) throw new PlayerNotFoundException(displayName);

            return new OnlineEcoPlayer(user.Player, Container);
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

            output = new OnlineEcoPlayer(user.Player, Container);
            return true;
        }

        public bool Kick(IOnlinePlayer player, ICommandCaller caller = null, string reason = null)
        {
            if (player is OnlineEcoPlayer ecoPlayer && player.IsOnline)
            {
                ecoPlayer.User.Client.Disconnect("You have been kicked.", reason ?? string.Empty, false);
                return true;
            }

            return false;
        }

        public bool Ban(IPlayer player, ICommandCaller caller, string reason, TimeSpan? timeSpan = null)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(player.Id)) throw new ArgumentException("The argument has invalid members.", nameof(player));

            if (reason == null) reason = string.Empty;

            if (player is EcoPlayer ecoPlayer && ecoPlayer.User != null)
            {
                if (AddBanBlacklist(ecoPlayer.User.SlgId) || AddBanBlacklist(ecoPlayer.User.SteamId))
                {
                    UserManager.Obj.SaveConfig();

                    if (player.IsOnline)
                    {
                        ecoPlayer.User.Client.Disconnect("You have been banned.", reason ?? string.Empty, false);
                    }

                    return true;
                }
            }
            else
            {
                if (AddBanBlacklist(player.Id))
                {
                    UserManager.Obj.SaveConfig();
                    return true;
                }
            }

            return false;
        }

        private bool AddBanBlacklist(string user)
        {
            if (string.IsNullOrWhiteSpace(user)) return false;

            return UserManager.Config.BlackList.AddUnique(user);
        }
    }
}