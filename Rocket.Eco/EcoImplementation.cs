using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Eco.Core.Plugins;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Configuration;
using Rocket.API.Economy;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.API.Scheduler;
using Rocket.API.User;
using Rocket.Core.Commands.Events;
using Rocket.Core.Implementation.Events;
using Rocket.Core.Logging;
using Rocket.Core.Permissions;
using Rocket.Core.Player.Events;
using Rocket.Eco.API.Legislation;
using Rocket.Eco.API.Patching;
using Rocket.Eco.Commands;
using Rocket.Eco.Delegates;
using Rocket.Eco.Economy;
using Rocket.Eco.Eventing;
using Rocket.Eco.Extensions;
using Rocket.Eco.Legislation;
using Rocket.Eco.Patches;
using Rocket.Eco.Player;
using Rocket.Eco.Scheduling;

namespace Rocket.Eco
{
    /// <inheritdoc />
    /// <summary>
    ///     Rocket.Eco's implementation of Rocket's <see cref="IImplementation" />.
    /// </summary>
    public sealed class EcoImplementation : IImplementation
    {
        private EcoConsole console;
        private IRuntime runtime;

        /// <inheritdoc />
        public IConsole Console => console ?? (console = new EcoConsole());

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
            console.Init(runtime.Container);

            IPatchManager patchManager = runtime.Container.Resolve<IPatchManager>();
            ILogger logger = runtime.Container.Resolve<ILogger>();
            IEventManager eventManager = runtime.Container.Resolve<IEventManager>();
            IPluginManager pluginManager = runtime.Container.Resolve<IPluginManager>();
            ICommandHandler commandHandler = runtime.Container.Resolve<ICommandHandler>();
            ConfigurationPermissionProvider permissionProvider = (ConfigurationPermissionProvider) runtime.Container.Resolve<IPermissionProvider>("default_permissions");

            //TODO: Add a IConfiguration.TryGetSection method.
            try
            {
                permissionProvider.PlayersConfig.GetSection("EcoUser");
            }
            catch
            {
                logger.LogInformation("Detected first-time initialization for Permissions config, regenerating it now.");

                permissionProvider.PlayersConfig.CreateSection("EcoUser", SectionType.Array);
                permissionProvider.PlayersConfig.Save();
            }

            patchManager.RegisterPatch<UserPatch>();
            patchManager.RegisterPatch<ChatManagerPatch>();
            eventManager.AddEventListener(this, new EcoEventListener(runtime.Container));

            pluginManager.Init();

            EcoPlayerManager ecoPlayerManager = new EcoPlayerManager(runtime.Container);

            runtime.Container.RegisterSingletonInstance<IUserManager>(ecoPlayerManager, "ecousermanager");
            runtime.Container.RegisterSingletonInstance<IPlayerManager>(ecoPlayerManager, null, "ecoplayermanager");
            runtime.Container.RegisterSingletonType<IGovernment, EcoGovernment>(null, "ecogovernment");
            runtime.Container.RegisterSingletonType<ITaskScheduler, EcoTaskScheduler>(null, "ecotaskscheduler");

#if DEBUG
            runtime.Container.RegisterSingletonType<IEconomyProvider, EcoEconomyProvider>(null, "ecoeconomyprovider");
#endif

            //This throws a StackOverflowException if not done this way do to how Unity's Dependency Container works.
            runtime.Container.RegisterSingletonInstance<ICommandProvider>(new EcoVanillaCommandProvider(runtime.Container.Resolve<ICommandProvider>().Commands, runtime.Container), "ecovanillacommandprovider");

            PostInit(logger, Console, commandHandler, ecoPlayerManager);
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            runtime.Container.Resolve<ITaskScheduler>()
                   .ScheduleNextFrame(this, () =>
                   {
                       runtime.Container.Resolve<ILogger>().LogInformation("The shutdown sequence has been initiated.");

                       StorageManager.SaveAndFlush();

                       foreach (IPlugin plugin in runtime.Container.Resolve<IPluginManager>().Plugins)
                           plugin.Unload();

                       EcoPlayerManager playerManager = (EcoPlayerManager) runtime.Container.Resolve<IPlayerManager>("ecoplayermanager");

                       foreach (EcoPlayer player in playerManager.OnlinePlayers.Cast<EcoPlayer>())
                           playerManager.Kick(player.User, null, "The server is shutting down.");

                       //TODO: This appears to cause a StackOverflowException.
                       //runtime.Shutdown();

                       Thread.Sleep(2000);

                       Environment.Exit(0);
                   });
        }

        /// <inheritdoc />
        public void Reload()
        {
            foreach (IPlugin plugin in runtime.Container.Resolve<IPluginManager>())
                if (plugin.Unload())
                    plugin.Load(true);
        }

        private void PostInit(ILogger logger, IUser consoleCommandCaller, ICommandHandler commandHandler, EcoPlayerManager playerManager)
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
            runtime.Container.Resolve<IEventManager>().Emit(this, e);

            logger.LogInformation("[EVENT] Eco has initialized!");

            while (true)
            {
                string input = System.Console.ReadLine();

                if (input == null)
                    continue;

                runtime.Container.Resolve<ITaskScheduler>("ecotaskscheduler")
                       .ScheduleNextFrame(this, () =>
                       {
                           if (input.StartsWith("/", StringComparison.InvariantCulture))
                               input = input.Remove(0, 1);

                           bool wasHandled = commandHandler.HandleCommand(consoleCommandCaller, input, string.Empty);

                           if (!wasHandled)
                               logger.LogError("That command could not be found!");
                       });
            }
        }

        internal void _EmitPlayerJoin(object user)
        {
            if (user == null || !(user is User castedUser))
                return;

            EcoPlayerManager playerManager = runtime.Container.Resolve<IPlayerManager>("ecoplayermanager") as EcoPlayerManager;
            EcoPlayer ecoPlayer = playerManager?._Players.FirstOrDefault(x => x.Id.Equals(castedUser.SlgId) || x.Id.Equals(castedUser.SteamId));

            string firstTime = string.Empty;

            if (ecoPlayer == null)
            {
                ecoPlayer = new EcoPlayer(castedUser, runtime.Container.Resolve<IUserManager>("ecousermanager"), runtime.Container);
                playerManager?._Players.Add(ecoPlayer);

                firstTime = " for the first time!";
            }

            if (ecoPlayer.InternalEcoUser == null)
            {
                ecoPlayer.BuildReference(castedUser);

                firstTime = " for the first time!";
            }

            ConfigurationPermissionProvider permissionProvider = (ConfigurationPermissionProvider) runtime.Container.Resolve<IPermissionProvider>("default_permissions");

            IConfigurationSection configurationSection = permissionProvider.PlayersConfig["EcoUser"];

            List<PlayerPermissionSection> playerPermissions = configurationSection.Get<PlayerPermissionSection[]>().ToList();

            if (playerPermissions.FirstOrDefault(x => x.Id.Equals(ecoPlayer.Id)) == null)
            {
                IEnumerable<GroupPermissionSection> autoGroups = permissionProvider.GroupsConfig["Groups"].Get<GroupPermissionSection[]>().Where(x => x.AutoAssign);

                PlayerPermissionSection section = new PlayerPermissionSection
                {
                    Id = ecoPlayer.Id,
                    Permissions = new string[0],
                    Groups = autoGroups.Select(x => x.Id).ToArray()
                };

                playerPermissions.Add(section);

                configurationSection.Set(playerPermissions);

                permissionProvider.PlayersConfig.Save();
            }

            UserConnectedEvent e = new UserConnectedEvent(ecoPlayer.User, null, EventExecutionTargetContext.NextFrame);
            runtime.Container.Resolve<IEventManager>().Emit(this, e);

            runtime.Container.Resolve<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has joined{firstTime}!");
        }

        internal void _EmitPlayerLeave(object player)
        {
            if (player == null || !(player is User castedUser))
                return;

            EcoPlayerManager playerManager = runtime.Container.Resolve<IPlayerManager>("ecoplayermanager") as EcoPlayerManager;
            EcoPlayer ecoPlayer = playerManager?._Players.FirstOrDefault(x => x.Id.Equals(castedUser.SteamId));

            ILogger logger = runtime.Container.Resolve<ILogger>();

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has left the game. Please report this to a Rocket.Eco developer!");
                return;
            }

            UserDisconnectedEvent e = new UserDisconnectedEvent(ecoPlayer, null, EventExecutionTargetContext.NextFrame);
            runtime.Container.Resolve<IEventManager>().Emit(this, e);

            runtime.Container.Resolve<ILogger>().LogInformation($"[EVENT] [{ecoPlayer.Id}] {ecoPlayer.Name} has left!");
        }

        internal bool _EmitPlayerChat(object user, string text)
        {
            if (user == null || !(user is User castedUser) || !castedUser.LoggedIn)
                return true;

            ILogger logger = runtime.Container.Resolve<ILogger>();

            EcoPlayer ecoPlayer = (EcoPlayer) runtime.Container.Resolve<IPlayerManager>("ecoplayermanager").GetOnlinePlayerById(castedUser.SteamId);

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has chatted. Please report this to a Rocket.Eco developer!");
                return false;
            }

            IEventManager eventManager = runtime.Container.Resolve<IEventManager>();

            if (text.StartsWith("/", StringComparison.InvariantCulture))
            {
                PreCommandExecutionEvent commandEvent = new PreCommandExecutionEvent(ecoPlayer.User, text.Remove(0, 1));
                eventManager.Emit(this, commandEvent);

                if (commandEvent.IsCancelled)
                {
                    ecoPlayer.SendErrorMessage("Execution of your command has been cancelled!");

                    goto RETURN;
                }

                runtime.Container.Resolve<ITaskScheduler>("ecotaskscheduler")
                       .ScheduleNextFrame(this, () =>
                       {
                           bool wasHandled = true;

                           try
                           {
                               wasHandled = runtime.Container.Resolve<ICommandHandler>().HandleCommand(ecoPlayer.User, text.Remove(0, 1), string.Empty);
                           }
                           catch (NotEnoughPermissionsException)
                           {
                               ecoPlayer.SendErrorMessage("You do not have enough permission to execute this command!");
                           }
                           catch (Exception e)
                           {
                               logger.LogError($"{ecoPlayer.Name} failed to execute the command `{text.Remove(0, 1).Split(' ')[0]}`!");
                               logger.LogError($"{e.Message}\n{e.StackTrace}");

                               ecoPlayer.SendErrorMessage("A runtime error occurred while executing this command, please contact an administrator!");

                               return;
                           }

                           if (!wasHandled)
                               ecoPlayer.SendErrorMessage("That command could not be found!");
                       });

                RETURN:
                return true;
            }

            UserChatEvent chatEvent = new UserChatEvent(ecoPlayer.User, text)
            {
                IsCancelled = false
            };

            eventManager.Emit(this, chatEvent);

            string commandCancelled = chatEvent.IsCancelled ? "[CANCELLED] " : "";
            logger.LogInformation($"{commandCancelled}[{ecoPlayer.Id}] {ecoPlayer.Name}: {text}");

            return chatEvent.IsCancelled;
        }
    }
}