using System;
using System.Collections.Generic;
using Rocket.API.DependencyInjection;
using Rocket.Eco.API;
using Rocket.Eco.API.Legislation;
using Rocket.Eco.Player;
using EcoLegislation = Eco.Gameplay.Legislation;

namespace Rocket.Eco.Legislation
{
    public sealed class EcoGovernment : ContainerAccessor, IGovernment
    {
        private readonly EcoLegislation ecoLegislation;

        public EcoGovernment(EcoLegislation ecoLegislation, IDependencyContainer container) : base(container)
        {
            this.ecoLegislation = ecoLegislation;

            //TODO: Pull all info from EcoLegislation.
        }

        public IElection CurrentElection { get; private set; }
        public EcoPlayer CurrentLeader { get; private set; }

        public IEnumerable<ILaw> Laws => throw new NotImplementedException();

        public IEnumerable<IElection> PastElections => throw new NotImplementedException();
    }
}