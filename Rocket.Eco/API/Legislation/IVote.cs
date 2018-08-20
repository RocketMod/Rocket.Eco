#if DEBUG
using System;
using Rocket.API.Player;

namespace Rocket.Eco.API.Legislation
{
    public interface IVote
    {
        IPlayer Voter { get; }

        DateTime? VoteCreated { get; }
        DateTime? VoteLastChanged { get; }
    }
}
#endif