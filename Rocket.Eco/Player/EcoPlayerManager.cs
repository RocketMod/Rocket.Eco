using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Eco.Shared.Utils;
using Rocket.API.Commands;
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
        public IPlayer GetOnlinePlayer(string nameOrId)
        {
            IEnumerable<EcoPlayer> players = OnlinePlayers.Cast<EcoPlayer>();

            return players.FirstOrDefault(x => x.Id.Equals(nameOrId))
                ?? players.FirstOrDefault(x => x.Name.Equals(nameOrId, StringComparison.InvariantCultureIgnoreCase))
                ?? players.FirstOrDefault(x => x.Name.ComparerContains(nameOrId))
                ?? throw new EcoPlayerNotFoundException(nameOrId);
        }

        /// <inheritdoc />
        public IPlayer GetOnlinePlayerByName(string name)
        {
            IEnumerable<EcoPlayer> players = OnlinePlayers.Cast<EcoPlayer>();

            return players.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?? players.FirstOrDefault(x => x.Name.ComparerContains(name))
                ?? throw new EcoPlayerNotFoundException(name);
        }

        /// <inheritdoc />
        public IPlayer GetOnlinePlayerById(string id)
        {
            IEnumerable<EcoPlayer> players = OnlinePlayers.Cast<EcoPlayer>();

            return players.FirstOrDefault(x => x.Id.Equals(id))
                ?? throw new EcoPlayerNotFoundException(id);
        }

        /// <inheritdoc />
        public bool TryGetOnlinePlayer(string nameOrId, out IPlayer output)
        {
            IEnumerable<EcoPlayer> players = OnlinePlayers.Cast<EcoPlayer>();

            EcoPlayer player = players.FirstOrDefault(x => x.Id.Equals(nameOrId))
                ?? players.FirstOrDefault(x => x.Name.Equals(nameOrId, StringComparison.InvariantCultureIgnoreCase))
                ?? players.FirstOrDefault(x => x.Name.ComparerContains(nameOrId));

            output = player;

            return player != null;
        }

        /// <inheritdoc />
        public bool TryGetOnlinePlayerById(string id, out IPlayer output)
        {
            IEnumerable<EcoPlayer> players = OnlinePlayers.Cast<EcoPlayer>();

            EcoPlayer player = players.FirstOrDefault(x => x.Id.Equals(id));

            output = player;

            return player != null;
        }

        /// <inheritdoc />
        public bool TryGetOnlinePlayerByName(string name, out IPlayer output)
        {
            IEnumerable<EcoPlayer> players = OnlinePlayers.Cast<EcoPlayer>();

            EcoPlayer player = players.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?? players.FirstOrDefault(x => x.Name.ComparerContains(name));

            output = player;

            return player != null;
        }

        /// <inheritdoc />
        public IPlayer GetPlayer(string id) => TryGetOnlinePlayerById(id, out IPlayer p) ? p : new EcoPlayer(id, Container);

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

            ecoUser.Player.InternalEcoUser.Client.Disconnect("You have been kicked.", reason ?? string.Empty);

            return true;
        }

        /// <inheritdoc />
        public bool Ban(IUserInfo player, IUser caller, string reason, TimeSpan? timeSpan = null)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(player.Id))
                throw new ArgumentException("The argument has invalid members.", nameof(player));

            if (reason == null)
                reason = string.Empty;

            UserBanEvent e = new UserBanEvent(player, caller, reason, null);
            Container.ResolveEventManager().Emit(Container.ResolveImplementation(), e);

            if (e.IsCancelled)
                return false;

            if (player is EcoPlayer ecoPlayer && ecoPlayer.User != null)
            {
                bool bothSucceed = false;

                if (ecoPlayer.UserIdType == EUserIdType.Both)
                    bothSucceed = AddBanBlacklist(ecoPlayer.InternalEcoUser.SteamId);

                if (!AddBanBlacklist(ecoPlayer.Id) && !bothSucceed)
                    return false;

                UserManager.Obj.SaveConfig();

                if (ecoPlayer.IsOnline)
                    ecoPlayer.InternalEcoUser.Client.Disconnect("You have been banned.", reason);
            }
            else
            {
                if (!AddBanBlacklist(player.Id))
                    return false;

                UserManager.Obj.SaveConfig();
            }

            return true;
        }

        /// <inheritdoc />
        public bool Unban(IUserInfo user, IUser unbannedBy = null)
        {
            switch (user)
            {
                case null:
                    throw new ArgumentNullException(nameof(user));
                case EcoPlayer ecoPlayer when ecoPlayer.UserIdType == EUserIdType.Both:
                    RemoveBanBlacklist(ecoPlayer.InternalEcoUser.SteamId);
                    break;
            }

            return RemoveBanBlacklist(user.Id);
        }

        /// <inheritdoc />
        public void SendMessage(IUser sender, IUser receiver, string message, Color? color = null, params object[] arguments)
        {
            if (!(receiver is EcoUser ecoUser))
            {
                if (!(receiver is IConsole console))
                    throw new ArgumentException("Must be of type `EcoUser`.", nameof(receiver));

                console.WriteLine(string.IsNullOrWhiteSpace(sender?.Name) ? message : $"[{sender.Name}] {message}", arguments);
                return;
            }

            if (!ecoUser.IsOnline)
                throw new ArgumentException("Must be online.", nameof(receiver));

            string formattedMessage = string.Format(string.IsNullOrWhiteSpace(sender?.Name) ? message : $"[{sender.Name}] {message}", arguments);

            ChatManager.ServerMessageToPlayerAlreadyLocalized(formattedMessage, ecoUser.Player.InternalEcoUser);
        }

        /// <inheritdoc />
        public void Broadcast(IUser sender, IEnumerable<IUser> receivers, string message, Color? color = null, params object[] arguments)
        {
            List<EcoUser> users = new List<EcoUser>();

            foreach (IUser user in receivers)
            {
                if (!(user is EcoUser ecoUser))
                    throw new ArgumentException("Every enumeration must be of type `EcoUser`.", nameof(receivers));

                if (!ecoUser.IsOnline)
                    throw new ArgumentException("Every enumeration must be online.", nameof(receivers));

                users.Add(ecoUser);
            }

            string formattedMessage = string.Format(string.IsNullOrWhiteSpace(sender?.Name) ? message : $"[{sender.Name}] {message}", arguments);

            foreach (EcoUser ecoUser in users) ChatManager.ServerMessageToPlayerAlreadyLocalized(formattedMessage, ecoUser.Player.InternalEcoUser);
        }

        /// <inheritdoc />
        public void Broadcast(IUser sender, string message, Color? color = null, params object[] arguments)
        {
            string formattedMessage = string.Format(string.IsNullOrWhiteSpace(sender?.Name) ? message : $"[{sender.Name}] {message}", arguments);

            foreach (EcoPlayer ecoPlayer in OnlinePlayers.Cast<EcoPlayer>()) ChatManager.ServerMessageToPlayerAlreadyLocalized(formattedMessage, ecoPlayer.InternalEcoUser);
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