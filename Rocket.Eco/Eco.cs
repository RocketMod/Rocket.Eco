using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Rocket.API;
using Rocket.API.Logging;
using Rocket.Eco.Patches;

namespace Rocket.Eco
{
    public sealed class Eco : IImplementation
    {
        public string InstanceId => throw new NotImplementedException();
        public IEnumerable<string> Capabilities => new List<string> { "NADA" };

        internal static string[] Arguments = default(string[]);

        private static Dictionary<string, byte[]> assemblies = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<string, bool> nullCache = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
        private static object nullCacheLock = new object();

        public void Load(IRuntime runtime)
        {
            var logger = runtime.Container.Get<ILogger>();
            var patchManager = runtime.Container.Get<IPatchManager>();

            patchManager.RegisterPatch<Eco_Simulation_AnimalSim>(runtime.Container, logger);

            patchManager.PatchAll(assemblies, runtime.Container, logger);

            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
            {
                string realName = (new AssemblyName(args.Name)).Name;

                lock (nullCacheLock)
                {
                    if (nullCache.ContainsKey(realName))
                    {
                        return null;
                    }
                }

                if (assemblies.ContainsKey(realName))
                {
                    return Assembly.Load(assemblies[realName]);
                }

                lock (nullCacheLock)
                {
                    nullCache[realName] = true;
                    return null;
                }
            };

            if (Arguments.Contains("-extract", StringComparer.InvariantCultureIgnoreCase))
            {
                RunExtraction(assemblies, logger);
            }
            else
            {
                logger.Info("Rocket.Eco has initialized.");
            }
        }

        public void Shutdown()
        {

        }

        public void Reload()
        {

        }

        void RunExtraction(Dictionary<string, byte[]> asms, ILogger logger)
        {
            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "PatchedAssemblies");
            Directory.CreateDirectory(outputDir);

            foreach (KeyValuePair<string, byte[]> value in asms)
            {
                File.WriteAllBytes(Path.Combine(outputDir, value.Key + ".dll"), value.Value);
                logger.Info($"\"{value.Key}\" has been patched and extracted to your file system.");
            }
        }
    }
}
