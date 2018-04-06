using System;
using System.Collections.Generic;

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
            
            patchManager.RegisterPatch<UserPatch>(runtime);

            //PreLoad Plugins

            patchManager.RunPatching(runtime);

            //Load Plugins

            logger.Info("Rocket.Eco.E has initialized.");
        }

        public void Shutdown()
        {

        }

        public void Reload()
        {

        }
    }
}