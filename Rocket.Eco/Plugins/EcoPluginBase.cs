using System;

using Rocket.API.DependencyInjection;

using Rocket.Core.Plugins;

namespace Rocket.Eco.Plugins
{
    public abstract class EcoPluginBase : PluginBase, IEcoPlugin
    {
        protected EcoPluginBase(IDependencyContainer container) : base(container) { }

        protected EcoPluginBase(string name, IDependencyContainer container) : base(name, container) { }

        public virtual void PreLoad() { }
    }
}
