using System.Collections.Generic;
using Rocket.API.Player;

namespace Rocket.Eco.API.Legislation
{
    public interface IElection
    {
        bool IsFinished { get; }
        IPlayer CurrentTopVote { get; }
        IReadOnlyCollection<IPlayer> EnteredPlayers { get; }
        IReadOnlyCollection<IElectionVote> Votes { get; }
    }
}