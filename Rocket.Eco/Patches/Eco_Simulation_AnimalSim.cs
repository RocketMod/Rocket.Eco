using System;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Rocket.Eco.API;

namespace Rocket.Eco.Patches
{
    public class Eco_Simulation_AnimalSim : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.Simulation";
        public string TargetType => "Eco.Simulation.AnimalSim";

        public void Patch(TypeDefinition definition)
        {
            MethodDefinition spawn = definition.Methods.First(x => x.Name.Equals("SpawnAnimal"));

            ILProcessor il = spawn.Body.GetILProcessor();

            Instruction[] writeLine =
            {
                il.Create(OpCodes.Ldstr, "An animal has been spawned."),
                il.Create(OpCodes.Call, spawn.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[]{ typeof(string) })))
            };

            for (int i = 0; i < writeLine.Length; i++)
            {
                il.InsertBefore(spawn.Body.Instructions[i], writeLine[i]);
            }
        }
    }
}
