using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rocket.Eco.Patching.API;
using Rocket.Eco.Patching.Callbacks;

namespace Rocket.Eco.Patching.Patches
{
    /// <inheritdoc />
    /// <summary>
    ///     A patch to inject delegate calls into Eco's User class.
    /// </summary>
    public sealed class UserPatch : IAssemblyPatch
    {
        /// <inheritdoc />
        public string TargetAssembly => "Eco.Gameplay";

        /// <inheritdoc />
        public string TargetType => "Eco.Gameplay.Players.User";

        /// <inheritdoc />
        public void Patch(TypeDefinition definition)
        {
            FieldDefinition loginDelegate = new FieldDefinition("OnUserLogin", FieldAttributes.Public | FieldAttributes.Static, definition.Module.ImportReference(typeof(EcoUserActionDelegate)));
            FieldDefinition logoutDelegate = new FieldDefinition("OnUserLogout", FieldAttributes.Public | FieldAttributes.Static, definition.Module.ImportReference(typeof(EcoUserActionDelegate)));

            definition.Fields.Add(loginDelegate);
            definition.Fields.Add(logoutDelegate);

            MethodDefinition login = definition.Methods.First(x => x.Name.Equals("Login"));
            MethodDefinition logout = definition.Methods.First(x => x.Name.Equals("Logout"));

            PatchMethod(login, loginDelegate);
            PatchMethod(logout, logoutDelegate);
        }

        private static void PatchMethod(MethodDefinition definition, FieldReference delegateDefinition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] injection =
            {
                il.Create(OpCodes.Ldsfld, delegateDefinition),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Callvirt, definition.Module.ImportReference(typeof(EcoUserActionDelegate).GetMethod("Invoke")))
            };

            foreach (Instruction t in injection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);
        }
    }
}