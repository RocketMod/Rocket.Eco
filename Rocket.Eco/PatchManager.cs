using System;

using Rocket.Core;
using Rocket.IOC;

using Rocket.Eco.API;

namespace Rocket.Eco
{
    public sealed class PatchManager : IPatchManager
    {
        public void RegisterPatch<T>(IDependencyContainer container, ILog logger) where T : IAssemblyPatch, new()
        {
            T patch = new T();
            container.RegisterInstance<IAssemblyPatch>(patch, patch.TargetAssembly);

            logger.Info($"A patch for {patch.TargetAssembly} has been registered.");
        }
    }

    public interface IPatchManager
    {
        void RegisterPatch<T>(IDependencyContainer container, ILog logger) where T : IAssemblyPatch, new();
    }
}
