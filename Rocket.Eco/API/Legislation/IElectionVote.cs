#if DEBUG
using System.Linq;
using Rocket.API.Player;

namespace Rocket.Eco.API.Legislation
{
    public interface IElectionVote : IVote
    {
        IElection Election { get; }
        IOrderedEnumerable<IPlayer> Votes { get; }
    }
}
#endif