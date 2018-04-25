using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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

            foreach (string file in Directory.GetFiles(Path.Combine(currentPath, "Rocket", "Binaries")).Where(x => x.EndsWith(".dll")))
                try
                {
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
                //TODO: Make this WAY better!
                catch (Exception e)
                {
                    Console.WriteLine(e.GetType().ToString());
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("Launching failed, you may not have extracted. Running the extraction now...");

                    Thread.Sleep(2000);

                    ProcessStartInfo info = new ProcessStartInfo
                    {
                        FileName = typeof(Program).Assembly.Location,
                        Arguments = "-extract"
                    };

                    Process.Start(info);

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
                    index = i + 2;
                    break;
                }

            List<Instruction> removedInstructions = new List<Instruction>();

            for (int i = index; i < il.Body.Instructions.Count; i++)
                removedInstructions.Add(il.Body.Instructions[i]);

            foreach (Instruction i in removedInstructions) il.Remove(i);

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