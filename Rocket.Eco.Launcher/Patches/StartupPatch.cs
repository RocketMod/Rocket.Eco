using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rocket.Patching.API;

namespace Rocket.Launcher.Patches
{
    /// <inheritdoc />
    /// <summary>
    ///     A patch to inject delegate calls into Eco's Startup class.
    /// </summary>
    public sealed class StartupPatch : IAssemblyPatch
    {
        /// <inheritdoc />
        public string TargetAssembly => "EcoServer";

        /// <inheritdoc />
        public string TargetType => "Eco.Server.Startup";

        /// <inheritdoc />
        public void Patch(TypeDefinition definition)
        {
            ILProcessor il = definition.Methods.First(x => x.Name == "Start").Body.GetILProcessor();

            int index = default(int);

            for (int i = il.Body.Instructions.Count - 1; i != 0; i--)
                if (il.Body.Instructions[i].OpCode == OpCodes.Leave)
                {
                    index = i;
                    break;
                }

            List<Instruction> removedInstructions = new List<Instruction>();

            for (int i = index; i < il.Body.Instructions.Count; i++)
                removedInstructions.Add(il.Body.Instructions[i]);

            foreach (Instruction i in removedInstructions)
                il.Remove(i);

            //il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], il.Create(OpCodes.Pop));
            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], il.Create(OpCodes.Ret));

            il.Body.ExceptionHandlers.Clear();

            VariableDefinition variable1 = il.Body.Variables[0];
            VariableDefinition variable2 = il.Body.Variables[1];
            VariableDefinition variable3 = il.Body.Variables[2];

            il.Body.Variables.Clear();

            il.Body.Variables.Add(variable1);
            il.Body.Variables.Add(variable2);
            il.Body.Variables.Add(variable3);

            il.Body.InitLocals = false;

            il.Body.Optimize();
        }
    }
}