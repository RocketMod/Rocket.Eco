using System;

using Rocket.API;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;

using Rocket.Eco.Player;

namespace Rocket.Eco.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterType<IPatchManager, PatchManager>();
            container.RegisterType<IPlayerManager, EcoPlayerManager>();
            container.RegisterType<IImplementation, Eco>();
        }
    }
}
