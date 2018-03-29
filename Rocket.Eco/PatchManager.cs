using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Mono.Cecil;

using Rocket.Eco.API;
using Rocket.API.IOC;
using Rocket.API.Logging;
using Rocket.Compability;

namespace Rocket.Eco
{
    public sealed class PatchManager : IPatchManager
    {
        public void RegisterPatch<T>(IDependencyContainer container, ILogger logger) where T : IAssemblyPatch, new()
        {
            T patch = new T();
            container.RegisterInstance<IAssemblyPatch>(patch, $"{typeof(T).Assembly.FullName}_{patch.TargetAssembly}_{patch.TargetType}");

            logger.Info($"A patch for {patch.TargetAssembly} has been registered.");
        }

        public Tuple<string, Stream>[] PatchAll(IDependencyResolver resolver)
        {
            Assembly eco = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals("EcoServer", StringComparison.InvariantCultureIgnoreCase));

            resolver.TryGetAll<IAssemblyPatch>(out var patches);

            if (patches == null)
            {
                patches = new List<IAssemblyPatch>();
            }

            var resources = eco.GetManifestResourceNames().Where(x => x.EndsWith(".compressed", StringComparison.InvariantCultureIgnoreCase)).Where(x => x.StartsWith("costura.", StringComparison.InvariantCultureIgnoreCase));

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

                                    AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(memStream);

                                    if (asmDef == null)
                                    {
                                        Console.WriteLine("awfawf");
                                    }

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
                                    WriteAssembly(finalName, memStream);
                                }
                            }
                            else
                            {
                                WriteAssembly(finalName, deflateStream);
                            }
                        }
                    }

                    //File.WriteAllBytes(Path.Combine(OutputDir, finalName), finalFile);
                    //Console.WriteLine($"Successfully extracted {finalName}!");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occured while extracting {finalName}!");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            return null;
        }

        void WriteAssembly(string finalName, Stream stream)
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

            Assembly.Load(finalAssembly);

            string directory = Path.Combine(Directory.GetCurrentDirectory(), "PatchedAssemblies/");
            Directory.CreateDirectory(directory);

            File.WriteAllBytes(Path.Combine(directory, finalName), finalAssembly);
        }
    }

    public interface IPatchManager
    {
        void RegisterPatch<T>(IDependencyContainer container, ILogger logger) where T : IAssemblyPatch, new();
        Tuple<string, Stream>[] PatchAll(IDependencyResolver resolver);
    }
}
