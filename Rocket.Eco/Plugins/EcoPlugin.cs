using System;

using Rocket.API.DependencyInjection;

using Rocket.Core.Plugins;

namespace Rocket.Eco.Plugins
{
    public abstract class EcoPlugin : Plugin, IEcoPlugin
    {
        protected EcoPlugin(IDependencyContainer container) : base(container) { }

        protected EcoPlugin(string name, IDependencyContainer container) : base(name, container) { }

        public virtual void PreLoad() { }
    }
}
