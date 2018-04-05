using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

using Rocket.API;
using Rocket.API.Logging;
using Rocket.Eco.Patches;

namespace Rocket.Eco
{
    public sealed class Eco : IImplementation
    {
        public string InstanceId => throw new NotImplementedException();
        public IEnumerable<string> Capabilities => new List<string> { "NADA" };
        public bool IsAlive { get; } = true;

        public void Load(IRuntime runtime)
        {
            var patchManager = runtime.Container.Get<IPatchManager>();
            var logger = runtime.Container.Get<ILogger>();
            
            patchManager.RegisterPatch<UserPatch>(runtime.Container, logger);

            RunPatching(runtime, patchManager);

            logger.Info("Rocket.Eco.E has initialized.");
        }

        public void Shutdown()
        {

        }

        public void Reload()
        {

        }

        //TODO: Implement this into IPatchManager
        void RunPatching(IRuntime runtime, IPatchManager patchManager)
        {
            var dict = patchManager.CollectAssemblies(runtime.Container);

            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco");
            Directory.CreateDirectory(outputDir);

            foreach (KeyValuePair<string, byte[]> value in dict)
            {
                File.WriteAllBytes(Path.Combine(outputDir, value.Key), value.Value);
            }

            var monoAssemblyResolver = new DefaultAssemblyResolver();
            monoAssemblyResolver.AddSearchDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco"));

            patchManager.PatchAll(dict, runtime.Container, monoAssemblyResolver);

            for (int i = 0; i < dict.Values.Count; i++)
            {
                Assembly.Load(dict.Values.ElementAt(i));
            }
        }
    }
}