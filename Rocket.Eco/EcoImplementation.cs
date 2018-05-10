using System;
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
using Rocket.API.User;
using Rocket.Core.Commands.Events;
using Rocket.Core.Implementation.Events;
using Rocket.Core.Logging;
using Rocket.Core.Permissions;
using Rocket.Core.Player.Events;
using Rocket.Eco.API.Legislation;
using Rocket.Eco.API.Patching;
using Rocket.Eco.Delegates;
using Rocket.Eco.Eventing;
using Rocket.Eco.Extensions;
using Rocket.Eco.Legislation;
using Rocket.Eco.Patches;
using Rocket.Eco.Player;

namespace Rocket.Eco
{
    /// <inheritdoc />
    /// <summary>
    ///     Rocket.Eco's implementation of Rocket's <see cref="IImplementation"/>.
    /// </summary>
    public sealed class EcoImplementation : IImplementation
    {
        private IRuntime runtime;

        /// <inheritdoc />
        public IConsole Console => new EcoConsole();

        /// <inheritdoc />
        public string InstanceId => throw new NotImplementedException();

        /// <inheritdoc />
        public bool IsAlive { get; } = true;

        /// <inheritdoc />
        public string WorkingDirectory => "./Rocket/";

        /// <inheritdoc />
        public string ConfigurationName => Name;

        /// <inheritdoc />
        public string Name => "Rocket.Eco";

        /// <inheritdoc />
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

            patchManager.RegisterPatch<UserPatch>();
            patchManager.RegisterPatch<ChatManagerPatch>();
            eventManager.AddEventListener(this, new EcoEventListener(runtime.Container));

            pluginManager.Init();

            runtime.Container.RegisterSingletonType<IPlayerManager, EcoPlayerManager>(null, "ecoplayermanager");
            runtime.Container.RegisterSingletonType<IGovernment, EcoGovernment>(null, "ecogovernment");

            PostInit(logger, Console, commandHandler);
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            StorageManager.SaveAndFlush();

            foreach (IPlugin plugin in runtime.Container.ResolvePluginManager())
                plugin.Unload();

            Environment.Exit(0);
        }

        /// <inheritdoc />
        public void Reload()
        {
            IPluginManager pluginManager = runtime.Container.ResolvePluginManager();

            foreach (IPlugin plugin in pluginManager)
                if (plugin.Unload())
                    plugin.Load(true);
        }

        private void PostInit(ILogger logger, IUser consoleCommandCaller, ICommandHandler commandHandler)
        {
            EcoUserActionDelegate playerJoin = _EmitPlayerJoin;
            EcoUserActionDelegate playerLeave = _EmitPlayerLeave;
            EcoUserChatDelegate playerChat = _EmitPlayerChat;

            Type userType = typeof(User);
            Type chatManagerType = typeof(ChatManager);

            userType.GetField("OnUserLogin").SetValue(null, playerJoin);
            userType.GetField("OnUserLogout").SetValue(null, playerLeave);
            chatManagerType.GetField("OnUserChat").SetValue(null, playerChat);

            ImplementationReadyEvent e = new ImplementationReadyEvent(this);
            runtime.Container.ResolveEventManager().Emit(this, e);

            logger.LogInformation("[EVENT] Eco has initialized!");

            while (true)
            {
                string input = System.Console.ReadLine();

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
            EcoPlayer ecoPlayer = playerManager?._Players.FirstOrDefault(x => x.Id.Equals(castedUser.SteamId)) ?? new EcoPlayer(castedUser, runtime.Container);

            UserConnectedEvent e = new UserConnectedEvent(ecoPlayer.User, null, EventExecutionTargetContext.NextFrame);
            runtime.Container.ResolveEventManager().Emit(this, e);

            runtime.Container.ResolveLogger().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has joined!");
        }

        internal void _EmitPlayerLeave(object player)
        {
            if (player == null || !(player is User castedUser))
                return;

            EcoPlayerManager playerManager = runtime.Container.ResolvePlayerManager("ecoplayermanager") as EcoPlayerManager;
            EcoPlayer ecoPlayer = playerManager?._Players.FirstOrDefault(x => x.Id.Equals(castedUser.SteamId));

            ILogger logger = runtime.Container.ResolveLogger();

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has left the game. Please report this to a Rocket.Eco developer!");
                return;
            }

            UserDisconnectedEvent e = new UserDisconnectedEvent(ecoPlayer, null, EventExecutionTargetContext.NextFrame);
            runtime.Container.ResolveEventManager().Emit(this, e);

            runtime.Container.ResolveLogger().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has left!");
        }

        internal bool _EmitPlayerChat(object user, string text)
        {
            if (user == null || !(user is User castedUser) || !castedUser.LoggedIn)
                return true;

            ILogger logger = runtime.Container.ResolveLogger();

            EcoPlayer ecoPlayer = (EcoPlayer) runtime.Container.ResolvePlayerManager("ecoplayermanager").GetOnlinePlayerById(castedUser.SteamId);

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has chatted. Please report this to a Rocket.Eco developer!");
                return false;
            }

            IEventManager eventManager = runtime.Container.ResolveEventManager();

            if (text.StartsWith("/", StringComparison.InvariantCulture))
            {
                PreCommandExecutionEvent commandEvent = new PreCommandExecutionEvent(ecoPlayer.User, text.Remove(0, 1));
                eventManager.Emit(this, commandEvent);

                bool wasCancelled = false;

                if (commandEvent.IsCancelled)
                {
                    ecoPlayer.SendErrorMessage("Execution of your command has been cancelled!");
                    wasCancelled = true;

                    goto RETURN;
                }

                bool wasHandled = true;

                try
                {
                    wasHandled = runtime.Container.ResolveCommandHandler().HandleCommand(ecoPlayer.User, text.Remove(0, 1), string.Empty);
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

                string commandCancelled = wasCancelled ? ": CANCELLED" : "";

                logger.LogInformation($"[EVENT{commandCancelled}] [{ecoPlayer.Id}] {ecoPlayer.Name}: {text}");

                return true;
            }

            UserChatEvent chatEvent = new UserChatEvent(ecoPlayer.User, text)
            {
                IsCancelled = false
            };

            eventManager.Emit(this, chatEvent);

            string chatCancelled = chatEvent.IsCancelled ? ": CANCELLED" : "";

            logger.LogInformation($"[EVENT{chatCancelled}] [{ecoPlayer.Id}] {ecoPlayer.Name}: {text}");

            return chatEvent.IsCancelled;
        }
    }
}