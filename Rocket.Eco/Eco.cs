using System;
using System.Linq;

using Rocket.Core;
using Rocket.IOC;

using Rocket.Eco.Patches;
using Rocket.Eco.API;

namespace Rocket.Eco
{
    public sealed class Eco : IEco
    {
        public Eco(IDependencyContainer container, IDependencyResolver resolver, ILog logger, IPatchManager patchManager)
        {
            //patchManager.RegisterPatch<EcoSharedPatch>(container, logger);
            logger.Info("Rocket.Eco.E has initialized.");

            patchManager.PatchAll(resolver);
        }
    }

    public interface IEco
    {
        //TODO: Add some methods
    }
}
