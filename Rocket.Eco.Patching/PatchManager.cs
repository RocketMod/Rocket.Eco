using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Rocket.Eco.Patching.API;

namespace Rocket.Eco.Patching
{
    /// <inheritdoc cref="IPatchManager" />
    public sealed class PatcherPass : IPatcherPass
    {
        private readonly List<(AssemblyDefinition, IAssemblyResolver)> assemblies = new List<(AssemblyDefinition, IAssemblyResolver)>();
        private readonly List<IAssemblyPatch> patches = new List<IAssemblyPatch>();

        /// <inheritdoc />
        public bool RegisterPatch<T>() where T : IAssemblyPatch, new()
        {
            if (patches.FirstOrDefault(x => x.GetType() == typeof(T)) != null)
                return false;

            patches.Add(new T());
            return true;
        }

        /// <inheritdoc />
        public bool RegisterAssembly(AssemblyDefinition assemblyDefinition, IAssemblyResolver resolver) => throw new NotImplementedException();

        /// <inheritdoc />
        public IEnumerable<AssemblyDefinition> FinalizePass() => throw new NotImplementedException();

        /// <inheritdoc />
        public void RegisterDependencyResolver(IAssemblyResolver resolver)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RunPatching()
        {
            if (Assembly.GetCallingAssembly().GetName().Name == "Rocket.Eco")
            {
                Dictionary<string, byte[]> dict = CollectAssemblies();

                string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco");
                Directory.CreateDirectory(outputDir);

                foreach (KeyValuePair<string, byte[]> value in dict)
                    File.WriteAllBytes(Path.Combine(outputDir, value.Key), value.Value);

                DefaultAssemblyResolver monoAssemblyResolver = new DefaultAssemblyResolver();
                monoAssemblyResolver.AddSearchDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco"));

                //PatchAll(dict, patchContainer, monoAssemblyResolver);

                for (int i = 0; i < dict.Values.Count; i++)
                    Assembly.Load(dict.Values.ElementAt(i));
            }
            else
            {
                throw new MethodAccessException("This method may only be called from the Rocket.Eco assembly.");
            }
        }

        private Dictionary<string, byte[]> CollectAssemblies()
        {
            Assembly eco = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals("EcoServer", StringComparison.InvariantCultureIgnoreCase));

            if (eco == null) throw new Exception("The Eco assembly could not be found!");

            IEnumerable<string> resources = eco.GetManifestResourceNames().Where(x => x.EndsWith(".compressed", StringComparison.InvariantCultureIgnoreCase)).Where(x => x.StartsWith("costura.", StringComparison.InvariantCultureIgnoreCase));
            Dictionary<string, byte[]> assemblies = new Dictionary<string, byte[]>();

            foreach (string resource in resources)
            {
                string finalName = resource.Replace(".compressed", "").Replace("costura.", "");

                try
                {
                    using (Stream stream = eco.GetManifestResourceStream(resource))
                    {
                        if (stream == null) continue;

                        using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
                        {
                            WriteAssembly(finalName, deflateStream, assemblies);
                        }
                    }
                }
                catch (Exception e)
                {
                    //ILogger logger = container.Resolve<ILogger>();

                    //logger.LogError("Unable to deflate and write an Assembly to the disk!", e);
                    //logger.LogError(e.Message);
                    //logger.LogError(e.StackTrace);
                }
            }

            return assemblies;
        }

        private static void PatchAll(IDictionary<string, byte[]> targets, IAssemblyResolver monoCecilResolver)
        {
            IEnumerable<IAssemblyPatch> patches = null; //resolver.ResolveAll<IAssemblyPatch>();
            foreach (KeyValuePair<string, byte[]> target in targets.ToList())
            {
                string finalName = target.Key;

                IEnumerable<IAssemblyPatch> targetedPatches = patches.Where(x => x.TargetAssembly.Equals(finalName.Replace(".dll", ""), StringComparison.InvariantCultureIgnoreCase));

                if (!targetedPatches.Any())
                    continue;

                using (MemoryStream memStream = new MemoryStream(target.Value))
                {
                    AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(memStream, new ReaderParameters
                    {
                        AssemblyResolver = monoCecilResolver
                    });

                    foreach (IAssemblyPatch patch in targetedPatches)
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

                    asmDef.Write(memStream);

                    asmDef.Dispose();

                    memStream.Position = 0;
                    WriteAssembly(finalName, memStream, targets);
                }
            }
        }

        private static void WriteAssembly(string finalName, Stream stream, IDictionary<string, byte[]> dict)
        {
            byte[] finalAssembly;

            using (MemoryStream memStream = new MemoryStream())
            {
                byte[] array = new byte[81920];
                int count;

                while ((count = stream.Read(array, 0, array.Length)) != 0)
                    memStream.Write(array, 0, count);

                memStream.Position = 0;

                finalAssembly = new byte[memStream.Length];
                memStream.Read(finalAssembly, 0, finalAssembly.Length);
            }

            dict[finalName] = finalAssembly;
        }
    }
}