using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Eco.Core.Plugins;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Eco.Plugins.Networking;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Configuration;
using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.API.Scheduling;
using Rocket.Core.Commands.Events;
using Rocket.Core.Implementation.Events;
using Rocket.Core.Logging;
using Rocket.Core.Permissions;
using Rocket.Core.Player.Events;
using Rocket.Core.Scheduling;
using Rocket.Core.User;
using Rocket.Eco.Player;

#if DEBUG
#endif

namespace Rocket.Eco
{
    /// <inheritdoc />
    /// <summary>
    ///     Rocket.Eco's implementation of Rocket's <see cref="IHost" />.
    /// </summary>
    public sealed class EcoHost : IHost
    {
        private ICommandHandler commandHandler;
        private IEventBus eventManager;
        private ILogger logger;
        private ConfigurationPermissionProvider permissionProvider;
        private EcoPlayerManager playerManager;
        private IPluginLoader pluginManager;

        private IRuntime runtime;
        private ITaskScheduler taskScheduler;

        /// <inheritdoc />
        public EcoHost(IDependencyContainer container)
        {
            Console = new StdConsole(container);
        }

        /// <inheritdoc />
        public ushort ServerPort
        {
            get
            {
                if (NetworkManager.Config == null) return 0;

                return (ushort) NetworkManager.Config.GameServerPort;
            }
        }

        /// <inheritdoc />
        public IConsole Console { get; }

        /// <inheritdoc />
        public string GameName => "Eco";

        /// <inheritdoc />
        public Version GameVersion => AppDomain.CurrentDomain.GetAssemblies()
                                               .First(x => x.GetName().Name == "Eco.Gameplay")
                                               .GetName()
                                               .Version;

        /// <inheritdoc />
        public string ServerName => NetworkManager.Config?.Description ?? "Unknown";

        /// <inheritdoc />
        public bool IsAlive => true;

        /// <inheritdoc />
        public string WorkingDirectory => "./Rocket/";

        /// <inheritdoc />
        public string ConfigurationName => Name;

        /// <inheritdoc />
        public string Name => "Rocket.Eco";

        /// <inheritdoc />
        public async Task InitAsync(IRuntime runtime)
        {
            if (Assembly.GetCallingAssembly().GetName().Name != "Rocket.Runtime")
                throw new MethodAccessException();

            this.runtime = runtime;

            logger = runtime.Container.Resolve<ILogger>();
            eventManager = runtime.Container.Resolve<IEventBus>();
            pluginManager = runtime.Container.Resolve<IPluginLoader>();
            commandHandler = runtime.Container.Resolve<ICommandHandler>();
            taskScheduler = runtime.Container.Resolve<ITaskScheduler>();
            permissionProvider =
                (ConfigurationPermissionProvider) runtime.Container.Resolve<IPermissionProvider>("default_permissions");
            playerManager = (EcoPlayerManager) runtime.Container.Resolve<IPlayerManager>("eco");

            CheckConfig();

            Func<object, bool> prePlayerJoin = _EmitPlayerPreJoin;
            Action<object> playerJoin = _EmitPlayerJoin;
            Action<object> playerLeave = _EmitPlayerLeave;
            Func<object, string, bool> playerChat = _EmitPlayerChat;

            Type userType = typeof(User);
            Type chatManagerType = typeof(ChatManager);

            userType.GetField("OnUserPreLogin").SetValue(null, prePlayerJoin);
            userType.GetField("OnUserLogin").SetValue(null, playerJoin);
            userType.GetField("OnUserLogout").SetValue(null, playerLeave);
            chatManagerType.GetField("OnUserChat").SetValue(null, playerChat);

            await pluginManager.InitAsync();

            eventManager.Emit(this, new ImplementationReadyEvent(this));

            logger.LogInformation($"Rocket has initialized under the server name {ServerName}!");

            while (true)
            {
                string input = System.Console.ReadLine();

                if (input == null)
                    continue;

                taskScheduler
                    .ScheduleNextFrame(this, async () =>
                    {
                        if (input.StartsWith("/", StringComparison.InvariantCulture))
                            input = input.Remove(0, 1);

                        bool wasHandled = await commandHandler.HandleCommandAsync(Console, input, string.Empty);

                        if (!wasHandled)
                            logger.LogError("That command could not be found!");
                    }, "Console Command");
            }
        }

        /// <inheritdoc />
        public async Task ShutdownAsync()
        {
            taskScheduler
                .ScheduleNextFrame(this, async () =>
                {
                    logger.LogInformation("The shutdown sequence has been initiated.");

                    StorageManager.SaveAndFlush();

                    foreach (IPlugin plugin in pluginManager)
                        await plugin.DeactivateAsync();

                    foreach (EcoPlayer player in (await playerManager.GetPlayersAsync()).Cast<EcoPlayer>())
                        await playerManager.KickAsync(player.User, null, "The server is shutting down.");

                    //TODO: This appears to cause a StackOverflowException.
                    //runtime.Shutdown();

                    Thread.Sleep(2000);
                    Environment.Exit(0);
                }, "Shutdown");

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task ReloadAsync()
        {
            foreach (IPlugin plugin in pluginManager)
                if (await plugin.DeactivateAsync())
                    await plugin.ActivateAsync(true);
        }

        /// <inheritdoc />
        public Version HostVersion => GetType().Assembly.GetName().Version;

        private void CheckConfig()
        {
            //TODO: Add a IConfiguration.TryGetSection method to Rocket.API.
            try
            {
                permissionProvider.PlayersConfig.GetSection("EcoUser");
            }
            catch
            {
                logger.LogInformation(
                    "Detected first-time initialization for Permissions config, regenerating it now.");

                permissionProvider.PlayersConfig.CreateSection("EcoUser", SectionType.Array);
                permissionProvider.PlayersConfig.SaveAsync().GetAwaiter().GetResult();
            }
        }

        internal bool _EmitPlayerPreJoin(object user)
        {
            if (user == null || !(user is User castedUser))
                return false;

            EcoPlayer ecoPlayer = playerManager?.InternalPlayersList.FirstOrDefault(x
                => x.Id.Equals(castedUser.SlgId)
                || x.Id.Equals(castedUser.SteamId));

            if (ecoPlayer == null)
            {
                ecoPlayer = new EcoPlayer(castedUser, playerManager, runtime.Container);
                playerManager?.InternalPlayersList.Add(ecoPlayer);
            }
            else if (ecoPlayer.InternalEcoUser == null)
            {
                ecoPlayer.BuildReference(castedUser);
            }

            logger.LogDebug($"Emitting PlayerPreConnectEvent [{ecoPlayer.Id}]");
            PlayerPreConnectEvent e = new PlayerPreConnectEvent(ecoPlayer);
            eventManager.Emit(this, e);

            if (!e.IsCancelled) return true;

            logger.LogInformation($"[{ecoPlayer.Id}] {ecoPlayer.Name} was prevent from joining for the reason: "
                + (e.RejectionReason ?? "No reason was supplied."));
            ecoPlayer.InternalEcoPlayer.Client.Disconnect("You have been prevented from joining.",
                e.RejectionReason ?? "No reason was supplied.");
            return false;
        }

        internal void _EmitPlayerJoin(object user)
        {
            if (user == null || !(user is User castedUser))
                return;

            EcoPlayer ecoPlayer = playerManager?.InternalPlayersList.FirstOrDefault(x
                => x.Id.Equals(castedUser.SlgId)
                || x.Id.Equals(castedUser.SteamId));

            IConfigurationSection configurationSection = permissionProvider.PlayersConfig["EcoUser"];

            List<PlayerPermissionSection> playerPermissions =
                configurationSection.Get<PlayerPermissionSection[]>().ToList();

            if (playerPermissions.FirstOrDefault(x => x.Id.Equals(ecoPlayer.Id)) == null)
            {
                IEnumerable<GroupPermissionSection> autoGroups = permissionProvider
                                                                 .GroupsConfig["Groups"]
                                                                 .Get<GroupPermissionSection[]>()
                                                                 .Where(x => x.AutoAssign);

                PlayerPermissionSection section = new PlayerPermissionSection
                {
                    Id = ecoPlayer.Id,
                    Permissions = new string[0],
                    Groups = autoGroups.Select(x => x.Id).ToArray()
                };

                playerPermissions.Add(section);

                configurationSection.Set(playerPermissions);

                permissionProvider.PlayersConfig.SaveAsync().GetAwaiter().GetResult();
            }

            logger.LogDebug($"Emitting UserConnectedEvent [{ecoPlayer.Id}]");
            eventManager.Emit(this, new PlayerConnectedEvent(ecoPlayer, EventExecutionTargetContext.NextFrame));

            logger.LogInformation($"[{ecoPlayer.Id}] {ecoPlayer.Name} has joined.");
        }

        internal void _EmitPlayerLeave(object player)
        {
            if (player == null || !(player is User castedUser))
                return;

            EcoPlayer ecoPlayer =
                playerManager?.InternalPlayersList.FirstOrDefault(x => x.Id.Equals(castedUser.SteamId));

            if (ecoPlayer == null)
            {
                logger.LogWarning("An unknown player has left the game. Please report this to a Rocket.Eco developer!");
                return;
            }

            logger.LogDebug($"Emitting UserDisconnectedEvent [{ecoPlayer.Id}]");
            eventManager.Emit(this, new PlayerConnectedEvent(ecoPlayer, EventExecutionTargetContext.NextFrame));

            logger.LogInformation($"[{ecoPlayer.Id}] {ecoPlayer.Name} has left.");
        }

        internal bool _EmitPlayerChat(object user, string text)
        {
            if (user == null || !(user is User castedUser) || !castedUser.LoggedIn)
                return true;

            if (!playerManager.TryGetOnlinePlayerById(castedUser.SteamId, out IPlayer ecoPlayer))
            {
                logger.LogWarning("An unknown player has chatted. Please report this to a Rocket.Eco developer!");
                return false;
            }

            if (text.StartsWith("/", StringComparison.InvariantCulture))
            {
                logger.LogDebug($"Emitting PreCommandExecutionEvent [{ecoPlayer.User.Id}, {text}]");
                PreCommandExecutionEvent commandEvent = new PreCommandExecutionEvent(ecoPlayer.User, text.Remove(0, 1));
                eventManager.Emit(this, commandEvent);

                if (commandEvent.IsCancelled)
                {
                    playerManager.SendMessageAsync(null, ecoPlayer.User,
                        "Execution of your command has been cancelled!");

                    return true;
                }

                taskScheduler
                    .ScheduleNextFrame(this, () =>
                    {
                        bool wasHandled = true;

                        try
                        {
                            wasHandled = commandHandler
                                         .HandleCommandAsync(ecoPlayer.User, text.Remove(0, 1), string.Empty)
                                         .GetAwaiter()
                                         .GetResult();
                        }
                        catch (NotEnoughPermissionsException)
                        {
                            playerManager.SendMessageAsync(null, ecoPlayer.User,
                                "You do not have enough permission to execute this command!");
                        }
                        catch (Exception e)
                        {
                            logger.LogError(
                                $"{ecoPlayer.User.UserName} failed to execute the command `{text.Remove(0, 1).Split(' ')[0]}`!");
                            logger.LogError($"{e.Message}\n{e.StackTrace}");

                            playerManager.SendMessageAsync(null, ecoPlayer.User,
                                "A runtime error occurred while executing this command, please contact an administrator!");

                            return;
                        }

                        if (!wasHandled)
                            playerManager.SendMessageAsync(null, ecoPlayer.User, "That command could not be found!");
                    }, "Player Chat Event");

                return true;
            }

            logger.LogDebug($"Emitting UserChatEvent [{ecoPlayer.User.Id}, {text}]");

            PlayerChatEvent chatEvent = new PlayerChatEvent(ecoPlayer, text)
            {
                IsCancelled = false
            };

            eventManager.Emit(this, chatEvent);

            string commandCancelled = chatEvent.IsCancelled ? "[CANCELLED] " : "";
            logger.LogInformation($"{commandCancelled}[{ecoPlayer.User.Id}] {ecoPlayer.User.UserName}: {text}");

            return chatEvent.IsCancelled;
        }
    }
}