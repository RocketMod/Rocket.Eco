using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eco.Core.Plugins;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.Core.Commands.Events;
using Rocket.Core.Permissions;
using Rocket.Core.Player.Events;
using Rocket.Eco.API.Legislation;
using Rocket.Eco.API.Patching;
using Rocket.Eco.Delegates;
using Rocket.Eco.Eventing;
using Rocket.Eco.Events;
using Rocket.Eco.Extensions;
using Rocket.Eco.Legislation;
using Rocket.Eco.Patches;
using Rocket.Eco.Player;

namespace Rocket.Eco
{
    public sealed class EcoImplementation : IImplementation
    {
        private IRuntime runtime;
        public IEnumerable<string> Capabilities => new string[0];

        public IConsoleCommandCaller ConsoleCommandCaller => new EcoConsoleCommandCaller(runtime.Container);

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

            IPatchManager patchManager = runtime.Container.ResolvePatchManager();
            ILogger logger = runtime.Container.ResolveLogger();
            IEventManager eventManager = runtime.Container.ResolveEventManager();
            IPluginManager pluginManager = runtime.Container.ResolvePluginManager();
            ICommandHandler commandHandler = runtime.Container.ResolveCommandHandler();

            ICommandCaller consoleCommandCaller = new EcoConsoleCommandCaller(runtime.Container);

            patchManager.RegisterPatch<UserPatch>();
            patchManager.RegisterPatch<ChatManagerPatch>();
            eventManager.AddEventListener(this, new EcoEventListener(runtime.Container));

            pluginManager.Init();

            runtime.Container.RegisterSingletonType<IPlayerManager, EcoPlayerManager>(null, "ecoplayermanager");
            runtime.Container.RegisterSingletonType<IGovernment, EcoGovernment>(null, "ecogovernment");

            PostInit(logger, consoleCommandCaller, commandHandler);
        }

        public void Shutdown()
        {
            StorageManager.SaveAndFlush();

            foreach (IPlugin plugin in runtime.Container.ResolvePluginManager())
                plugin.Deactivate();

            Environment.Exit(0);
        }

        public void Reload()
        {
            IPluginManager pluginManager = runtime.Container.ResolvePluginManager();

            foreach (IPlugin plugin in pluginManager)
                if (plugin.Deactivate())
                    plugin.Activate();
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
            runtime.Container.ResolveEventManager().Emit(this, e);

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

            EcoPlayerManager playerManager = runtime.Container.ResolvePlayerManager("ecoplayermanager") as EcoPlayerManager;
            EcoPlayer ecoPlayer = playerManager?._Players.FirstOrDefault(x => x.Id.Equals(castedUser.SteamId));

            ILogger logger = runtime.Container.ResolveLogger();

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has left the game. Please report this to a Rocket.Eco developer!");
                return;
            }

            OnlineEcoPlayer onlineEcoPlayer = new OnlineEcoPlayer(castedUser.Player, runtime.Container);

            playerManager._Players.Remove(ecoPlayer);
            playerManager._Players.Add(onlineEcoPlayer);

            PlayerConnectedEvent e = new PlayerConnectedEvent(onlineEcoPlayer, null, EventExecutionTargetContext.NextFrame);
            runtime.Container.ResolveEventManager().Emit(this, e);

            runtime.Container.ResolveLogger().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has joined!");
        }

        internal void _EmitPlayerLeave(object player)
        {
            if (player == null || !(player is User castedUser))
                return;

            EcoPlayerManager playerManager = runtime.Container.ResolvePlayerManager("ecoplayermanager") as EcoPlayerManager;
            OnlineEcoPlayer ecoPlayer = playerManager?._Players.FirstOrDefault(x => x.Id.Equals(castedUser.SteamId)) as OnlineEcoPlayer;

            ILogger logger = runtime.Container.ResolveLogger();

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has left the game. Please report this to a Rocket.Eco developer!");
                return;
            }

            playerManager._Players.Remove(ecoPlayer);
            playerManager._Players.Add(new EcoPlayer(castedUser, runtime.Container));

            PlayerDisconnectedEvent e = new PlayerDisconnectedEvent(ecoPlayer, null, EventExecutionTargetContext.NextFrame);
            runtime.Container.ResolveEventManager().Emit(this, e);

            runtime.Container.ResolveLogger().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has left!");
        }

        internal bool _EmitPlayerChat(object user, string text)
        {
            if (user == null || !(user is User castedUser) || !castedUser.LoggedIn)
                return true;

            ILogger logger = runtime.Container.ResolveLogger();

            OnlineEcoPlayer ecoPlayer = (OnlineEcoPlayer) runtime.Container.ResolvePlayerManager("ecoplayermanager").GetOnlinePlayerById(castedUser.SteamId);

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has chatted. Please report this to a Rocket.Eco developer!");
                return false;
            }

            IEventManager eventManager = runtime.Container.ResolveEventManager();

            if (text.StartsWith("/", StringComparison.InvariantCulture))
            {
                PreCommandExecutionEvent e1 = new PreCommandExecutionEvent(ecoPlayer, text.Remove(0, 1));
                eventManager.Emit(this, e1);

                bool wasCancelled = false;

                if (e1.IsCancelled)
                {
                    ecoPlayer.SendErrorMessage("Execution of your command has been cancelled!");
                    wasCancelled = true;

                    goto RETURN;
                }

                bool wasHandled = true;

                try
                {
                    wasHandled = runtime.Container.ResolveCommandHandler().HandleCommand(ecoPlayer, text.Remove(0, 1), string.Empty);
                }
                catch (NotEnoughPermissionsException)
                {
                    ecoPlayer.SendErrorMessage("You do not have enough permission to execute this command!");
                    wasCancelled = true;
                }
                catch (Exception e)
                {
                    logger.LogError($"{ecoPlayer.Name} failed to execute the command `{text.Remove(0, 1).Split(' ')[0]}`!");
                    logger.LogError($"{e.Message}\n{e.StackTrace}");

                    ecoPlayer.SendErrorMessage("A runtime error occurred while executing this command, please contact an administrator!");

                    return true;
                }

                if (!wasHandled)
                    ecoPlayer.SendErrorMessage("That command could not be found!");

                RETURN:

                string canceled1 = wasCancelled ? ": CANCELLED" : "";

                logger.LogInformation($"[EVENT{canceled1}] [{ecoPlayer.Id}] {ecoPlayer.Name}: {text}");

                return true;
            }

            PlayerChatEvent e2 = new PlayerChatEvent(ecoPlayer, text)
            {
                IsCancelled = false
            };

            eventManager.Emit(this, e2);

            string canceled2 = e2.IsCancelled ? ": CANCELLED" : "";

            logger.LogInformation($"[EVENT{canceled2}] [{ecoPlayer.Id}] {ecoPlayer.Name}: {text}");

            return e2.IsCancelled;
        }
    }
}