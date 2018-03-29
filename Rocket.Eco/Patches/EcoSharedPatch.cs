using System;

using Mono.Cecil;

using Rocket.Eco.API;

namespace Rocket.Eco.Patches
{
    public class EcoSharedPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.Shared";
        public string TargetType => "idfk";

        public void Patch(TypeDefinition definition)
        {
            
        }
    }
}
