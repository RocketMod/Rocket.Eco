using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rocket.Eco.API;

namespace Rocket.Eco.Patches
{
    public sealed class ChatManagerPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.Gameplay";

        public string TargetType => "Eco.Gameplay.Systems.Chat.ChatManager";

        public void Patch(TypeDefinition definition)
        {
            MethodDefinition processAsCommandMethod = definition.Methods.First(x => x.Name.Equals("ProcessAsCommand"));
            MethodDefinition sendChatMethod = definition.Methods.First(x => x.Name.Equals("SendChat") && x.Parameters.Count == 3);

            PatchProcessAsCommand(processAsCommandMethod);
            PatchSendChat(sendChatMethod);
        }

        private static void PatchProcessAsCommand(MethodDefinition definition)
        {
            ILProcessor il = definition.Body.GetILProcessor();
        }

        private static void PatchSendChat(MethodDefinition definition)
        {
            ILProcessor il = definition.Body.GetILProcessor();
        }
    }
}