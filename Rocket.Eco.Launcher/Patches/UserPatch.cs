using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rocket.Patching.API;

namespace Rocket.Eco.Launcher.Patches
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
            FieldDefinition preLoginDelegate = new FieldDefinition("OnUserPreLogin",
                FieldAttributes.Public | FieldAttributes.Static,
                definition.Module.ImportReference(typeof(Func<object, bool>)));
            FieldDefinition loginDelegate = new FieldDefinition("OnUserLogin",
                FieldAttributes.Public | FieldAttributes.Static,
                definition.Module.ImportReference(typeof(Action<object>)));
            FieldDefinition logoutDelegate = new FieldDefinition("OnUserLogout",
                FieldAttributes.Public | FieldAttributes.Static,
                definition.Module.ImportReference(typeof(Action<object>)));

            definition.Fields.Add(preLoginDelegate);
            definition.Fields.Add(loginDelegate);
            definition.Fields.Add(logoutDelegate);

            MethodDefinition login = definition.Methods.First(x => x.Name.Equals("Login"));
            MethodDefinition logout = definition.Methods.First(x => x.Name.Equals("Logout"));

            PatchLogin(login, loginDelegate, preLoginDelegate);
            PatchLogout(logout, logoutDelegate);
        }

        private static void PatchLogin(MethodDefinition definition, FieldReference delegateDefinition,
                                       FieldReference preDelegateDefinition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] startInjection =
            {
                il.Create(OpCodes.Ldsfld, preDelegateDefinition),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Callvirt,
                    definition.Module.ImportReference(typeof(Func<object, bool>).GetMethod("Invoke"))),
                il.Create(OpCodes.Brtrue_S, il.Body.Instructions[0]),
                il.Create(OpCodes.Ret)
            };

            Instruction[] endInjection =
            {
                il.Create(OpCodes.Ldsfld, delegateDefinition),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Callvirt,
                    definition.Module.ImportReference(typeof(Action<object>).GetMethod("Invoke")))
            };

            for (int i = 0; i < startInjection.Length; i++)
                if (i == 0)
                    il.InsertBefore(il.Body.Instructions[0], startInjection[i]);
                else
                    il.InsertAfter(il.Body.Instructions[i - 1], startInjection[i]);

            foreach (Instruction t in endInjection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);

            il.Body.Optimize();
        }

        private static void PatchLogout(MethodDefinition definition, FieldReference delegateDefinition)
        {
            ILProcessor il = definition.Body.GetILProcessor();

            Instruction[] endInjection =
            {
                il.Create(OpCodes.Ldsfld, delegateDefinition),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Callvirt,
                    definition.Module.ImportReference(typeof(Action<object>).GetMethod("Invoke")))
            };

            foreach (Instruction t in endInjection)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);

            il.Body.Optimize();
        }
    }
}