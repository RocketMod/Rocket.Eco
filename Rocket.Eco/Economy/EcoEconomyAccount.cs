using System;
using Rocket.API.Economy;
using Rocket.API.User;
using Rocket.Eco.Player;

namespace Rocket.Eco.Economy
{
    /// <inheritdoc />
    public sealed class EcoEconomyAccount : IEconomyAccount
    {
        internal EcoEconomyAccount(IIdentity player, IEconomyCurrency currency)
        {
            if (player == null || !(player is EcoPlayer ecoPlayer) && !(player is EcoUser))
                throw new ArgumentException("Must be of type \"EcoPlayer\".", nameof(player));

            Owner = player;
            Currency = currency;
        }

        /// <inheritdoc />
        public IIdentity Owner { get; }

        /// <inheritdoc />
        public IEconomyCurrency Currency { get; }

        /// <inheritdoc />
        public string Name => $"{Owner.Name}'s {Currency.Name} Account";

        /// <inheritdoc />
        public decimal Balance { get; }
    }
}