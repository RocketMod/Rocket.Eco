using System.Collections.Generic;
using System.Linq;
using Rocket.API.DependencyInjection;
using Rocket.Eco.API;
using Rocket.Eco.API.Legislation;
using Rocket.Eco.Player;
using EcoLegislation = Eco.Gameplay.Legislation;

namespace Rocket.Eco.Legislation
{
    public sealed class EcoGovernment : ContainerAccessor, IGovernment
    {
        private readonly IDependencyContainer governmentContainer;
        private readonly EcoLegislation ecoLegislation;

        public IElection CurrentElection { get; private set; }
        public EcoPlayer CurrentLeader { get; private set; }

        public EcoGovernment(EcoLegislation ecoLegislation, IDependencyContainer container) : base(container)
        {
            this.ecoLegislation = ecoLegislation;
            governmentContainer = container.CreateChildContainer();

            //TODO: Pull all info from EcoLegislation.
        }

        public IEnumerable<ILaw> Laws
        {
            get
            {
                governmentContainer.TryGetAll<ILaw>(out IEnumerable<ILaw> laws);
                return laws;
            }
        }

        public IEnumerable<IElection> PastElections
        {
            get
            {
                governmentContainer.TryGetAll<IElection>(out IEnumerable<IElection> elections);
                return elections.Where(x => x.IsFinished);
            }
        }
    }
}
