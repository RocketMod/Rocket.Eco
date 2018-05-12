using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco.Gameplay.Economy;
using Rocket.API.DependencyInjection;
using Rocket.API.Economy;
using Rocket.API.Player;
using Rocket.Eco.API;

namespace Rocket.Eco.Economy
{
    public sealed class EcoCurrency : ContainerAccessor, IEconomyCurrency
    {
        private readonly Currency internalCurrency;
        private readonly IPlayerManager playerManager;
        internal readonly List<EcoEconomyAccount> _Accounts = new List<EcoEconomyAccount>();

        public EcoCurrency(Currency internalCurrency, IDependencyContainer container) : base(container)
        {
            this.internalCurrency = internalCurrency;

            playerManager = Container.Resolve<IPlayerManager>("ecoplayermanager");

            Owner = playerManager.GetPlayer(internalCurrency.Owner);
        }

        public decimal Exchange(double amount, IEconomyCurrency targetCurrency) => throw new NotSupportedException("Vanilla currencies may not exchange with each-other.");

        public bool CanExchange(IEconomyCurrency currency) => false;

        public string Name => internalCurrency.CurrencyName;

        public bool IsBacked => internalCurrency.BackingItem != null;

        public IPlayer Owner { get; }

        public IEnumerable<EcoEconomyAccount> Accounts { get; }
    }
}
