#if DEBUG
using System.Collections.Generic;
using Rocket.API.Player;

namespace Rocket.Eco.API.Legislation
{
    public interface ILawProposal
    {
        ILaw Law { get; }
        IPlayer ProposingPlayer { get; }
        bool IsRemoval { get; }

        IReadOnlyCollection<ILawVote> Votes { get; }
    }
}
#endif