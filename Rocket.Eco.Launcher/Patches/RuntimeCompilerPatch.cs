using Mono.Cecil;
using Rocket.Eco.Patching.API;

namespace Rocket.Eco.Launcher.Patches
{
    public sealed class RuntimeCompilerPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.ModKit";
        public string TargetType => "Eco.ModKit.RuntimeCompiler";

        public void Patch(TypeDefinition definition)
        {
           
        }
    }
}
