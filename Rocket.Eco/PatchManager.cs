using System;

using Mono.Cecil;

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
            container.RegisterInstance<IAssemblyPatch>(patch, $"{typeof(T).Assembly.FullName}_{patch.TargetAssembly}_{patch.TargetType}");

            logger.Info($"A patch for {patch.TargetAssembly} has been registered.");
        }

        internal void Patch(IDependencyResolver resolver)
        {
            var patches = resolver.GetAll<IAssemblyPatch>();

            foreach (IAssemblyPatch patch in patches)
            {

            }
        }
    }

    public interface IPatchManager
    {
        void RegisterPatch<T>(IDependencyContainer container, ILog logger) where T : IAssemblyPatch, new();
    }
}
