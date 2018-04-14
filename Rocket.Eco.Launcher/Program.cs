using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;

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

                AssemblyDefinition ecoServer = AssemblyDefinition.ReadAssembly(Path.Combine(currentPath, "EcoServer.exe"), new ReaderParameters
                {
                    AssemblyResolver = monoAssemblyResolver
                });

                try
                {
                    TypeDefinition pluginManager = ecoServer.MainModule.GetType("Eco.Server.PluginManager");

                    PatchPluginManagerConstructor(pluginManager);

                    string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco", "EcoServer.exe");

                    ecoServer.Write(outputDir);

                    Assembly.LoadFile(outputDir);
                }
                catch
                {
                    Console.WriteLine("Launching failed, you may not have extracted. Running the extraction now...");

                    Thread.Sleep(2000);

                    ProcessStartInfo info = new ProcessStartInfo
                    {
                        FileName = typeof(Program).Assembly.Location,
                        Arguments = "-extract",
                        CreateNoWindow = false
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

        private static void PatchPluginManagerConstructor(TypeDefinition definition)
        {
            ILProcessor il = definition.Methods.First(x => x.Name == ".ctor").Body.GetILProcessor();

            //TODO
            Instruction[] inject =
            {
                //il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetProperty("Instance").GetGetMethod())),
                //il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetMethod("_EmitEcoInit", BindingFlags.Instance | BindingFlags.NonPublic)))
            };

            foreach (Instruction t in inject)
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], t);
        }

        private static Assembly GatherRocketDependencies(object obj, ResolveEventArgs args) => Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", args.Name.Remove(args.Name.IndexOf(",")) + ".dll"));
    }
}