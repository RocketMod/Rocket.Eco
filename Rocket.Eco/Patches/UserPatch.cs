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
            FieldDefinition loginDelegate = new FieldDefinition("OnUserLogin", FieldAttributes.Public, definition.Module.ImportReference(typeof(EcoUserActionDelegate)));
            FieldDefinition logoutDelegate = new FieldDefinition("OnUserLogout", FieldAttributes.Public, definition.Module.ImportReference(typeof(EcoUserActionDelegate)));

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
                il.Create(OpCodes.Ldfld, delegateDefinition),
                il.Create(OpCodes.Ldc_I4_1),
                il.Create(OpCodes.Newarr, definition.Module.ImportReference(typeof(object))),
                il.Create(OpCodes.Dup),
                il.Create(OpCodes.Ldc_I4_0),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Stelem_I4),
                il.Create(OpCodes.Callvirt, definition.Module.ImportReference(typeof(Action).GetMethod("DynamicInvoke")))
            };

            foreach (Instruction t in injection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);
        }

        private static void PatchLogout(MethodDefinition definition, FieldReference delegateDefinition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] injection =
            {
                il.Create(OpCodes.Ldfld, delegateDefinition),
                il.Create(OpCodes.Ldc_I4_1),
                il.Create(OpCodes.Newarr, definition.Module.ImportReference(typeof(object))),
                il.Create(OpCodes.Dup),
                il.Create(OpCodes.Ldc_I4_0),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Stelem_I4),
                il.Create(OpCodes.Callvirt, definition.Module.ImportReference(typeof(Action).GetMethod("DynamicInvoke")))
            };

            foreach (Instruction t in injection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);
        }
    }
}