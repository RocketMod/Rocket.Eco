using System;
using System.Collections.Generic;
using System.Reflection;
using Eco.Core.Plugins;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
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

        public IConsoleCommandCaller GetConsoleCaller() => new EcoConsoleCommandCaller(runtime.Container);

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

            ICommandCaller consoleCommandCaller = new EcoConsoleCommandCaller(runtime.Container);

            patchManager.RegisterPatch<UserPatch>();
            patchManager.RegisterPatch<ChatManagerPatch>();
            eventManager.AddEventListener(this, new EcoEventListener(runtime.Container));

            pluginManager.Init();

            PostInit(logger, consoleCommandCaller, commandHandler);
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

        private void PostInit(ILogger logger, ICommandCaller consoleCommandCaller, ICommandHandler commandHandler)
        {
            EcoUserActionDelegate playerJoin = _EmitPlayerJoin;
            EcoUserActionDelegate playerLeave = _EmitPlayerLeave;
            EcoUserChatDelegate playerChat = _EmitPlayerChat;

            Type userType = typeof(User);
            Type chatManagerType = typeof(ChatManager);

            userType.GetField("OnUserLogin").SetValue(null, playerJoin);
            userType.GetField("OnUserLogout").SetValue(null, playerLeave);
            chatManagerType.GetField("OnUserChat").SetValue(null, playerChat);

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

        internal void _EmitPlayerJoin(object user)
        {
            if (user == null || !(user is User castedUser))
                return;

            OnlineEcoPlayer ecoPlayer = new OnlineEcoPlayer(castedUser.Player, runtime.Container);

            /* TODO: This will be broken until I implement the EcoTaskScheduler.
            PlayerConnectedEvent e = new PlayerConnectedEvent(ecoPlayer, null, EventExecutionTargetContext.NextFrame);
            runtime.Container.Get<IEventManager>().Emit(this, e);
            */

            runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has joined!");
        }

        internal void _EmitPlayerLeave(object player)
        {
            if (player == null || !(player is User castedUser))
                return;

            OnlineEcoPlayer ecoPlayer = new OnlineEcoPlayer(castedUser.Player, runtime.Container);

            /* TODO: This will be broken until I implement the EcoTaskScheduler.
            PlayerDisconnectedEvent e = new PlayerDisconnectedEvent(ecoPlayer, null, EventExecutionTargetContext.NextFrame);
            runtime.Container.Get<IEventManager>().Emit(this, e);
            */

            runtime.Container.Get<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has left!");
        }

        internal bool _EmitPlayerChat(object user, string text)
        {
            if (user == null || !(user is User castedUser) || !castedUser.LoggedIn)
                return true;

            OnlineEcoPlayer p = new OnlineEcoPlayer(castedUser.Player, runtime.Container);

            IEventManager eventManager = runtime.Container.Get<IEventManager>();
            ILogger logger = runtime.Container.Get<ILogger>();

            if (text.StartsWith("/", StringComparison.InvariantCulture))
            {
                PreCommandExecutionEvent e1 = new PreCommandExecutionEvent(p, text.Remove(0, 1));
                eventManager.Emit(this, e1);

                bool wasCancelled = false;

                if (e1.IsCancelled)
                {
                    p.SendErrorMessage("Execution of your command has been cancelled!");
                    wasCancelled = true;

                    goto RETURN;
                }

                bool wasHandled = true;

                try
                {
                    wasHandled = runtime.Container.Get<ICommandHandler>().HandleCommand(p, text.Remove(0, 1), string.Empty);
                }
                catch (NotEnoughPermissionsException)
                {
                    p.SendErrorMessage("You do not have enough permission to execute this command!");
                    wasCancelled = true;
                }
                catch (Exception e)
                {
                    logger.LogError($"{p.Name} failed to execute the command `{text.Remove(0, 1).Split(' ')[0]}`!");
                    logger.LogError($"{e.Message}\n{e.StackTrace}");

                    p.SendErrorMessage("A runtime error occurred while executing this command, please contact an administrator!");

                    return true;
                }

                if (!wasHandled)
                    p.SendErrorMessage("That command could not be found!");

                RETURN:

                string canceled1 = wasCancelled ? ": CANCELLED" : "";

                logger.LogInformation($"[EVENT{canceled1}] [{p.Id}] {p.Name}: {text}");

                return true;
            }

            PlayerChatEvent e2 = new PlayerChatEvent(p, text)
            {
                IsCancelled = false
            };

            eventManager.Emit(this, e2);

            string canceled2 = e2.IsCancelled ? ": CANCELLED" : "";

            logger.LogInformation($"[EVENT{canceled2}] [{p.Id}] {p.Name}: {text}");

            return e2.IsCancelled;
        }
    }
}