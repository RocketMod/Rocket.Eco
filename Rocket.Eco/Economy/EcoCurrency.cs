#if DEBUG
using System;
using System.Collections.Generic;
using Eco.Gameplay.Economy;
using Rocket.API.DependencyInjection;
using Rocket.API.Economy;
using Rocket.API.Player;

namespace Rocket.Eco.Economy
{
    public sealed class EcoCurrency : IEconomyCurrency
    {
        internal readonly List<EcoEconomyAccount> _Accounts = new List<EcoEconomyAccount>();

        private readonly IDependencyContainer container;
        private readonly Currency internalCurrency;
        private readonly IPlayerManager playerManager;

        public EcoCurrency(Currency internalCurrency, IDependencyContainer container)
        {
            this.container = container;
            this.internalCurrency = internalCurrency;

            playerManager = container.Resolve<IPlayerManager>("ecoplayermanager");

            Owner = playerManager.GetPlayer(internalCurrency.Owner);
        }

        public bool IsBacked => internalCurrency.BackingItem != null;

        public IPlayer Owner { get; }

        public IEnumerable<EcoEconomyAccount> Accounts { get; }

        public decimal ExchangeTo(decimal amount, IEconomyCurrency targetCurrency) => throw new NotImplementedException();

        public bool CanExchange(IEconomyCurrency currency) => false;

        public string Name => internalCurrency.CurrencyName;

        public decimal Exchange(double amount, IEconomyCurrency targetCurrency) => throw new NotSupportedException("Vanilla currencies may not exchange with each-other.");
    }
}
#endif