using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Eco.Core.Plugins;
using Eco.Gameplay.Players;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Plugin;
using Rocket.Core.Commands.Events;
using Rocket.Core.Permissions;
using Rocket.Core.Player.Events;
using Rocket.Eco.API.Patching;
using Rocket.Eco.Delegates;
using Rocket.Eco.Eventing;
using Rocket.Eco.Events;
using Rocket.Eco.Patches;
using Rocket.Eco.Player;

namespace Rocket.Eco
{
    public sealed class EcoImplementation : IImplementation
    {
        private IRuntime runtime;
        public IEnumerable<string> Capabilities => new string[0];

        public IConsoleCommandCaller GetConsoleCaller() => new EcoConsoleCommandCaller(runtime);

        public string InstanceId => throw new NotImplementedException();
        public bool IsAlive { get; } = true;
        public string WorkingDirectory => "./Rocket/";
        public string ConfigurationName => Name;
        public string Name => "Rocket.Eco";

        public void Init(IRuntime runtime)
        {
            if (Assembly.GetCallingAssembly().GetName().Name != "Rocket.Runtime")
                throw new MethodAccessException();

            this.runtime = runtime;
            
            IPatchManager patchManager = runtime.Container.Get<IPatchManager>();
            ILogger logger = runtime.Container.Get<ILogger>();
            IEventManager eventManager = runtime.Container.Get<IEventManager>();
            IPluginManager pluginManager = runtime.Container.Get<IPluginManager>();
            ICommandHandler commandHandler = runtime.Container.Get<ICommandHandler>();

            ICommandCaller consoleCommandCaller = new EcoConsoleCommandCaller(runtime);
            
            patchManager.RegisterPatch<UserPatch>();
            eventManager.AddEventListener(this, new EcoEventListener(runtime));
            
            pluginManager.Init();
            
            PostInit(logger, consoleCommandCaller, commandHandler);
        }

        private void PostInit(ILogger logger, ICommandCaller consoleCommandCaller, ICommandHandler commandHandler)
        {
            EcoUserActionDelegate playerJoin = _EmitPlayerJoin;
            EcoUserActionDelegate playerLeave = _EmitPlayerLeave;

            Type type = typeof(User);
            
            type.GetField("OnUserLogin").SetValue(null, playerJoin);
            type.GetField("OnUserLogout").SetValue(null, playerLeave);
            
            EcoReadyEvent e = new EcoReadyEvent(this);
            runtime.Container.Get<IEventManager>().Emit(this, e);

            logger.LogInformation("[EVENT] Eco has initialized!");

            while (true)
            {
                string input = Console.ReadLine();

                if (input == null)
                    continue;

                if (input.StartsWith("/", StringComparison.InvariantCulture))
                    input = input.Remove(0, 1);

                bool wasHandled = commandHandler.HandleCommand(consoleCommandCaller, input, string.Empty);

                if (!wasHandled)
                    logger.LogError("That command could not be found!");
            }
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

        internal void _EmitPlayerJoin(object user)
        {
            if (user == null || !(user is User castedUser)) return;

            OnlineEcoPlayer ecoPlayer = new OnlineEcoPlayer(castedUser.Player, runtime.Container);
            PlayerConnectedEvent e = new PlayerConnectedEvent(ecoPlayer, "");

            runtime.Container.Get<IEventManager>().Emit(this, e);

            runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has joined!");

            StackTrace trace = new StackTrace();

            foreach (StackFrame frame in trace.GetFrames())
            {
                Console.Write(frame.GetMethod().ReflectedType);
                Console.Write(".");
                Console.WriteLine(frame.GetMethod().Name);
            }
        }

        internal void _EmitPlayerLeave(object player)
        {
            if (player == null || !(player is User castedUser)) return;

            OnlineEcoPlayer ecoPlayer = new OnlineEcoPlayer(castedUser.Player, runtime.Container);
            PlayerDisconnectedEvent e = new PlayerDisconnectedEvent(ecoPlayer, "");

            runtime.Container.Get<IEventManager>().Emit(this, e);

            runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has left!");

            StackTrace trace = new StackTrace();

            foreach (StackFrame frame in trace.GetFrames())
            {
                Console.Write(frame.GetMethod().ReflectedType);
                Console.Write(".");
                Console.WriteLine(frame.GetMethod().Name);
            }
        }

        //TODO: Implement
        internal bool _EmitPlayerChat(string text, object user)
        {
            Console.WriteLine(Thread.CurrentThread.Name);
            if (user == null || !(user is User castedUser) || !castedUser.LoggedIn) return true;

            OnlineEcoPlayer p = new OnlineEcoPlayer(castedUser.Player, runtime.Container);

            IEventManager eventManager = runtime.Container.Get<IEventManager>();

            if (text.StartsWith("/", StringComparison.InvariantCulture))
            {
                PreCommandExecutionEvent e1 = new PreCommandExecutionEvent(p, text.Remove(0, 1));
                eventManager.Emit(this, e1);

                if (e1.IsCancelled)
                {
                    p.SendErrorMessage("Execution of your command has been cancelled!");
                    return true;
                }

                bool wasHandled = true;

                try
                {
                    wasHandled = runtime.Container.Get<ICommandHandler>().HandleCommand(p, text.Remove(0, 1), string.Empty);
                }
                catch (NotEnoughPermissionsException)
                {
                    p.SendErrorMessage("You do not have enough permission to execute this command!");
                }
                catch (Exception e)
                {
                    runtime.Container.Get<ILogger>().LogError($"{p.Name} failed to execute the command `{text.Remove(0, 1).Split(' ')[0]}`!", e);
                }

                if (!wasHandled)
                    p.SendErrorMessage("That command could not be found!");

                return true;
            }

            PlayerChatEvent e2 = new PlayerChatEvent(p, string.Empty, text);
            eventManager.Emit(this, e2);

            return !e2.IsCancelled;
        }
    }
}