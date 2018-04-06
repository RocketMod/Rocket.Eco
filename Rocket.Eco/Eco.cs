using System;
using System.Collections.Generic;

using Rocket.API;
using Rocket.API.Eventing;
using Rocket.API.Logging;

using Rocket.Eco.Eventing;

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
            var eventManager = runtime.Container.Get<IEventManager>();
            
            //patchManager.RegisterPatch<UserPatch>(runtime);
            patchManager.RunPatching(runtime);

            eventManager.AddEventListener(this, new EcoEventListener());

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