using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Eco.Core.Utils;
using Eco.ModKit;
using Eco.Shared.Utils;
using Microsoft.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rocket.Patching.API;

namespace Rocket.Eco.Launcher.Patches
{
    public sealed class RuntimeCompilerPatch : IAssemblyPatch
    {
        public string TargetAssembly => "Eco.ModKit";
        public string TargetType => "Eco.ModKit.RuntimeCompiler";

        public void Patch(TypeDefinition definition)
        {
            PatchConstructor(definition.Methods.First(x => x.IsConstructor));
        }

        private static void PatchConstructor(MethodDefinition method)
        {
            ILProcessor il = method.Body.GetILProcessor();

            il.Body.Instructions.Clear();
            il.Body.Variables.Clear();

            il.Body.InitLocals = false;
            il.Body.ExceptionHandlers.Clear();

            il.Body.Variables.Add(new VariableDefinition(method.Module.ImportReference(typeof(CSharpCodeProvider))));
            il.Body.Variables.Add(new VariableDefinition(method.Module.ImportReference(typeof(CompilerResults))));

            Instruction[] instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Newobj,
                    method.Module.ImportReference(typeof(Dictionary<string, string>).GetConstructor(new Type[0]))),
                Instruction.Create(OpCodes.Dup),
                Instruction.Create(OpCodes.Ldstr, "CompilerVersion"),
                Instruction.Create(OpCodes.Ldstr, "v4.0"),
                Instruction.Create(OpCodes.Callvirt,
                    method.Module.ImportReference(
                        typeof(Dictionary<string, string>).GetMethod("Add",
                            new[] {typeof(string), typeof(string)}))),
                Instruction.Create(OpCodes.Newobj,
                    method.Module.ImportReference(
                        typeof(CSharpCodeProvider).GetConstructor(new[] {typeof(Dictionary<string, string>)}))),
                Instruction.Create(OpCodes.Stloc_0),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldloc_0),
                Instruction.Create(OpCodes.Stfld, method.DeclaringType.Fields.First(x => x.Name.Equals("provider"))),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldloc_0),
                Instruction.Create(OpCodes.Call,
                    method.Module.ImportReference(typeof(RuntimeCompilerPatch).GetMethod("CompileMods",
                        BindingFlags.Static | BindingFlags.Public))),
                Instruction.Create(OpCodes.Stloc_1),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldloc_1),
                Instruction.Create(OpCodes.Stfld,
                    method.DeclaringType.Fields.First(x => x.Name.Equals("compilerResults"))),
                Instruction.Create(OpCodes.Ldloc_1),
                Instruction.Create(OpCodes.Callvirt,
                    method.Module.ImportReference(typeof(CompilerResults).GetMethod("get_CompiledAssembly",
                        BindingFlags.Instance | BindingFlags.Public))),
                Instruction.Create(OpCodes.Stfld,
                    method.DeclaringType.Fields.First(x => x.Name.Equals("modkitAssembly"))),
                Instruction.Create(OpCodes.Ret)
            };

            foreach (Instruction instruction in instructions) il.Body.Instructions.Add(instruction);
        }

        public static CompilerResults CompileMods(object compiler, CSharpCodeProvider provider)
        {
            CompilerParameters parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                TempFiles = new TempFileCollection(Path.GetTempPath(), true),
                OutputAssembly = Path.Combine(Path.GetTempPath(), "Eco.Mods.dll"),
                IncludeDebugInformation = false
            };

            parameters.CompilerOptions += "/optimize";

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");

            string path = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco");

            // ToLowerInvariant() is because the files are extracted and written without case

            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Eco.Core.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Eco.Gameplay.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Eco.ModKit.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Eco.Shared.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Eco.Simulation.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Eco.World.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Eco.Stats.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Eco.WorldGenerator.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "LiteDB.dll".ToLowerInvariant()));
            parameters.ReferencedAssemblies.Add(Path.Combine(path, "Priority Queue.dll".ToLowerInvariant()));

            string[] files = Directory.GetFiles(ModKitPlugin.ModDirectory, "*.cs", SearchOption.AllDirectories);

            using (new TimedTask("Compiling mods (handled by RocketMod)".ToLocString()))
            {
                CompilerResults results = provider.CompileAssemblyFromFile(parameters, files);

                if (!results.Errors.HasErrors)
                {
                    compiler.GetType().GetProperty("HasError").SetValue(compiler, false);
                    return results;
                }

                StringBuilder stringBuilder = new StringBuilder();

                foreach (object obj in results.Errors)
                {
                    CompilerError compilerError = (CompilerError) obj;
                    stringBuilder.AppendLine(
                        $"Error in {Path.GetFileNameWithoutExtension(compilerError.FileName)} at {compilerError.Line}: {compilerError.ErrorText} ({compilerError.ErrorNumber})"
                    );
                }

                string error = stringBuilder.ToString();

                Log.WriteLine("Mods recompiled with errors.".ToLocString());
                Log.WriteLine(error.ToLocString());

                compiler.GetType().GetProperty("HasError").SetValue(compiler, true);
                compiler.GetType().GetProperty("LastError").SetValue(compiler, error);

                return results;
            }
        }
    }
}