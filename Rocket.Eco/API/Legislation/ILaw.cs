#if DEBUG
namespace Rocket.Eco.API.Legislation
{
    public interface ILaw
    {
        string Name { get; }
        string Reason { get; }
        bool IsEnacted { get; }
    }
}
#endif