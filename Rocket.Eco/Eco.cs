using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Rocket.API;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.Eco.Patches;

namespace Rocket.Eco
{
    public sealed class Eco : IImplementation
    {
        public string InstanceId => throw new NotImplementedException();
        public IEnumerable<string> Capabilities => new List<string> { "idk" };

        internal static string[] Arguments = default(string[]);

        public void Load(IRuntime runtime)
        {
            var patchManager = runtime.Container.Get<IPatchManager>();
            var logger = runtime.Container.Get<ILogger>();

            patchManager.RegisterPatch<Eco_Simulation_AnimalSim>(runtime.Container, logger);

            var result = patchManager.PatchAll(runtime.Container, logger);

            if (Arguments.Contains("-extract", StringComparer.InvariantCultureIgnoreCase))
            {
                RunExtraction(result, logger);
            }
            else
            {
                logger.Info("Rocket.Eco.E has initialized.");
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
                File.WriteAllBytes(Path.Combine(outputDir, value.Key), value.Value);
                logger.Info($"\"{value.Key}\" has been patched and extracted to your file system.");
            }
        }
    }
}