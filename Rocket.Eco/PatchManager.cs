using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Mono.Cecil;

using Rocket.Eco.API;
using Rocket.API.Logging;
using Rocket.API.DependencyInjection;
using Rocket.API;

namespace Rocket.Eco
{
    public sealed class PatchManager : IPatchManager
    {
        IDependencyContainer patchContainer;

        public void Init(IRuntime runtime)
        {
            if (Assembly.GetCallingAssembly().GetName().Name == "Rocket.Eco")
            {
                patchContainer = runtime.Container.CreateChildContainer();
            }
            else
            {
                throw new MethodAccessException("This method may only be called from the Rocket.Eco assembly.");
            }
        }

        public void RegisterPatch<T>(IRuntime runtime) where T : IAssemblyPatch, new()
        {
            var logger = patchContainer.Get<ILogger>();

            T patch = new T();
            patchContainer.RegisterInstance<IAssemblyPatch>(patch, $"{typeof(T).Assembly.FullName}_{patch.TargetAssembly}_{patch.TargetType}");

            logger.LogInformation($"A patch for {patch.TargetAssembly} has been registered.");
        }

        public void RunPatching(IRuntime runtime)
        {
            if (Assembly.GetCallingAssembly().GetName().Name == "Rocket.Eco")
            {
                var dict = CollectAssemblies();

                string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco");
                Directory.CreateDirectory(outputDir);

                foreach (KeyValuePair<string, byte[]> value in dict)
                {
                    File.WriteAllBytes(Path.Combine(outputDir, value.Key), value.Value);
                }

                var monoAssemblyResolver = new DefaultAssemblyResolver();
                monoAssemblyResolver.AddSearchDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco"));

                PatchAll(dict, patchContainer, monoAssemblyResolver);

                for (int i = 0; i < dict.Values.Count; i++)
                {
                    Assembly.Load(dict.Values.ElementAt(i));
                }
            }
            else
            {
                throw new MethodAccessException("This method may only be called from the Rocket.Eco assembly.");
            }
        }

        Dictionary<string, byte[]> CollectAssemblies()
        {
            Assembly eco = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals("EcoServer", StringComparison.InvariantCultureIgnoreCase));

            patchContainer.TryGetAll<IAssemblyPatch>(out var patches);

            if (patches == null)
            {
                patches = new List<IAssemblyPatch>();
            }

            var resources = eco.GetManifestResourceNames().Where(x => x.EndsWith(".compressed", StringComparison.InvariantCultureIgnoreCase)).Where(x => x.StartsWith("costura.", StringComparison.InvariantCultureIgnoreCase));

            Dictionary<string, byte[]> assemblies = new Dictionary<string, byte[]>();

            foreach (string resource in resources)
            {
                string finalName = resource.Replace(".compressed", "").Replace("costura.", "");

                try
                {
                    using (Stream stream = eco.GetManifestResourceStream(resource))
                    {
                        using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
                        {
                            var targetedPatches = patches.Where(x => x.TargetAssembly.Equals(finalName.Replace(".dll", ""), StringComparison.InvariantCultureIgnoreCase));

                            if (targetedPatches != null && targetedPatches.Count() != 0)
                            {
                                using (MemoryStream memStream = new MemoryStream())
                                {
                                    byte[] array = new byte[81920];
                                    int count;

                                    while ((count = deflateStream.Read(array, 0, array.Length)) != 0)
                                    {
                                        memStream.Write(array, 0, count);
                                    }

                                    memStream.Position = 0;
                                    WriteAssembly(finalName, memStream, assemblies);
                                }
                            }
                            else
                            {
                                WriteAssembly(finalName, deflateStream, assemblies);
                            }
                        }
                    }
                }
                catch { }
            }

            return assemblies;
        }

        void PatchAll(Dictionary<string, byte[]> targets, IDependencyResolver resolver, DefaultAssemblyResolver monoCecilResolver)
        {
            var patches = resolver.GetAll<IAssemblyPatch>();
            foreach (KeyValuePair<string, byte[]> target in targets.ToList())
            {
                string finalName = target.Key;

                var targetedPatches = patches.Where(x => x.TargetAssembly.Equals(finalName.Replace(".dll", ""), StringComparison.InvariantCultureIgnoreCase));

                if (targetedPatches != null && targetedPatches.Count() != 0)
                {
                    using (MemoryStream memStream = new MemoryStream(target.Value))
                    {
                        AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(memStream, new ReaderParameters { AssemblyResolver = monoCecilResolver });

                        foreach (IAssemblyPatch patch in targetedPatches)
                        {
                            foreach (ModuleDefinition modDef in asmDef.Modules)
                            {
                                TypeDefinition typeDef = modDef.Types.FirstOrDefault(x => x.FullName.Equals(patch.TargetType, StringComparison.InvariantCultureIgnoreCase));

                                if (typeDef == null)
                                {
                                    continue;
                                }

                                patch.Patch(typeDef);

                                break;
                            }
                        }

                        asmDef.Write(memStream);
                        memStream.Position = 0;
                        WriteAssembly(finalName, memStream, targets);
                    }
                }
            }
        }

        void WriteAssembly(string finalName, Stream stream, Dictionary<string, byte[]> dict)
        {
            byte[] finalAssembly;

            if (stream is MemoryStream)
            {
                stream.Position = 0;

                finalAssembly = new byte[stream.Length];
                stream.Read(finalAssembly, 0, finalAssembly.Length);
            }
            else
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    byte[] array = new byte[81920];
                    int count;

                    while ((count = stream.Read(array, 0, array.Length)) != 0)
                    {
                        memStream.Write(array, 0, count);
                    }

                    memStream.Position = 0;

                    finalAssembly = new byte[memStream.Length];
                    memStream.Read(finalAssembly, 0, finalAssembly.Length);
                }
            }

            try
            {
                dict[finalName] = finalAssembly;
            }
            catch { }
        }
    }

    public interface IPatchManager
    {
        void Init(IRuntime runtime);
        void RegisterPatch<T>(IRuntime runtime) where T : IAssemblyPatch, new();
        void RunPatching(IRuntime runtime);
    }
}