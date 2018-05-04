using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.Player;
using Rocket.Core.Player.Events;
using Rocket.Eco.API;

namespace Rocket.Eco.Player
{
    public sealed class EcoPlayerManager : ContainerAccessor, IPlayerManager
    {
        //TODO: Migrate to a thread-safe collection.
        internal readonly List<EcoPlayer> _Players = new List<EcoPlayer>();

        public EcoPlayerManager(IDependencyContainer container) : base(container)
        {
            foreach (User user in UserManager.Users) _Players.Add(user.LoggedIn ? new OnlineEcoPlayer(user.Player, Container) : new EcoPlayer(user, Container));
        }

        public IEnumerable<EcoPlayer> Players => _Players.AsReadOnly();

        public IEnumerable<IOnlinePlayer> OnlinePlayers => Players.Where(x => x.IsOnline).Cast<IOnlinePlayer>();

        public IPlayer GetPlayer(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return Players.FirstOrDefault(x => x.Id.Equals(id)) ?? new EcoPlayer(id, Container);
        }

        public IOnlinePlayer GetOnlinePlayer(string idOrName)
        {
            if (idOrName == null) throw new ArgumentNullException(nameof(idOrName));

            OnlineEcoPlayer player = (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Id == idOrName)
                ?? (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Name.Equals(idOrName, StringComparison.InvariantCultureIgnoreCase))
                ?? (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Name.ComparerContains(idOrName));

            if (player == null) throw new EcoPlayerNotFoundException(idOrName);

            return player;
        }

        public bool TryGetOnlinePlayer(string idOrName, out IOnlinePlayer output)
        {
            if (idOrName == null)
            {
                output = null;
                return false;
            }

            OnlineEcoPlayer player = (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Id == idOrName)
                ?? (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Name.Equals(idOrName, StringComparison.InvariantCultureIgnoreCase))
                ?? (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Name.ComparerContains(idOrName));

            if (player == null)
            {
                output = null;
                return false;
            }

            output = player;
            return true;
        }

        public IOnlinePlayer GetOnlinePlayerById(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            OnlineEcoPlayer player = (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Id == id);

            if (player == null) throw new EcoPlayerNotFoundException(id);

            return player;
        }

        public bool TryGetOnlinePlayerById(string id, out IOnlinePlayer output)
        {
            if (id == null)
            {
                output = null;
                return false;
            }

            OnlineEcoPlayer player = (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Id == id);

            if (player == null)
            {
                output = null;
                return false;
            }

            output = player;
            return true;
        }

        public IOnlinePlayer GetOnlinePlayerByName(string displayName)
        {
            if (displayName == null) throw new ArgumentNullException(nameof(displayName));

            OnlineEcoPlayer player = (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Name.Equals(displayName, StringComparison.InvariantCultureIgnoreCase))
                ?? (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Name.ComparerContains(displayName));

            if (player == null) throw new EcoPlayerNotFoundException(displayName);

            return player;
        }

        public bool TryGetOnlinePlayerByName(string displayName, out IOnlinePlayer output)
        {
            if (displayName == null)
            {
                output = null;
                return false;
            }

            OnlineEcoPlayer player = (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Name.Equals(displayName, StringComparison.InvariantCultureIgnoreCase))
                ?? (OnlineEcoPlayer) _Players.FirstOrDefault(x => x.IsOnline && x.Name.ComparerContains(displayName));

            if (player == null)
            {
                output = null;
                return false;
            }

            output = player;
            return true;
        }

        public bool Kick(IOnlinePlayer player, ICommandCaller caller = null, string reason = null)
        {
            if (!(player is OnlineEcoPlayer ecoPlayer) || !player.IsOnline) return false;

            PlayerKickEvent e = new PlayerKickEvent(player, caller, reason);
            Container.Resolve<IEventManager>().Emit(Container.Resolve<IImplementation>(), e);

            if (e.IsCancelled) return false;

            ecoPlayer.User.Client.Disconnect("You have been kicked.", reason ?? string.Empty, false);
            return true;
        }

        public bool Ban(IPlayer player, ICommandCaller caller, string reason, TimeSpan? timeSpan = null)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(player.Id)) throw new ArgumentException("The argument has invalid members.", nameof(player));

            if (reason == null) reason = string.Empty;

            PlayerBanEvent e = new PlayerBanEvent(player, caller, reason, null);
            Container.Resolve<IEventManager>().Emit(Container.Resolve<IImplementation>(), e);

            if (e.IsCancelled) return false;

            if (player is EcoPlayer ecoPlayer && ecoPlayer.User != null)
            {
                if (!AddBanBlacklist(ecoPlayer.User.SlgId) && !AddBanBlacklist(ecoPlayer.User.SteamId)) return false;

                UserManager.Obj.SaveConfig();

                if (player.IsOnline) ecoPlayer.User.Client.Disconnect("You have been banned.", reason, false);
            }
            else
            {
                if (!AddBanBlacklist(player.Id)) return false;

                UserManager.Obj.SaveConfig();
            }

            return true;
        }

        public bool Unban(IPlayer player, ICommandCaller caller = null) => !RemoveBanBlacklist(player.Id) || player is EcoPlayer ecoPlayer && ecoPlayer.User?.SlgId != null && RemoveBanBlacklist(ecoPlayer.User.SlgId);

        private static bool AddBanBlacklist(string user) => !string.IsNullOrWhiteSpace(user) && UserManager.Config.BlackList.AddUnique(user);
        private static bool RemoveBanBlacklist(string user) => !string.IsNullOrWhiteSpace(user) && UserManager.Config.BlackList.Remove(user);
    }

    public sealed class EcoPlayerNotFoundException : PlayerNotFoundException
    {
        public EcoPlayerNotFoundException(string nameOrId) : base(nameOrId) { }
    }
}