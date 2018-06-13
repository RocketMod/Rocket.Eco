using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Rocket.Eco.Patching.API;

namespace Rocket.Eco.Patching
{
    /// <inheritdoc />
    public sealed class PatchingService : IPatchingService
    {
        private readonly List<AssemblyDefinition> assemblies = new List<AssemblyDefinition>();
        private readonly List<IAssemblyPatch> patches = new List<IAssemblyPatch>();

        private readonly object lockObj = new object();

        /// <inheritdoc />
        public bool RegisterPatch<T>() where T : IAssemblyPatch, new()
        {
            lock (lockObj)
            {
                if (patches.FirstOrDefault(x => x.GetType() == typeof(T)) != null)
                    return false;

                patches.Add(new T());
                return true;
            }
        }

        /// <inheritdoc />
        public bool RegisterAssembly(AssemblyDefinition assemblyDefinition)
        {
            lock (lockObj)
            {
                if (assemblies.FirstOrDefault(x => x.Name.Name.Equals(assemblyDefinition.Name.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
                    return false;

                assemblies.Add(assemblyDefinition);
                return true;
            }
        }

        /// <inheritdoc />
        public IEnumerable<AssemblyDefinition> Patch()
        {
            lock (lockObj)
            {
                foreach (AssemblyDefinition asmDef in assemblies)
                {
                    foreach (IAssemblyPatch patch in patches)
                    {
                        foreach (ModuleDefinition modDef in asmDef.Modules)
                        {
                            TypeDefinition typeDef = modDef.Types.FirstOrDefault(x => x.FullName.Equals(patch.TargetType, StringComparison.InvariantCultureIgnoreCase));

                            if (typeDef == null)
                                continue;

                            patch.Patch(typeDef);

                            break;
                        }
                    }
                }

                AssemblyDefinition[] asmArray = assemblies.ToArray();

                assemblies.Clear();
                patches.Clear();

                return asmArray;
            }
        }
    }
}