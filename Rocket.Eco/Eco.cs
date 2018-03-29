using System;
using System.Collections.Generic;
using System.Linq;

using Rocket.API;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;

namespace Rocket.Eco
{
    public sealed class Eco : IImplementation
    {
        public string InstanceId => "RocketEco";
        public IEnumerable<string> Capabilities => new List<string> { "idk" };

        public Eco(IDependencyContainer container, IDependencyResolver resolver, ILogger logger, IPatchManager patchManager)
        {
            //patchManager.RegisterPatch<EcoSharedPatch>(container, logger);
            logger.Info("Rocket.Eco.E has initialized.");

            patchManager.PatchAll(resolver);
        }

        public void Shutdown()
        {

        }

        public void Reload()
        {

        }
    }
}
