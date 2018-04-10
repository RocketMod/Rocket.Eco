﻿using System;
using System.IO;

using Rocket.API;
using Rocket.API.DependencyInjection;
using Rocket.API.Plugin;

namespace Rocket.Eco.Plugins
{
    public sealed class EcoPluginManager : IPluginManager
    {
        public static string PluginsDir => Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Plugins");
        public static string PackagesDir => Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Packages");

        public static string OldPluginsDir => Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        public static string OldPackagesDir => Path.Combine(Directory.GetCurrentDirectory(), "Packages");

        IDependencyContainer pluginsContainer;

        public EcoPluginManager(IRuntime runtime)
        {
            pluginsContainer = runtime.Container.CreateChildContainer();
        }

        public void Init()
        {
            Directory.Delete(OldPluginsDir);
            Directory.Delete(OldPackagesDir);

            Directory.CreateDirectory(PluginsDir);
            Directory.CreateDirectory(PackagesDir);
        }

        public void PostInit()
        {

        }

        public bool ExecutePluginDependendCode(string pluginName, Action<IPlugin> action)
        {
            throw new NotImplementedException();
        }

        public IPlugin GetPlugin(string name)
        {
            throw new NotImplementedException();
        }

        public bool PluginExists(string name)
        {
            throw new NotImplementedException();
        }

        public bool LoadPlugin(string name)
        {
            throw new NotImplementedException();
        }

        public bool UnloadPlugin(string name)
        {
            throw new NotImplementedException();
        }
    }
}
