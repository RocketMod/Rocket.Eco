using Mono.Cecil;

namespace Rocket.Eco.API
{
    public interface IAssemblyPatch
    {
        string TargetAssembly { get; }
        string TargetType { get; }

        void Patch(TypeDefinition definition);
    }
}