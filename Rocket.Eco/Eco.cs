using System;
using System.Collections.Generic;

using Eco.Gameplay.Players;

using Rocket.API;
using Rocket.API.Eventing;
using Rocket.API.Logging;

using Rocket.Eco.Eventing;
using Rocket.Eco.Patches;
using Rocket.Eco.Player;

namespace Rocket.Eco
{
    public sealed class Eco : IImplementation, IEventEmitter
    {
        public string InstanceId => throw new NotImplementedException();
        public IEnumerable<string> Capabilities => new string[0];
        public bool IsAlive { get; } = true;

        public string Name => "Rocket.Eco";

        public static Eco Instance => _runtime.Container.Get<IImplementation>() as Eco;
        static IRuntime _runtime = null;

        public void Load(IRuntime runtime)
        {
            var patchManager = runtime.Container.Get<IPatchManager>();
            var logger = runtime.Container.Get<ILogger>();
            var eventManager = runtime.Container.Get<IEventManager>();

            _runtime = runtime;

            //TODO: Add Init() to IPatchManager
            (patchManager as PatchManager).Init(runtime);

            patchManager.RegisterPatch<UserPatch>(runtime);
            patchManager.RunPatching(runtime);

            //eventManager.AddEventListener(this, new EcoEventListener());

            logger.LogInformation("Rocket.Eco.E has initialized.");
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

            PlayerJoinEvent e = new PlayerJoinEvent(ecoPlayer);

            _runtime.Container.Get<IEventManager>().Emit(this, e);
        }

        internal void _EmitPlayerLeave(object player)
        {
            EcoPlayer ecoPlayer = new EcoPlayer((player as User).Player);

            PlayerLeaveEvent e = new PlayerLeaveEvent(ecoPlayer);

            _runtime.Container.Get<IEventManager>().Emit(this, e);
        }

        internal void _EmitEcoInit()
        {
            EcoInitEvent e = new EcoInitEvent();
            _runtime.Container.Get<IEventManager>().Emit(this, e);
        }
    }
}