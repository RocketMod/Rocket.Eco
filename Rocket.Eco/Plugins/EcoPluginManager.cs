using System;

using Rocket.API.Plugin;

namespace Rocket.Eco.Plugins
{
    public sealed class EcoPluginManager : IPluginManager
    {
        public void Init()
        {
            throw new NotImplementedException();
        }

        public bool ExecutePluginDependendCode(string pluginName, Action<IPlugin> action)
        {
            throw new NotImplementedException();
        }

        public IPlugin GetPlugin(string name)
        {
            throw new NotImplementedException();
        }

        public bool LoadPlugin(string name)
        {
            throw new NotImplementedException();
        }

        public bool PluginExists(string name)
        {
            throw new NotImplementedException();
        }

        public bool UnloadPlugin(string name)
        {
            throw new NotImplementedException();
        }
    }
}
