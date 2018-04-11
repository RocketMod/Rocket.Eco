using System;
using System.Collections.Generic;
using System.IO;
using Eco.Gameplay.Players;

using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Eventing;
using Rocket.API.Logging;

using Rocket.Core.Events.Player;

using Rocket.Eco.Eventing;
using Rocket.Eco.Patches;
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

        public static Eco Instance => _runtime.Container.Get<IImplementation>() as Eco;

        static IRuntime _runtime = null;

        internal static object[] launchArgs;
        internal static bool isExtraction;

        public void Load(IRuntime runtime)
        {
            var patchManager = runtime.Container.Get<IPatchManager>();
            var logger = runtime.Container.Get<ILogger>();
            var eventManager = runtime.Container.Get<IEventManager>();

            _runtime = runtime;
            
            patchManager.Init(runtime);
            patchManager.RegisterPatch<UserPatch>(runtime);
            patchManager.RunPatching(runtime);

            //eventManager.AddEventListener(this, new EcoEventListener());

            logger.LogInformation("Rocket bootstrapping is finished.");
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

            _runtime.Container.Get<IEventManager>().Emit(this, e);

            _runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has joined!");
        }

        internal void _EmitPlayerLeave(object player)
        {
            EcoPlayer ecoPlayer = new EcoPlayer((player as User).Player);
            PlayerDisconnectEvent e = new PlayerDisconnectEvent(ecoPlayer, "");

            _runtime.Container.Get<IEventManager>().Emit(this, e);

            _runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has left!");
        }

        internal void _EmitEcoInit()
        {
            EcoInitEvent e = new EcoInitEvent();
            _runtime.Container.Get<IEventManager>().Emit(this, e);

            _runtime.Container.Get<ILogger>().LogInformation("[EVENT] Eco has initialized!");
        }

        internal bool _ProcessCommand(string text, object user)
        {
            if (text.StartsWith("/"))
            {
                EcoPlayer p = new EcoPlayer((user as User).Player);
                bool wasHandled = _runtime.Container.Get<ICommandHandler>().HandleCommand(p, text);

                if (!wasHandled)
                {
                    p.Player.SendTemporaryErrorLoc("That command could not be found!");
                }

                return true;
            }

            return false;
        }
    }
}