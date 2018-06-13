using System;
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
                string finalName = x.Name.Replace(".compressed", "").Replace("costura.", "");

                using (Stream stream = x.GetResourceStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
                    {
                        
                    }
                }
            });

            return definitions;
        }
    }
}
