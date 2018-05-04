using System;
using System.Linq;
using System.Reflection;
using Eco.Gameplay.Systems.Chat;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.Eco.API;
using Rocket.Eco.Extensions;
using Rocket.Eco.Player;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandWrapper : ContainerAccessor, ICommand
    {
        private static ChatManager ecoChatManager;
        private static MethodInfo execute;

        private readonly ChatCommandAttribute command;
        private readonly MethodInfo commandMethod;

        internal EcoCommandWrapper(MethodInfo method, IDependencyContainer container) : base(container)
        {
            if (ecoChatManager == null)
                ecoChatManager = (ChatManager) typeof(ChatServer).GetField("netChatManager", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(ChatServer.Obj)
                    ?? throw new Exception("A critical part of the Eco codebase has been changed; please uninstall Rocket until it is updated to support these changes.");

            if (execute == null)
                execute = typeof(ChatManager).GetMethod("InvokeCommand", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new Exception("A critical part of the Eco codebase has been changed; please uninstall Rocket until it is updated to support these changes.");

            command = (ChatCommandAttribute) method.GetCustomAttributes().FirstOrDefault(x => x is ChatCommandAttribute);

            if (command != null)
                commandMethod = method;
            else
                Container.Resolve<ILogger>().LogError("An attempt was made to register a vanilla command with inproper attributes!");
        }

        public string[] Aliases => new string[0];
        public ISubCommand[] ChildCommands => new ISubCommand[0];

        public string Name => command.CommandName;
        public string Permission => $"Eco.Base.{Name}";
        public string Description => command.HelpText;

        //TODO: Make this match the parameter list of `commandMethod`
        public string Syntax => string.Empty;

        public bool SupportsCaller(Type type) => type == typeof(OnlineEcoPlayer);

        public void Execute(ICommandContext context)
        {
            string args = string.Join(",", context.Parameters);

            try
            {
                execute.Invoke(ecoChatManager, new object[] {Name, commandMethod, args, ((OnlineEcoPlayer) context.Caller).User});
            }
            catch (Exception e)
            {
                Container.ResolveLogger().LogError($"{context.Caller.Name} failed to execute the vanilla command `{Name}`!", e);
            }
        }
    }
}