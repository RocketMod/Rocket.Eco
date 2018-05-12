using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Rocket.Eco.Launcher
{
    internal static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += GatherRocketDependencies;

            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
            {
                try
                {
                    AssemblyName assemblyName = new AssemblyName(args.Name);
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                    return (from assembly in assemblies
                            let interatedName = assembly.GetName()
                            where string.Equals(interatedName.Name, assemblyName.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals(interatedName.CultureInfo?.Name ?? "", assemblyName.CultureInfo?.Name ?? "", StringComparison.InvariantCultureIgnoreCase)
                            select assembly).FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            };
        }

        public static void Main(string[] args)
        {
            string currentPath = Directory.GetCurrentDirectory();

            bool isExtraction = args.Length != 0 && args.Contains("-extract", StringComparer.InvariantCultureIgnoreCase);

            string path = Path.Combine(currentPath, "Rocket", "Binaries");
            string rocketEcoFile = Path.Combine(path, "Rocket.Eco.dll");

            Assembly.LoadFile(rocketEcoFile);

            foreach (string file in Directory.GetFiles(path).Where(x => x.EndsWith(".dll")))
                try
                {
                    if (file != rocketEcoFile)
                        Assembly.LoadFile(file);
                }
                catch { }

            AppDomain.CurrentDomain.AssemblyResolve -= GatherRocketDependencies;

            if (isExtraction)
            {
                Assembly.LoadFile(Path.Combine(currentPath, "EcoServer.exe"));
            }
            else
            {
                DefaultAssemblyResolver monoAssemblyResolver = new DefaultAssemblyResolver();
                monoAssemblyResolver.AddSearchDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco"));

                ReaderParameters reader = new ReaderParameters
                {
                    AssemblyResolver = monoAssemblyResolver
                };

                AssemblyDefinition ecoServer = AssemblyDefinition.ReadAssembly(Path.Combine(currentPath, "EcoServer.exe"), reader);

                //TODO: Make this WAY better! https://i.redd.it/atf1ietqwaxy.jpg
                try
                {
                    TypeDefinition startup = ecoServer.MainModule.GetType("Eco.Server.Startup");

                    PatchStartup(startup);

                    string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco", "EcoServer.exe");

                    WriterParameters writer = new WriterParameters();

                    using (FileStream file = new FileStream(outputDir, FileMode.Create))
                    {
                        ecoServer.Write(file, writer);
                    }

                    Assembly.LoadFile(outputDir);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetType().ToString());
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("Launching failed! (Maybe you didn't extract)");
                    return;
                }
                finally
                {
                    ecoServer.Dispose();
                }
            }

            Runtime.Bootstrap();
        }

        private static void PatchStartup(TypeDefinition definition)
        {
            ILProcessor il = definition.Methods.First(x => x.Name == "Start").Body.GetILProcessor();

            int index = default(int);

            for (int i = il.Body.Instructions.Count - 1; i != 0; i--)
                if (il.Body.Instructions[i].OpCode == OpCodes.Newobj)
                {
                    index = i + 1;
                    break;
                }

            List<Instruction> removedInstructions = new List<Instruction>();

            for (int i = index; i < il.Body.Instructions.Count; i++)
                removedInstructions.Add(il.Body.Instructions[i]);

            foreach (Instruction i in removedInstructions)
                il.Remove(i);

            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], il.Create(OpCodes.Pop));
            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], il.Create(OpCodes.Ret));

            il.Body.ExceptionHandlers.Clear();
            il.Body.Variables.Clear();

            il.Body.InitLocals = false;

            il.Body.Optimize();
            il.Body.OptimizeMacros();
        }

        private static Assembly GatherRocketDependencies(object obj, ResolveEventArgs args) => Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", args.Name.Remove(args.Name.IndexOf(",", StringComparison.InvariantCultureIgnoreCase)) + ".dll"));
    }
}