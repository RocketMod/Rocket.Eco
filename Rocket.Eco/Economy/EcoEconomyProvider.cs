using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API.DependencyInjection;
using Rocket.API.Economy;
using Rocket.API.User;
using Rocket.Eco.API;
using Rocket.Eco.Player;

namespace Rocket.Eco.Economy
{
    public sealed class EcoEconomyProvider : IEconomyProvider
    {
        private readonly IDependencyContainer container;
        internal readonly List<EcoCurrency> _Currencies = new List<EcoCurrency>();

        public EcoEconomyProvider(IDependencyContainer container)
        {
            this.container = container;
        }

        public IEnumerable<IEconomyCurrency> Currencies => _Currencies;
        public IEconomyCurrency DefaultCurrency => _Currencies.FirstOrDefault(x => x.IsBacked);

        public void AddBalance(IIdentity owner, decimal amount, string reason = null)
        {
            throw new System.NotImplementedException();
        }

        public bool Transfer(IEconomyAccount source, IEconomyAccount target, decimal amount, string reason = null)
        {
            throw new System.NotImplementedException();
        }

        public void AddBalance(IEconomyAccount account, decimal amount, string reason = null)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveBalance(IIdentity owner, decimal amount, string reason = null)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveBalance(IEconomyAccount account, decimal amount, string reason = null)
        {
            throw new System.NotImplementedException();
        }

        public void SetBalance(IIdentity owner, decimal amount)
        {
            throw new System.NotImplementedException();
        }

        public void SetBalance(IEconomyAccount account, decimal amount)
        {
            throw new System.NotImplementedException();
        }

        public bool SupportsNegativeBalance(IIdentity owner) => false;

        public bool SupportsNegativeBalance(IEconomyAccount account) => false;

        public bool CreateAccount(IIdentity owner, string name, out IEconomyAccount account) => throw new NotSupportedException("You may not create vanilla accounts.");

        public bool CreateAccount(IIdentity owner, string name, IEconomyCurrency currency, out IEconomyAccount account) => throw new NotSupportedException("You may not create vanilla accounts.");

        public bool DeleteAccount(IEconomyAccount account) => throw new NotSupportedException("You may not delete vanilla accounts.");

        public IEconomyAccount GetAccount(IIdentity owner, string accountName = null)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IEconomyAccount> GetAccounts(IIdentity owner)
        {
            throw new System.NotImplementedException();
        }

        public bool SupportsIdentity(IIdentity identity) => identity is EcoPlayer || identity is EcoUser;
    }
}
