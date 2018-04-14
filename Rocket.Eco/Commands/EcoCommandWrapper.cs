using System;
using System.Linq;
using System.Reflection;
using Eco.Gameplay.Systems.Chat;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Logging;
using Rocket.Eco.Player;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandWrapper : ICommand
    {
        private static ChatManager ecoChatManager;
        private static MethodInfo execute;

        private readonly ChatCommandAttribute command;
        private readonly MethodInfo commandMethod;
        private readonly IRuntime runtime;

        internal EcoCommandWrapper(MethodInfo method, IRuntime runtime)
        {
            if (ecoChatManager == null)
                ecoChatManager = (ChatManager) typeof(ChatServer).GetField("netChatManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ChatServer.Obj);

            if (execute == null)
                execute = typeof(ChatManager).GetMethod("InvokeCommand", BindingFlags.Instance | BindingFlags.NonPublic);

            command = (ChatCommandAttribute) method.GetCustomAttributes().FirstOrDefault(x => x is ChatCommandAttribute);
            this.runtime = runtime;

            if (command != null)
                commandMethod = method;
            else
                runtime.Container.Get<ILogger>().LogError("An attempt was made to register a vanilla command with inproper attributes!");
        }

        public string Name => command.CommandName;
        public string Permission => $"Eco.Base.{Name}";

        public void Execute(ICommandContext context)
        {
            if (context.Caller is EcoPlayer p)
            {
                string args = string.Join(",", context.Parameters);

                try
                {
                    execute.Invoke(ecoChatManager, new object[] {Name, commandMethod, args, p.User});
                }
                catch (Exception e)
                {
                    runtime.Container.Get<ILogger>().LogError($"{p.Name} failed to execute the vanilla command `{Name}`!", e);
                }
            }
            else
            {
                context.Caller.SendMessage("Only an in-game user may call vanilla Eco commands!");
            }
        }
    }
}