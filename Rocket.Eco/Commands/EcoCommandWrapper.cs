using System;
using System.Linq;
using System.Reflection;

using Eco.Gameplay.Systems.Chat;

using Rocket.API.Commands;
using Rocket.API.Logging;
using Rocket.Eco.Player;

namespace Rocket.Eco.Commands
{
    public sealed class EcoCommandWrapper : ICommand
    {
        public string Name => command.CommandName;
        public string Permission => $"Eco.Base.{Name}";

        readonly ChatCommandAttribute command;
        readonly MethodInfo commandMethod;

        static ChatManager ecoChatManager;
        static MethodInfo execute;

        internal EcoCommandWrapper(MethodInfo method)
        {
            if (ecoChatManager == null)
            {
                ecoChatManager = (ChatManager)typeof(ChatServer).GetField("netChatManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ChatServer.Obj);
            }

            if (execute == null)
            {
                execute = typeof(ChatManager).GetMethod("InvokeCommand", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            command = (ChatCommandAttribute)method.GetCustomAttributes().FirstOrDefault(x => x is ChatCommandAttribute);

            if (command != null)
            {
                commandMethod = method;
            }
            else
            {
                Eco.runtime.Container.Get<ILogger>().LogError("An attempt was made to register a vanilla command with inproper attributes!");
            }
        }

        public void Execute(ICommandContext context)
        {
            if (context.Caller is EcoPlayer p)
            {
                string args = string.Join(",", context.Parameters);

                try
                {
                    execute.Invoke(ecoChatManager, new object[] { Name, commandMethod, args, p.User });
                }
                catch
                {
                    Eco.runtime.Container.Get<ILogger>().LogError($"{p.Name} failed to execute the command `{Name}`!");
                }
            }
            else
            {
                context.Caller.SendMessage("Only an in-game user may call vanilla Eco commands!");
            }
        }
    }
}
