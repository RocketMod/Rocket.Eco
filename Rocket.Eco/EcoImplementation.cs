using System;
using System.Collections.Generic;
using Eco.Core.Plugins;
using Eco.Gameplay.Players;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Plugin;
using Rocket.Core.Events.Player;
using Rocket.Eco.Eventing;
using Rocket.Eco.Events;
using Rocket.Eco.Patches;
using Rocket.Eco.Patching;
using Rocket.Eco.Player;

namespace Rocket.Eco
{
    public sealed class EcoImplementation : IImplementation
    {
        private IRuntime runtime;
        public string InstanceId => throw new NotImplementedException();
        public IEnumerable<string> Capabilities => new string[0];
        public bool IsAlive { get; } = true;
        public string WorkingDirectory => "./Rocket/";
        public string Name => "Rocket.Eco";

        public void Load(IRuntime runtime)
        {
            this.runtime = runtime;

            IPatchManager patchManager = runtime.Container.Get<IPatchManager>();
            ILogger logger = runtime.Container.Get<ILogger>();
            IEventManager eventManager = runtime.Container.Get<IEventManager>();
            IPluginManager pluginManager = runtime.Container.Get<IPluginManager>();

            patchManager.RegisterPatch<UserPatch>();
            eventManager.AddEventListener(this, new EcoEventListener(runtime));
            pluginManager.Init();

            logger.LogInformation("Rocket bootstrapping is finished.");
        }

        public void Shutdown()
        {
            StorageManager.SaveAndFlush();

            foreach (IPlugin plugin in runtime.Container.Get<IPluginManager>())
                plugin.Unload();

            Environment.Exit(0);
        }

        public void Reload()
        {
            IPluginManager pluginManager = runtime.Container.Get<IPluginManager>();

            foreach (IPlugin plugin in runtime.Container.Get<IPluginManager>())
                if (pluginManager.UnloadPlugin(plugin.Name))
                    pluginManager.LoadPlugin(plugin.Name);
        }

        internal void _EmitPlayerJoin(object player)
        {
            EcoPlayer ecoPlayer = new EcoPlayer((player as User).Player);
            PlayerConnectedEvent e = new PlayerConnectedEvent(ecoPlayer, "");

            runtime.Container.Get<IEventManager>().Emit(this, e);

            runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has joined!");
        }

        internal void _EmitPlayerLeave(object player)
        {
            EcoPlayer ecoPlayer = new EcoPlayer((player as User).Player);
            PlayerDisconnectedEvent e = new PlayerDisconnectedEvent(ecoPlayer, "");

            runtime.Container.Get<IEventManager>().Emit(this, e);

            runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has left!");
        }

        internal void _EmitEcoInit()
        {
            EcoReadyEvent e = new EcoReadyEvent(this);
            runtime.Container.Get<IEventManager>().Emit(this, e);

            runtime.Container.Get<ILogger>().LogInformation("[EVENT] Eco has initialized!");
        }

        internal bool _EmitPlayerChat(string text, object user)
        {
            PlayerChatEvent e = new PlayerChatEvent(new EcoPlayer((user as User).Player), string.Empty, text);
            runtime.Container.Get<IEventManager>().Emit(this, e);

            return !e.IsCancelled;
        }

        internal bool _ProcessCommand(string text, object user)
        {
            if (!text.StartsWith("/", StringComparison.InvariantCulture)) return false;

            EcoPlayer p = new EcoPlayer((user as User).Player);
            bool wasHandled = runtime.Container.Get<ICommandHandler>().HandleCommand(p, text.Remove(0, 1));

            if (!wasHandled) p.Player.SendTemporaryErrorLoc("That command could not be found!");

            return true;
        }

        //TODO
        internal void _AwaitInput()
        {
            while (true)
            {
                string input = Console.ReadLine();

                if (input.StartsWith("/", StringComparison.InvariantCulture)) input = input.Remove(0, 1);

                bool wasHandled = runtime.Container.Get<ICommandHandler>().HandleCommand(new EcoConsoleCommandCaller(), input);

                if (!wasHandled) runtime.Container.Get<ILogger>().LogError("That command could not be found!");
            }
        }
    }
}