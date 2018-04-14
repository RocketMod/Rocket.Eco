using System;

namespace Rocket.Eco.API.Patching
{
    public interface IPatchManager
    {
        void RegisterPatch<T>() where T : IAssemblyPatch, new();
        void RegisterPatch(Type type);
        void RunPatching();
    }
}
