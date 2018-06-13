using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Mono.Cecil;

namespace Rocket.Eco.Launcher
{
    public static class CosturaHelper
    {
        public static IEnumerable<AssemblyDefinition> ExtractCosturaAssemblies(AssemblyDefinition definition)
        {
            List<AssemblyDefinition> definitions = new List<AssemblyDefinition>();

            definition.MainModule.Resources.Where(x => x.ResourceType == ResourceType.Embedded).Cast<EmbeddedResource>().Where(x => x.Name.EndsWith(".compressed") && x.Name.StartsWith("costura")).ForEach(x =>
            {
                using (Stream stream = x.GetResourceStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
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

                            definitions.Add(AssemblyDefinition.ReadAssembly(memStream));
                        }
                    }
                }
            });

            return definitions;
        }
    }
}
