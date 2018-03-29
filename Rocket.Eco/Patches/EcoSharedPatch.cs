using System;

using Rocket.Eco.API;

namespace Rocket.Eco.Patches
{
    public class EcoSharedPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.Shared";

        public void Patch(ref byte[] assembly)
        {
            
        }
    }
}
