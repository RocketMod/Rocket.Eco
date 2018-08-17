using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rocket.Patching.API;

namespace Rocket.Eco.Launcher.Patches
{
    /// <inheritdoc />
    /// <summary>
    ///     A patch to inject delegate calls into Eco's ChatManager class.
    /// </summary>
    public sealed class ChatManagerPatch : IAssemblyPatch
    {
        /// <inheritdoc />
        public string TargetAssembly => "Eco.Gameplay";

        /// <inheritdoc />
        public string TargetType => "Eco.Gameplay.Systems.Chat.ChatManager";

        /// <inheritdoc />
        public void Patch(TypeDefinition definition)
        {
            FieldDefinition chatDelegate = new FieldDefinition("OnUserChat", FieldAttributes.Public | FieldAttributes.Static, definition.Module.ImportReference(typeof(Func<object, string, bool>)));

            definition.Fields.Add(chatDelegate);

            //Let's kill this method.
            definition.Methods.Remove(definition.Methods.First(x => x.Name.Equals("ProcessAsCommand")));

            MethodDefinition sendChatMethod = definition.Methods.First(x => x.Name.Equals("SendChat") && x.Parameters.Count == 3);

            PatchSendChat(sendChatMethod, chatDelegate);
        }

        private static void PatchSendChat(MethodDefinition definition, FieldReference delegateDefinition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            int index = default(int);

            for (int i = 0; i < il.Body.Instructions.Count; i++)
                if (il.Body.Instructions[i].OpCode == OpCodes.Ldarg_0)
                {
                    index = i;
                    break;
                }

            for (int i = 0; i < 5; i++)
                il.Remove(il.Body.Instructions[index]);

            Instruction[] injection =
            {
                il.Create(OpCodes.Ldsfld, delegateDefinition),
                il.Create(OpCodes.Ldarg_3),
                il.Create(OpCodes.Ldarg_2),
                il.Create(OpCodes.Callvirt, definition.Module.ImportReference(typeof(Func<object, string, bool>).GetMethod("Invoke"))),
                il.Create(OpCodes.Brfalse_S, il.Body.Instructions[index + 2])
            };

            for (int i = 0; i < injection.Length; i++)
            {
                Instruction t = injection[i];
                il.InsertBefore(il.Body.Instructions[index + i], t);
            }

            il.Body.Optimize();
        }
    }
}