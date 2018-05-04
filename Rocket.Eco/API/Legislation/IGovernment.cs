using System.Collections.Generic;
using Rocket.API.Player;
using Rocket.Eco.Player;

namespace Rocket.Eco.API.Legislation
{
    public interface IGovernment
    {
        IReadOnlyCollection<ILaw> EnactedLaws { get; }
        IReadOnlyCollection<ILaw> ProposedLaws { get; }

        IReadOnlyCollection<IElection> PastElections { get; }

        IElection CurrentElection { get; }

        EcoPlayer CurrentLeader { get; }

        bool AddLaw(ILaw law);
        bool RemoveLaw(ILaw law);
        bool ProposeLaw(ILaw law);

        bool AddElection(IElection election);
        bool RemoveElection(IElection election);

        bool SetNewCurrentElection(IElection election);
        bool FinishCurrentElection();

        bool ForceNewLeader(IPlayer player);
    }
}