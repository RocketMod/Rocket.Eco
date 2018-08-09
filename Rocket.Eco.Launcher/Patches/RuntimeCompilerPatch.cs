using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rocket.Eco.Patching.API;

namespace Rocket.Eco.Launcher.Patches
{
    public sealed class RuntimeCompilerPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.ModKit";
        public string TargetType => "Eco.ModKit.RuntimeCompiler";

        public void Patch(TypeDefinition definition)
        {
            PatchConstructor(definition.Methods.First(x => x.IsConstructor));
            PatchLoadOrCompileMods(definition.Methods.First(x => x.Name.Equals("LoadOrCompileMods", StringComparison.InvariantCultureIgnoreCase)));
        }

        private static void PatchConstructor(MethodDefinition method)
        {
            ILProcessor il = method.Body.GetILProcessor();

            List<Instruction> removedInstructions = new List<Instruction>();

            for (int i = 0; i < removedInstructions.Count; i++)
            {
                removedInstructions.Add(il.Body.Instructions[i]);

                if (il.Body.Instructions[i].OpCode == OpCodes.Ldfld)
                    break;
            }

            foreach (Instruction instruction in removedInstructions)
            {
                il.Body.Instructions.Remove(instruction);
            }

            Instruction[] instructions = 
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, method.Module.ImportReference(typeof(object).GetConstructor(new Type[0]))),
                Instruction.Create(OpCodes.Call, method.Module.ImportReference(typeof(Directory).GetMethod("GetCurrentDirectory"))),
                Instruction.Create(OpCodes.Ldstr, "Rocket"),
                Instruction.Create(OpCodes.Ldstr, "Eco"),
                Instruction.Create(OpCodes.Ldstr, "Binaries"),
                Instruction.Create(OpCodes.Call, method.Module.ImportReference(typeof(Path).GetMethod("Combine", new[]{typeof(string), typeof(string), typeof(string), typeof(string)}))),
                Instruction.Create(OpCodes.Stfld, method.DeclaringType.Fields.First(x => x.Name.Equals("exeDir", StringComparison.InvariantCultureIgnoreCase))), 
            };
        }

        private static void PatchLoadOrCompileMods(MethodDefinition method)
        {

        }

        void test()
        {
            string exedir = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco");
        }
    }
}
