using System;
using System.Linq;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Rocket.Eco.API;
using Rocket.Eco.Eventing;

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

        void PatchLogin(MethodDefinition definition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] injection = new Instruction[]
            {
                il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetProperty("Instance").GetGetMethod())),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetMethod("_EmitPlayerJoin", BindingFlags.Instance | BindingFlags.NonPublic)))
            };

            for (int i = 0; i < injection.Length; i++)
            {
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], injection[i]);
            }
        }

        void PatchLogout(MethodDefinition definition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] injection = new Instruction[]
            {
                il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetProperty("Instance").GetGetMethod())),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetMethod("_EmitPlayerLeave", BindingFlags.Instance | BindingFlags.NonPublic)))
            };

            for (int i = 0; i < injection.Length; i++)
            {
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], injection[i]);
            }
        }
    }
}
