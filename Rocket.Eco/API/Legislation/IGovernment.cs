using System.Collections.Generic;
using Rocket.Eco.Player;

namespace Rocket.Eco.API.Legislation
{
    public interface IGovernment
    {
        IEnumerable<ILaw> Laws { get; }
        IEnumerable<IElection> PastElections { get; }

        IElection CurrentElection { get; }

        EcoPlayer CurrentLeader { get; }
    }
}
