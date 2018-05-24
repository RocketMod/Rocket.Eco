using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Eco.Core.Plugins;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Eco.Plugins.Networking;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Configuration;
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
using Rocket.Core.User.Events;
using Rocket.Eco.API.Patching;
using Rocket.Eco.Commands;
using Rocket.Eco.Delegates;
using Rocket.Eco.Eventing;
using Rocket.Eco.Patches;
using Rocket.Eco.Player;
using Rocket.Eco.Scheduling;

#if DEBUG
using Rocket.Eco.API.Legislation;
using Rocket.Eco.Economy;
using Rocket.Eco.Legislation;
using Rocket.API.Economy;
#endif

namespace Rocket.Eco
{
    /// <inheritdoc />
    /// <summary>
    ///     Rocket.Eco's implementation of Rocket's <see cref="IImplementation" />.
    /// </summary>
    public sealed class EcoImplementation : IImplementation
    {
        private ICommandHandler commandHandler;
        private EcoConsole console;
        private IEventManager eventManager;
        private ILogger logger;
        private ConfigurationPermissionProvider permissionProvider;
        private EcoPlayerManager playerManager;
        private IPluginManager pluginManager;

        private IRuntime runtime;
        private ITaskScheduler taskScheduler;

        /// <inheritdoc />
        public IConsole Console => console ?? (console = new EcoConsole());

        /// <inheritdoc />
        public string InstanceId => NetworkManager.Config?.Description ?? "Unknown";

        /// <inheritdoc />
        public bool IsAlive => true;

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

            logger = runtime.Container.Resolve<ILogger>();
            eventManager = runtime.Container.Resolve<IEventManager>();
            pluginManager = runtime.Container.Resolve<IPluginManager>();
            commandHandler = runtime.Container.Resolve<ICommandHandler>();
            taskScheduler = runtime.Container.Resolve<ITaskScheduler>("ecotaskscheduler");

            permissionProvider = (ConfigurationPermissionProvider) runtime.Container.Resolve<IPermissionProvider>("default_permissions");

            patchManager.RegisterPatch<UserPatch>();
            patchManager.RegisterPatch<ChatManagerPatch>();
            eventManager.AddEventListener(this, new EcoEventListener(runtime.Container));

            pluginManager.Init();

            playerManager = new EcoPlayerManager(runtime.Container);

            //TODO: This can go into DependencyRegistrator.cs after patching is migrated 
            runtime.Container.RegisterSingletonInstance<IUserManager>(playerManager, "ecousermanager");
            runtime.Container.RegisterSingletonInstance<IPlayerManager>(playerManager, null, "ecoplayermanager");
            runtime.Container.RegisterSingletonType<ITaskScheduler, EcoTaskScheduler>(null, "ecotaskscheduler");
            runtime.Container.RegisterSingletonInstance<ICommandProvider>(new EcoNativeCommandProvider(this, runtime.Container), "econativecommandprovider");

#if DEBUG
            runtime.Container.RegisterSingletonType<IGovernment, EcoGovernment>(null, "ecogovernment");
            runtime.Container.RegisterSingletonType<IEconomyProvider, EcoEconomyProvider>(null, "ecoeconomyprovider");
#endif

            CheckConfig();
            PostInit();

            eventManager.Emit(this, new ImplementationReadyEvent(this));

            logger.LogInformation($"Rocket has initialized under the server name {InstanceId}!");

            while (true)
            {
                string input = System.Console.ReadLine();

                if (input == null)
                    continue;

                taskScheduler
                    .ScheduleNextFrame(this, () =>
                    {
                        if (input.StartsWith("/", StringComparison.InvariantCulture))
                            input = input.Remove(0, 1);

                        bool wasHandled = commandHandler.HandleCommand(console, input, string.Empty);

                        if (!wasHandled)
                            logger.LogError("That command could not be found!");
                    });
            }
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            taskScheduler
                .ScheduleNextFrame(this, () =>
                {
                    logger.LogInformation("The shutdown sequence has been initiated.");

                    StorageManager.SaveAndFlush();

                    foreach (IPlugin plugin in pluginManager)
                        plugin.Unload();

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
            foreach (IPlugin plugin in pluginManager)
                if (plugin.Unload())
                    plugin.Load(true);
        }

        private void PostInit()
        {
            EcoUserActionDelegate playerJoin = _EmitPlayerJoin;
            EcoUserActionDelegate playerLeave = _EmitPlayerLeave;
            EcoUserChatDelegate playerChat = _EmitPlayerChat;

            Type userType = typeof(User);
            Type chatManagerType = typeof(ChatManager);

            userType.GetField("OnUserLogin").SetValue(null, playerJoin);
            userType.GetField("OnUserLogout").SetValue(null, playerLeave);
            chatManagerType.GetField("OnUserChat").SetValue(null, playerChat);
        }

        private void CheckConfig()
        {
            //TODO: Add a IConfiguration.TryGetSection method to Rocket.API.
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
        }

        internal void _EmitPlayerJoin(object user)
        {
            if (user == null || !(user is User castedUser))
                return;

            EcoPlayer ecoPlayer = playerManager?.InternalPlayersList.FirstOrDefault(x => x.Id.Equals(castedUser.SlgId) || x.Id.Equals(castedUser.SteamId));

            string firstTime = string.Empty;

            if (ecoPlayer == null)
            {
                ecoPlayer = new EcoPlayer(castedUser, playerManager, runtime.Container);
                playerManager?.InternalPlayersList.Add(ecoPlayer);

                firstTime = " for the first time!";
            }
            else if (ecoPlayer.InternalEcoUser == null)
            {
                ecoPlayer.BuildReference(castedUser);

                firstTime = " for the first time!";
            }

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

            eventManager.Emit(this, new UserConnectedEvent(ecoPlayer.User, null, EventExecutionTargetContext.NextFrame));

            logger.LogInformation($"[{ecoPlayer.Id}] {ecoPlayer.Name} has joined{firstTime}!");
        }

        internal void _EmitPlayerLeave(object player)
        {
            if (player == null || !(player is User castedUser))
                return;

            EcoPlayer ecoPlayer = playerManager?.InternalPlayersList.FirstOrDefault(x => x.Id.Equals(castedUser.SteamId));

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has left the game. Please report this to a Rocket.Eco developer!");
                return;
            }

            eventManager.Emit(this, new UserDisconnectedEvent(ecoPlayer.User, null, EventExecutionTargetContext.NextFrame));

            logger.LogInformation($"[{ecoPlayer.Id}] {ecoPlayer.Name} has left!");
        }

        internal bool _EmitPlayerChat(object user, string text)
        {
            if (user == null || !(user is User castedUser) || !castedUser.LoggedIn)
                return true;

            EcoPlayer ecoPlayer = (EcoPlayer) playerManager.GetOnlinePlayerById(castedUser.SteamId);

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has chatted. Please report this to a Rocket.Eco developer!");
                return false;
            }

            if (text.StartsWith("/", StringComparison.InvariantCulture))
            {
                PreCommandExecutionEvent commandEvent = new PreCommandExecutionEvent(ecoPlayer.User, text.Remove(0, 1));
                eventManager.Emit(this, commandEvent);

                if (commandEvent.IsCancelled)
                {
                    ecoPlayer.SendErrorMessage("Execution of your command has been cancelled!");

                    return true;
                }

                taskScheduler
                    .ScheduleNextFrame(this, () =>
                    {
                        bool wasHandled = true;

                        try
                        {
                            wasHandled = commandHandler.HandleCommand(ecoPlayer.User, text.Remove(0, 1), string.Empty);
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