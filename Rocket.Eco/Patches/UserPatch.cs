using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rocket.Eco.API.Patching;
using Rocket.Eco.Delegates;

namespace Rocket.Eco.Patches
{
    public sealed class UserPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.Gameplay";

        public string TargetType => "Eco.Gameplay.Players.User";
        
        public void Patch(TypeDefinition definition)
        {
            FieldDefinition loginDelegate = new FieldDefinition("OnUserLogin", FieldAttributes.Public | FieldAttributes.Static, definition.Module.ImportReference(typeof(EcoUserActionDelegate)));
            FieldDefinition logoutDelegate = new FieldDefinition("OnUserLogout", FieldAttributes.Public | FieldAttributes.Static, definition.Module.ImportReference(typeof(EcoUserActionDelegate)));

            definition.Fields.Add(loginDelegate);
            definition.Fields.Add(logoutDelegate);

            MethodDefinition login = definition.Methods.First(x => x.Name.Equals("Login"));
            MethodDefinition logout = definition.Methods.First(x => x.Name.Equals("Logout"));

            PatchLogin(login, loginDelegate);
            PatchLogout(logout, logoutDelegate);
        }

        private static void PatchLogin(MethodDefinition definition, FieldReference delegateDefinition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] injection =
            {
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Ldfld, delegateDefinition),
                il.Create(OpCodes.Callvirt, definition.Module.ImportReference(typeof(EcoUserActionDelegate).GetMethod("Invoke")))
            };

            foreach (Instruction t in injection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);
        }

        private static void PatchLogout(MethodDefinition definition, FieldReference delegateDefinition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] injection =
            {
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Ldfld, delegateDefinition),
                il.Create(OpCodes.Callvirt, definition.Module.ImportReference(typeof(EcoUserActionDelegate).GetMethod("Invoke")))
            };

            foreach (Instruction t in injection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);
        }
    }
}