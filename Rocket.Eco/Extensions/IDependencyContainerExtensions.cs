using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.API.Scheduler;
using Rocket.Eco.API.Legislation;
using Rocket.Eco.API.Patching;

namespace Rocket.Eco.Extensions
{
    public static class IDependencyContainerExtensions
    {
        public static ILogger ResolveLogger(this IDependencyContainer container, string mapping = null) => container.Resolve<ILogger>(mapping);

        public static IEventManager ResolveEventManager(this IDependencyContainer container, string mapping = null) => container.Resolve<IEventManager>(mapping);

        public static ICommandHandler ResolveCommandHandler(this IDependencyContainer container, string mapping = null) => container.Resolve<ICommandHandler>(mapping);

        public static IPlayerManager ResolvePlayerManager(this IDependencyContainer container, string mapping = null) => container.Resolve<IPlayerManager>(mapping);

        public static IPatchManager ResolvePatchManager(this IDependencyContainer container, string mapping = null) => container.Resolve<IPatchManager>(mapping);

        public static ITaskScheduler ResolveTaskScheduler(this IDependencyContainer container, string mapping = null) => container.Resolve<ITaskScheduler>(mapping);

        public static IGovernment ResolveGovernment(this IDependencyContainer container, string mapping = null) => container.Resolve<IGovernment>(mapping);

        public static IImplementation ResolveImplementation(this IDependencyContainer container, string mapping = null) => container.Resolve<IImplementation>(mapping);

        public static IPluginManager ResolvePluginManager(this IDependencyContainer container, string mapping = null) => container.Resolve<IPluginManager>(mapping);
    }
}