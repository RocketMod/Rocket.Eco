using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Rocket.Patching.API;

namespace Rocket.Patching
{
    /// <inheritdoc />
    public sealed class PatchingService : IPatchingService
    {
        private readonly List<AssemblyDefinition> assemblies = new List<AssemblyDefinition>();

        private readonly object lockObj = new object();
        private readonly List<IAssemblyPatch> patches = new List<IAssemblyPatch>();

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

                            Console.WriteLine($"The patch `{patch.GetType().FullName}` has been applied to `{typeDef.FullName}`");

                            break;
                        }
                    }
                }

                //I hope this kills the Stream...
                assemblies.ForEach(x => x.Dispose());

                AssemblyDefinition[] asmArray = assemblies.ToArray();

                assemblies.Clear();
                patches.Clear();

                return asmArray;
            }
        }
    }
}