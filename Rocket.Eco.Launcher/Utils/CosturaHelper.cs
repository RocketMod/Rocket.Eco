using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Mono.Cecil;

namespace Rocket.Eco.Launcher.Utils
{
    public static class CosturaHelper
    {
        internal static Dictionary<string, AssemblyData> Assemblies = new Dictionary<string, AssemblyData>(StringComparer.InvariantCultureIgnoreCase);

        public static IEnumerable<AssemblyDefinition> ExtractCosturaAssemblies(AssemblyDefinition definition)
        {
            List<AssemblyDefinition> definitions = new List<AssemblyDefinition>();

            definition.MainModule.Resources.Where(x => x.ResourceType == ResourceType.Embedded).Cast<EmbeddedResource>().Where(x => x.Name.EndsWith(".compressed") && x.Name.StartsWith("costura")).ForEach(x =>
            {
                using (Stream stream = x.GetResourceStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
                    {
                        MemoryStream memStream = new MemoryStream();

                        byte[] array = new byte[81920];

                        int count;
                        while ((count = deflateStream.Read(array, 0, array.Length)) != 0)
                        {
                            memStream.Write(array, 0, count);
                        }

                        Assemblies[x.Name.Replace(".dll.compressed", "").Replace("costura.", "")] = new AssemblyData(memStream, null);
                    }
                }
            });

            CosturaAssemblyResolver resolver = new CosturaAssemblyResolver();
            ReaderParameters parameters = new ReaderParameters()
            {
                AssemblyResolver = resolver
            };

            Assemblies.Keys.ForEach(x =>
            {
                AssemblyData asm = Assemblies[x];

                if (asm.Assembly != null)
                {
                    definitions.Add(asm.Assembly);
                }
                else
                {
                    asm.Stream.Position = 0;
                    asm.Assembly = AssemblyDefinition.ReadAssembly(asm.Stream, parameters);

                    definitions.Add(asm.Assembly);
                }
            });

            return definitions;
        }

        public static void DisposeStreams() => Assemblies.ForEach(x => x.Value.Stream.Dispose());

        //plz add data classes like in Kotlin
        public class AssemblyData
        {
            public AssemblyDefinition Assembly;
            public Stream Stream;

            public AssemblyData(Stream stream, AssemblyDefinition admdef)
            {
                Assembly = admdef;
                Stream = stream;
            }
        }
    }
}
