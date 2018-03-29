using System;
using System.Linq;

using Rocket.API.IOC;
using Rocket.API.Logging;

namespace Rocket.Eco
{
    public sealed class Eco : IEco
    {
        public Eco(IDependencyContainer container, IDependencyResolver resolver, ILogger logger, IPatchManager patchManager)
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
