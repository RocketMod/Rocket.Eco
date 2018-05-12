using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Eco.Shared.Utils;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;
using Rocket.Eco.API;
using Rocket.Eco.API.Legislation;
using Rocket.Eco.Player;
using EcoLegislation = Eco.Gameplay.Legislation;

namespace Rocket.Eco.Legislation
{
    public sealed class EcoGovernment : IGovernment
    {
        private readonly IDependencyContainer container;
        private readonly EcoLegislation ecoLegislation;

        private readonly List<IElection> elections = new List<IElection>();

        private readonly object electionsLock = new object();
        private readonly List<ILaw> laws = new List<ILaw>();
        private readonly object lawsLock = new object();

        public EcoGovernment(IDependencyContainer container)
        {
            this.container = container;
            ecoLegislation = Singleton<EcoLegislation>.Obj;

            //TODO: Pull all info from EcoLegislation.
        }

        public IElection CurrentElection { get; private set; }
        public EcoPlayer CurrentLeader { get; private set; }

        public bool AddLaw(ILaw law)
        {
            lock (lawsLock)
            {
                if (laws.Contains(law)) return false;

                laws.Add(law);
            }

            return true;
        }

        public bool RemoveLaw(ILaw law)
        {
            lock (lawsLock)
            {
                if (laws.Contains(law)) return false;

                laws.RemoveAll(x => x.Equals(law));
            }

            return true;
        }

        public bool ProposeLaw(ILaw law) => throw new NotImplementedException();

        public bool AddElection(IElection election)
        {
            lock (electionsLock)
            {
                if (elections.Contains(election)) return false;

                elections.Add(election);
            }

            return true;
        }

        public bool RemoveElection(IElection election)
        {
            lock (electionsLock)
            {
                if (elections.Contains(election)) return false;

                elections.Remove(election);
            }

            return true;
        }

        public bool SetNewCurrentElection(IElection election)
        {
            if (CurrentElection.Equals(election)) return false;

            CurrentElection = election;

            return true;
        }

        public bool FinishCurrentElection() => throw new NotImplementedException();

        public bool ForceNewLeader(IPlayer player) => throw new NotImplementedException();

        public IReadOnlyCollection<ILaw> EnactedLaws
        {
            get
            {
                List<ILaw> lawsList;

                lock (lawsLock)
                {
                    lawsList = laws.ToList();
                }

                lawsList.RemoveAll(x => !x.IsEnacted);

                return lawsList.AsReadOnly();
            }
        }

        public IReadOnlyCollection<ILaw> ProposedLaws
        {
            get
            {
                List<ILaw> lawsList;

                lock (lawsLock)
                {
                    lawsList = laws.ToList();
                }

                lawsList.RemoveAll(x => x.IsEnacted);

                return lawsList.AsReadOnly();
            }
        }

        public IReadOnlyCollection<IElection> PastElections
        {
            get
            {
                List<IElection> electionsList;

                lock (electionsLock)
                {
                    electionsList = elections.ToList();
                }

                electionsList.RemoveAll(x => x != CurrentElection);

                return electionsList.AsReadOnly();
            }
        }
    }
}