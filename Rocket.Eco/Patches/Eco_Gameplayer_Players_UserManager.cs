using System;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Rocket.Eco.API;

namespace Rocket.Eco.Patches
{
    [Serializable]
    public class Eco_Gameplayer_Players_UserManager : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.Gameplay";

        public string TargetType => "Eco.Gameplayer.Players.UserManager";

        public void Patch(TypeDefinition definition)
        {
            MethodDefinition run = definition.Methods.First(x => x.Name.Equals("Run"));

            ILProcessor il = run.Body.GetILProcessor();

            Instruction[] writeLine =
            {
                il.Create(OpCodes.Ldstr, "A tick was called from UserManager.Run()."),
                il.Create(OpCodes.Call, run.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[]{ typeof(string) })))
            };

            int index = default(int);

            for (int i = 0; i < il.Body.Instructions.Count; i++)
            {
                if (il.Body.Instructions[i].OpCode == OpCodes.Ldarg_0)
                {
                    index = i;
                    break;
                }
            }

            for (int i = 0; i < writeLine.Length; i++)
            {
                il.InsertBefore(il.Body.Instructions[index + i], writeLine[i]);
            }
        }
    }
}
