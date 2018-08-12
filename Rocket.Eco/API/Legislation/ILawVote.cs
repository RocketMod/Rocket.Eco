#if DEBUG
namespace Rocket.Eco.API.Legislation
{
    public interface ILawVote : IVote
    {
        ILaw Law { get; }
        bool PositiveVote { get; }
    }
}
#endif