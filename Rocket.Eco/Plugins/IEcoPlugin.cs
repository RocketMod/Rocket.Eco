using System;

using Rocket.API.Plugin;

namespace Rocket.Eco.Plugins
{
    public interface IEcoPlugin : IPlugin
    {
        void PreLoad();
    }
}
