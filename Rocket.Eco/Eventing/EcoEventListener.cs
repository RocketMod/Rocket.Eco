using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Plugins;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Core.Plugins.Events;
using Rocket.Eco.API.Patching;

namespace Rocket.Eco.Eventing
{
    /// <inheritdoc cref="IEventListener{TEvent}" />
    /// >
    /// <summary>
    ///     An internal class used by Rocket.Eco to handle a <see cref="PluginManagerInitEvent" /> emitted by Rocket's
    ///     <see cref="PluginManager" />.
    /// </summary>
    public sealed class EcoEventListener : IEventListener<PluginManagerInitEvent>
    {
        private readonly IDependencyContainer container;

        internal EcoEventListener(IDependencyContainer container)
        {
            this.container = container;
        }

        /// <inheritdoc />
        [Core.Eventing.EventHandler(IgnoreCancelled = true)]
        public void HandleEvent(IEventEmitter emitter, PluginManagerInitEvent @event)
        {
            IEnumerable<IPlugin> plugins = @event.PluginManager.Plugins;
            IPatchManager patchManager = container.Resolve<IPatchManager>();

            List<Assembly> registered = new List<Assembly>();

            foreach (IPlugin plugin in plugins)
                if (registered.Contains(plugin.GetType().Assembly))
                {
                    registered.Add(plugin.GetType().Assembly);

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
                    // It's legit in a try/catch block...
                    // ReSharper disable once PossibleNullReferenceException
                    AppDomain.CurrentDomain.GetAssemblies()
                             .First(x => x.GetName().Name.Equals("EcoServer"))
                             .GetType("Eco.Server.Startup")
                             .GetMethod("Start", BindingFlags.Static | BindingFlags.Public)
                             .Invoke(null, new object[]
                                 {args.Where(x => !x.Equals("-extract", StringComparison.InvariantCultureIgnoreCase)).ToArray()});
                }
                catch (NullReferenceException)
                {
                    container.Resolve<ILogger>().LogFatal("The entrypoint for the EcoServer couldn't be found!");

                    WaitAndExit();
                }
            }
            else
            {
                container.Resolve<ILogger>().LogInformation("Extraction has finished; please restart the program without the `-extract` argument to run.");

                WaitAndExit();
            }
        }

        private static void WaitAndExit()
        {
            Thread.Sleep(3000);
            Environment.Exit(0);
        }
    }
}