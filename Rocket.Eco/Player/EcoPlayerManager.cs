using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Core.Player.Events;
using Rocket.Eco.API;
using Rocket.Eco.Extensions;
using Color = System.Drawing.Color;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IPlayerManager" />
    public sealed class EcoPlayerManager : ContainerAccessor, IPlayerManager
    {
        //TODO: Migrate to a thread-safe collection.
        internal readonly List<EcoPlayer> _Players = new List<EcoPlayer>();

        /// <inheritdoc />
        public EcoPlayerManager(IDependencyContainer container) : base(container)
        {
            foreach (User user in UserManager.Users)
                _Players.Add(new EcoPlayer(user, Container));
        }

        /// <inheritdoc />
        public IEnumerable<IPlayer> OnlinePlayers => _Players.Where(x => x.IsOnline);

        /// <inheritdoc />
        public IEnumerable<IUser> Users => _Players.Select(x => x.User);

        /// <inheritdoc />
        public IPlayer GetOnlinePlayer(string nameOrId) => throw new NotImplementedException();

        /// <inheritdoc />
        public IPlayer GetOnlinePlayerByName(string name) => throw new NotImplementedException();

        /// <inheritdoc />
        public IPlayer GetOnlinePlayerById(string id) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool TryGetOnlinePlayer(string nameOrId, out IPlayer output) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool TryGetOnlinePlayerById(string id, out IPlayer output) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool TryGetOnlinePlayerByName(string name, out IPlayer output) => throw new NotImplementedException();

        /// <inheritdoc />
        public IPlayer GetPlayer(string id) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool Kick(IUser user, IUser kickedBy = null, string reason = null)
        {
            if (!(user is EcoUser ecoUser))
                throw new ArgumentException("Must be of type `EcoUser`", nameof(user));

            if (!ecoUser.IsOnline)
                throw new InvalidOperationException("You cannot kick an offline player.");

            UserKickEvent e = new UserKickEvent(ecoUser, ecoUser, reason);
            Container.ResolveEventManager().Emit(Container.ResolveImplementation(), e);

            if (e.IsCancelled)
                return false;

            ecoUser.Player.InternalEcoUser.Client.Disconnect("You have been kicked.", reason ?? string.Empty, false);

            return true;
        }

        /// <inheritdoc />
        public bool Ban(IUserInfo player, IUser caller, string reason, TimeSpan? timeSpan = null)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(player.Id)) throw new ArgumentException("The argument has invalid members.", nameof(player));

            if (reason == null) reason = string.Empty;

            UserBanEvent e = new UserBanEvent(player, caller, reason, null);
            Container.ResolveEventManager().Emit(Container.ResolveImplementation(), e);

            if (e.IsCancelled) return false;

            if (player is EcoPlayer ecoPlayer && ecoPlayer.User != null)
            {
                //TODO: Currently only bans by SteamIDs
                if (!AddBanBlacklist(ecoPlayer.InternalEcoUser.SteamId)) return false;

                UserManager.Obj.SaveConfig();

                if (ecoPlayer.IsOnline) ecoPlayer.InternalEcoUser.Client.Disconnect("You have been banned.", reason, false);
            }
            else
            {
                if (!AddBanBlacklist(player.Id)) return false;

                UserManager.Obj.SaveConfig();
            }

            return true;
        }

        /// <inheritdoc />
        public bool Unban(IUserInfo user, IUser unbannedBy = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return RemoveBanBlacklist(user.Id);
        }

        /// <inheritdoc />
        public void SendMessage(IUser sender, IUser receiver, string message, Color? color = null, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Broadcast(IUser sender, IEnumerable<IUser> receivers, string message, Color? color = null, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Broadcast(IUser sender, string message, Color? color = null, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        private static bool AddBanBlacklist(string user) => !string.IsNullOrWhiteSpace(user) && UserManager.Config.BlackList.AddUnique(user);
        private static bool RemoveBanBlacklist(string user) => !string.IsNullOrWhiteSpace(user) && UserManager.Config.BlackList.Remove(user);
    }

    /// <inheritdoc />
    public sealed class EcoPlayerNotFoundException : PlayerNotFoundException
    {
        /// <inheritdoc />
        public EcoPlayerNotFoundException(string nameOrId) : base(nameOrId) { }
    }
}