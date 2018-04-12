using System;
using System.Collections.Generic;

using Eco.Gameplay.Players;

using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Eventing;
using Rocket.API.Logging;

using Rocket.Core.Events.Player;

using Rocket.Eco.Eventing;
using Rocket.Eco.Events;
using Rocket.Eco.Patches;
using Rocket.Eco.Patching;
using Rocket.Eco.Player;

namespace Rocket.Eco
{
    public sealed class Eco : IImplementation
    {
        public string InstanceId => throw new NotImplementedException();
        public IEnumerable<string> Capabilities => new string[0];
        public bool IsAlive { get; } = true;
        public string WorkingDirectory => "./Rocket/";
        public string Name => "Rocket.Eco";

        public static Eco Instance => runtime.Container.Get<IImplementation>() as Eco;

        internal static IRuntime runtime = null;

        internal static object[] launchArgs;
        internal static bool isExtraction;

        public void Load(IRuntime _runtime)
        {
            var patchManager = runtime.Container.Get<IPatchManager>();
            var logger = runtime.Container.Get<ILogger>();
            var eventManager = runtime.Container.Get<IEventManager>();

            runtime = _runtime;
            
            patchManager.Init(runtime);
            patchManager.RegisterPatch<UserPatch>(runtime);
            
            eventManager.AddEventListener(this, new EcoEventListener());

            if (!isExtraction)
            {
                logger.LogInformation("Rocket bootstrapping is finished.");
            }
        }

        public void Shutdown()
        {

        }

        public void Reload()
        {

        }

        internal void _EmitPlayerJoin(object player)
        {
            EcoPlayer ecoPlayer = new EcoPlayer((player as User).Player);
            PlayerConnectEvent e = new PlayerConnectEvent(ecoPlayer, "");

            runtime.Container.Get<IEventManager>().Emit(this, e);

            runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has joined!");
        }

        internal void _EmitPlayerLeave(object player)
        {
            EcoPlayer ecoPlayer = new EcoPlayer((player as User).Player);
            PlayerDisconnectEvent e = new PlayerDisconnectEvent(ecoPlayer, "");

            runtime.Container.Get<IEventManager>().Emit(this, e);

            runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has left!");
        }

        internal void _EmitEcoInit()
        {
            EcoReadyEvent e = new EcoReadyEvent();
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
            if (text.StartsWith("/"))
            {
                EcoPlayer p = new EcoPlayer((user as User).Player);
                bool wasHandled = runtime.Container.Get<ICommandHandler>().HandleCommand(p, text.Remove(0, 1));

                if (!wasHandled)
                {
                    p.Player.SendTemporaryErrorLoc("That command could not be found!");
                }

                return true;
            }

            return false;
        }

        internal void _AwaitInput()
        {
            while (true)
            {
                string input = Console.ReadLine();

                if (input.StartsWith("/"))
                {
                    input = input.Remove(0, 1);
                }

                bool wasHandled = runtime.Container.Get<ICommandHandler>().HandleCommand(new EcoConsoleCommandCaller(), input);

                if (!wasHandled)
                {
                    runtime.Container.Get<ILogger>().LogError("That command could not be found!");
                }
            }
        }
    }
}