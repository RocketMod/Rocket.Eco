using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rocket.Eco.API
{
    public interface IAssemblyPatch
    {
        string TargetAssembly { get; }

        void Patch(ref byte[] assembly);
    }
}
