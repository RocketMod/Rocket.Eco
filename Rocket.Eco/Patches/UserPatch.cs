using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rocket.Eco.API;

namespace Rocket.Eco.Patches
{
    public sealed class UserPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.Gameplay";

        public string TargetType => "Eco.Gameplay.Players.User";

        public void Patch(TypeDefinition definition)
        {
            MethodDefinition login = definition.Methods.First(x => x.Name.Equals("Login"));
            MethodDefinition logout = definition.Methods.First(x => x.Name.Equals("Logout"));

            PatchLogin(login);
            PatchLogout(logout);
        }

        private static void PatchLogin(MethodDefinition definition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] injection =
            {
                //il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetProperty("Instance").GetGetMethod())),
                //il.Create(OpCodes.Ldarg_0),
                //il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetMethod("_EmitPlayerJoin", BindingFlags.Instance | BindingFlags.NonPublic)))
            };

            foreach (Instruction t in injection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);
        }

        private static void PatchLogout(MethodDefinition definition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] injection =
            {
                //il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetProperty("Instance").GetGetMethod())),
                //il.Create(OpCodes.Ldarg_0),
                //il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetMethod("_EmitPlayerLeave", BindingFlags.Instance | BindingFlags.NonPublic)))
            };

            foreach (Instruction t in injection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);
        }
    }
}