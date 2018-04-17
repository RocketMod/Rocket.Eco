using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Rocket.API;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Plugin;
using Rocket.Core.Plugins.Events;
using Rocket.Eco.API.Patching;

namespace Rocket.Eco.Eventing
{
    public sealed class EcoEventListener : IEventListener<PluginManagerInitEvent>
    {
        private readonly IRuntime runtime;

        internal EcoEventListener(IRuntime runtime)
        {
            this.runtime = runtime;
        }

        public void HandleEvent(IEventEmitter emitter, PluginManagerInitEvent @event)
        {
            IEnumerable<IPlugin> plugins = @event.PluginManager.Plugins;
            IPatchManager patchManager = runtime.Container.Get<IPatchManager>();

            foreach (IPlugin plugin in plugins)
            {
                Type[] types;

                try
                {
                    types = plugin.GetType().Assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                IEnumerable<Type> patches = types.Where(x => x.GetInterfaces().Contains(typeof(IAssemblyPatch)));

                foreach (Type type in patches) patchManager.RegisterPatch(type);
            }

            patchManager.RunPatching();

            string[] args = Environment.GetCommandLineArgs();

            if (!args.Contains("-extract", StringComparer.InvariantCultureIgnoreCase))
            {
                try
                {
                    AppDomain.CurrentDomain.GetAssemblies()
                             .First(x => x.GetName().Name.Equals("EcoServer"))
                             .GetType("Eco.Server.Startup")
                             .GetMethod("Start", BindingFlags.Static | BindingFlags.Public)
                             .Invoke(null, new object[]
                                 {args.Where(x => x.Equals("-extract", StringComparison.InvariantCultureIgnoreCase)).ToArray()});
                }
                catch (NullReferenceException)
                {
                    runtime.Container.Get<ILogger>().LogFatal("The entrypoint for the EcoServer couldn't be found!");

                    Thread.Sleep(3000);
                    Environment.Exit(0);
                }
            }
            else
            {
                runtime.Container.Get<ILogger>().LogInformation("Extraction has finished; please restart the program without the `-extract` argument to run.");

                Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }
    }
}