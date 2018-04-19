using Mono.Cecil;

namespace Rocket.Eco.API.Patching
{
    public interface IAssemblyPatch
    {
        string TargetAssembly { get; }
        string TargetType { get; }

        void Patch(TypeDefinition definition);
    }
}