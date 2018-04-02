using System;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Rocket.Eco.API;

namespace Rocket.Eco.Patches
{
    public sealed class PlayerPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.Gameplay";

        public string TargetType => "Eco.Gameplay.Players.Player";

        public void Patch(TypeDefinition definition)
        {
            MethodDefinition run = definition.Methods.First(x => x.Name.Equals(".ctor"));

            ILProcessor il = run.Body.GetILProcessor();

            Instruction[] writeLine =
            {
                il.Create(OpCodes.Ldstr, "A player has logged in!"),
                il.Create(OpCodes.Call, run.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[]{ typeof(string) })))
            };
            
            for (int i = 0; i < writeLine.Length; i++)
            {
                il.InsertBefore(il.Body.Instructions[i], writeLine[i]);
            }
        }
    }
}
