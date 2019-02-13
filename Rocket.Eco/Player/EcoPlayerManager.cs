using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Eco.Shared.Utils;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Core.Player;
using Rocket.Core.Player.Events;
using Rocket.Core.User.Events;
using Rocket.Eco.API;
using Rocket.Eco.Extensions;
using Color = Rocket.API.Drawing.Color;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IPlayerManager" />
    public sealed class EcoPlayerManager : IPlayerManager
    {
        private readonly IDependencyContainer container;

        //TODO: Migrate to a thread-safe collection.
        internal readonly List<EcoPlayer> InternalPlayersList = new List<EcoPlayer>();

        /// <inheritdoc />
        public EcoPlayerManager(IDependencyContainer container)
        {
            this.container = container;

            foreach (User user in UserManager.Users)
                InternalPlayersList.Add(new EcoPlayer(user, this, container));
        }

        /// <inheritdoc />
        public string ServiceName => GetType().Name;

        /// <inheritdoc />
        public Task<IEnumerable<IPlayer>> GetPlayersAsync() => Task.FromResult(InternalPlayersList.Where(x => x.IsOnline).Cast<IPlayer>());

        /// <inheritdoc />
        public Task BroadcastAsync(IUser sender, string message, Color? color = null, params object[] arguments) => BroadcastAsync(sender, InternalPlayersList.Select(x => x.User), message, color, arguments);

        /// <inheritdoc />
        public Task<IIdentity> GetIdentity /*Async*/(string identity) => throw new NotImplementedException();

        /// <inheritdoc />
        public Task<IUser> GetUserAsync(string id)
        {
            if (TryGetOnlinePlayerById(id, out IPlayer p))
                return Task.FromResult(p.User);

            p = new EcoPlayer(id, this, container);
            InternalPlayersList.Add((EcoPlayer) p);

            return Task.FromResult(p.User);
        }

        /// <inheritdoc />
        public async Task<IPlayer> GetPlayerAsync(string nameOrId)
        {
            IEnumerable<EcoPlayer> players = (await GetPlayersAsync()).Cast<EcoPlayer>();

            return players.FirstOrDefault(x => x.Id.Equals(nameOrId))
                ?? players.FirstOrDefault(x => x.Name.Equals(nameOrId, StringComparison.InvariantCultureIgnoreCase))
                ?? players.FirstOrDefault(x => x.Name.ComparerContains(nameOrId))
                ?? throw new PlayerNotFoundException(nameOrId);
        }

        /// <inheritdoc />
        public async Task<IPlayer> GetPlayerByNameAsync(string name)
        {
            IEnumerable<EcoPlayer> players = (await GetPlayersAsync()).Cast<EcoPlayer>();

            return players.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?? players.FirstOrDefault(x => x.Name.ComparerContains(name))
                ?? throw new PlayerNameNotFoundException(name);
        }

        /// <inheritdoc />
        public async Task<IPlayer> GetPlayerByIdAsync(string id)
        {
            IEnumerable<EcoPlayer> players = (await GetPlayersAsync()).Cast<EcoPlayer>();

            return players.FirstOrDefault(x => x.Id.Equals(id))
                ?? throw new PlayerIdNotFoundException(id);
        }

        /// <inheritdoc />
        public bool TryGetOnlinePlayer(string nameOrId, out IPlayer output)
        {
            IEnumerable<EcoPlayer> players = GetPlayersAsync().GetAwaiter().GetResult().Cast<EcoPlayer>();

            EcoPlayer player = players.FirstOrDefault(x => x.Id.Equals(nameOrId))
                ?? players.FirstOrDefault(x => x.Name.Equals(nameOrId, StringComparison.InvariantCultureIgnoreCase))
                ?? players.FirstOrDefault(x => x.Name.ComparerContains(nameOrId));

            output = player;

            return player != null;
        }

        /// <inheritdoc />
        public bool TryGetOnlinePlayerById(string id, out IPlayer output)
        {
            IEnumerable<EcoPlayer> players = GetPlayersAsync().GetAwaiter().GetResult().Cast<EcoPlayer>();

            EcoPlayer player = players.FirstOrDefault(x => x.Id.Equals(id));

            output = player;

            return player != null;
        }

        /// <inheritdoc />
        public bool TryGetOnlinePlayerByName(string name, out IPlayer output)
        {
            IEnumerable<EcoPlayer> players = GetPlayersAsync().GetAwaiter().GetResult().Cast<EcoPlayer>();

            EcoPlayer player = players.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?? players.FirstOrDefault(x => x.Name.ComparerContains(name));

            output = player;

            return player != null;
        }
        
        /// <inheritdoc />
        public Task<bool> KickAsync(IUser user, IUser kickedBy = null, string reason = null)
        {
            if (!(user is EcoPlayerUser ecoUser))
                throw new ArgumentException("Must be of type `EcoUser`", nameof(user));

            if (!ecoUser.Player.IsOnline)
                throw new InvalidOperationException("You cannot kick an offline player.");

            PlayerKickEvent e = new PlayerKickEvent(ecoUser.Player, ecoUser, reason);
            container.Resolve<IEventBus>().Emit(container.Resolve<IHost>(), e);

            if (e.IsCancelled)
                return Task.FromResult(false);

            ecoUser.Player.InternalEcoUser.Client.Disconnect("You have been kicked.", reason ?? string.Empty);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> BanAsync(IUser player, IUser caller, string reason, TimeSpan? timeSpan = null)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(player.Id))
                throw new ArgumentException("The argument has invalid members.", nameof(player));

            if (reason == null)
                reason = string.Empty;

            UserBanEvent e = new UserBanEvent(player, caller, reason, null);
            container.Resolve<IEventBus>().Emit(container.Resolve<IHost>(), e);

            if (e.IsCancelled)
                return Task.FromResult(false);

            if (player is EcoPlayerUser ecoUser)
            {
                bool bothSucceed = false;

                if (ecoUser.Player.UserIdType == UserIdType.Both)
                    bothSucceed = AddBanBlacklist(ecoUser.Player.InternalEcoUser.SteamId);

                if (!AddBanBlacklist(ecoUser.Id) && !bothSucceed)
                    return Task.FromResult(false);

                UserManager.Obj.SaveConfig();

                if (ecoUser.Player.IsOnline)
                    ecoUser.Player.InternalEcoUser.Client.Disconnect("You have been banned.", reason);
            }
            else
            {
                if (!AddBanBlacklist(player.Id))
                    return Task.FromResult(false);

                UserManager.Obj.SaveConfig();
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> UnbanAsync(IUser user, IUser unbannedBy = null)
        {
            switch (user)
            {
                case null:
                    throw new ArgumentNullException(nameof(user));
                case EcoPlayerUser ecoUser when ecoUser.Player.UserIdType == UserIdType.Both:
                    RemoveBanBlacklist(ecoUser.Player.InternalEcoUser.SteamId);
                    break;
            }

            return Task.FromResult(RemoveBanBlacklist(user.Id));
        }

        /// <inheritdoc />
        public Task SendMessageAsync(IUser sender, IUser receiver, string message, Color? color = null, params object[] arguments)
        {
            if (!(receiver is EcoPlayerUser ecoUser))
            {
                if (!(receiver is IConsole console))
                    throw new ArgumentException("Must be of type `EcoUser`.", nameof(receiver));

                console.WriteLine(string.IsNullOrWhiteSpace(sender?.DisplayName) ? message : $"[{sender.DisplayName}] {message}", arguments);
                return Task.CompletedTask;
            }

            if (!ecoUser.Player.IsOnline)
                throw new ArgumentException("Must be online.", nameof(receiver));

            string formattedMessage = string.Format(string.IsNullOrWhiteSpace(sender?.DisplayName) ? message : $"[{sender.DisplayName}] {message}", arguments);

            ChatManager.ServerMessageToPlayerAlreadyLocalized(formattedMessage, ecoUser.Player.InternalEcoUser);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task BroadcastAsync(IUser sender, IEnumerable<IUser> receivers, string message, Color? color = null, params object[] arguments)
        {
            List<EcoPlayerUser> users = new List<EcoPlayerUser>();

            foreach (IUser user in receivers)
            {
                if (!(user is EcoPlayerUser ecoUser))
                    throw new ArgumentException("Every enumeration must be of type `EcoUser`.", nameof(receivers));

                if (!ecoUser.Player.IsOnline)
                    throw new ArgumentException("Every enumeration must be online.", nameof(receivers));

                users.Add(ecoUser);
            }

            string formattedMessage = string.Format(string.IsNullOrWhiteSpace(sender?.DisplayName) ? message : $"[{sender.DisplayName}] {message}", arguments);

            foreach (EcoPlayerUser ecoUser in users) ChatManager.ServerMessageToPlayerAlreadyLocalized(formattedMessage, ecoUser.Player.InternalEcoUser);

            return Task.CompletedTask;
        }

        private static bool AddBanBlacklist(string user) => !string.IsNullOrWhiteSpace(user) && UserManager.Config.BlackList.AddUnique(user);
        private static bool RemoveBanBlacklist(string user) => !string.IsNullOrWhiteSpace(user) && UserManager.Config.BlackList.Remove(user);
    }
}