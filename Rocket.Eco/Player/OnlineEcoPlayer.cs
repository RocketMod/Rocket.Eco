﻿using System;
using System.Runtime.CompilerServices;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.Player;
using BaseEcoPlayer = Eco.Gameplay.Players.Player;

namespace Rocket.Eco.Player
{
    public sealed class OnlineEcoPlayer : EcoPlayer, IOnlinePlayer, IComparable<BaseEcoPlayer>, IEquatable<BaseEcoPlayer>
    {
        internal OnlineEcoPlayer(BaseEcoPlayer player, IDependencyContainer container) : base(player?.User, container) { }

        public OnlineEcoPlayer(EcoPlayer player, IDependencyContainer container) : base(player.User, container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            if (player.User == null || !player.IsOnline) throw new InvalidOperationException("The player must be online to cast to an OnlineEcoPlayer");
        }

        public BaseEcoPlayer Player
        {
            get
            {
                if (IsOnline) return User.Player;

                throw new InvalidOperationException("This player reference is now offline.");
            }
        }

        public int CompareTo(BaseEcoPlayer other) => other == null ? 1 : string.Compare(Id, other.User.SteamId, StringComparison.InvariantCulture);
        public bool Equals(BaseEcoPlayer other) => other != null && Id.Equals(other.User.SteamId, StringComparison.InvariantCulture);

        public double Health
        {
            get => -1;
            set => Container.Get<ILogger>().LogWarning("Setting player health is not supported in Eco!");
        }

        public double MaxHealth
        {
            get => -1;
            set => Container.Get<ILogger>().LogWarning("Setting player health is not supported in Eco!");
        }

        public void SendMessage(string message, ConsoleColor? color = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (IsOnline) User.Player.SendTemporaryMessage(FormattableStringFactory.Create(message));

            throw new InvalidOperationException("This player reference is currently offline.");
        }

        public void SendErrorMessage(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (IsOnline) User.Player.SendTemporaryError(FormattableStringFactory.Create(message));

            throw new InvalidOperationException("This player reference is currently offline.");
        }
    }
}