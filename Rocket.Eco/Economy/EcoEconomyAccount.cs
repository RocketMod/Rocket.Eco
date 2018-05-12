using System;
using Rocket.API.Economy;
using Rocket.API.User;
using Rocket.Eco.Player;

namespace Rocket.Eco.Economy
{
    public sealed class EcoEconomyAccount : IEconomyAccount
    {
        internal EcoEconomyAccount(IIdentity player, IEconomyCurrency currency)
        {
            if (player == null || !(player is EcoPlayer ecoPlayer) && !(player is EcoUser))
                throw new ArgumentException("Must be of type \"EcoPlayer\".", nameof(player));

            Owner = player;
            Currency = currency;
        }

        public IIdentity Owner { get; }
        public IEconomyCurrency Currency { get; }

        public string Name => $"{Owner.Name}'s {Currency.Name} Account";
        public decimal Balance { get; }
    }
}