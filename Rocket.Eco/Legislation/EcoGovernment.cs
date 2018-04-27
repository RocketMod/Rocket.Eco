using System;
using System.Collections.Generic;
using Eco.Shared.Utils;
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

        public EcoGovernment( IDependencyContainer container) : base(container)
        {
            ecoLegislation = Singleton<EcoLegislation>.Obj;

            //TODO: Pull all info from EcoLegislation.
        }

        public IElection CurrentElection { get; private set; }
        public EcoPlayer CurrentLeader { get; private set; }

        public IEnumerable<ILaw> Laws => throw new NotImplementedException();

        public IEnumerable<IElection> PastElections => throw new NotImplementedException();
    }
}