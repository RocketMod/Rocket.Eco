using System;
using System.Linq;
using System.IO;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Rocket.Eco.Launcher
{
    static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += GatherRocketDependencies;

            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
            {
                try
                {
                    AssemblyName assemblyName = new AssemblyName(args.Name);
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                    foreach (Assembly assembly in assemblies)
                    {
                        AssemblyName interatedName = assembly.GetName();
                        if (string.Equals(interatedName.Name, assemblyName.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals((interatedName.CultureInfo == null) ? "" : interatedName.CultureInfo.Name , (assemblyName.CultureInfo == null) ? "" : assemblyName.CultureInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return assembly;
                        }
                    }

                    return null;
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

            bool isExtraction = (args.Length != 0 && args.Contains("-extract", StringComparer.InvariantCultureIgnoreCase));

            foreach (string file in Directory.GetFiles(Path.Combine(currentPath, "Rocket", "Binaries")).Where(x => x.EndsWith(".dll")))
            {
                try
                {
                    Assembly.LoadFile(file);
                }
                catch { }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= GatherRocketDependencies;

            if (isExtraction)
            {
                Assembly.LoadFile(Path.Combine(currentPath, "EcoServer.exe"));
            }
            else
            {
                try
                {
                    var monoAssemblyResolver = new DefaultAssemblyResolver();
                    monoAssemblyResolver.AddSearchDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco"));

                    AssemblyDefinition ecoServer = AssemblyDefinition.ReadAssembly(Path.Combine(currentPath, "EcoServer.exe"), new ReaderParameters { AssemblyResolver = monoAssemblyResolver });
                    TypeDefinition typeDefinition = ecoServer.MainModule.GetType("Eco.Server.PluginManager");

                    ILProcessor il = typeDefinition.Methods.First(x => x.Name == ".ctor").Body.GetILProcessor();

                    Instruction[] inject = new Instruction[]
                    {
                        il.Create(OpCodes.Call, typeDefinition.Module.ImportReference(typeof(Eco).GetProperty("Instance").GetGetMethod())),
                        il.Create(OpCodes.Call, typeDefinition.Module.ImportReference(typeof(Eco).GetMethod("_EmitEcoInit", BindingFlags.Instance | BindingFlags.NonPublic)))
                    };

                    for (int i = 0; i < inject.Length; i++)
                    {
                        il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], inject[i]);
                    }

                    string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco", "EcoServer.exe");

                    ecoServer.Write(outputDir);

                    ecoServer.Dispose();

                    Assembly.LoadFile(outputDir);
                }
                catch
                {
                    Console.WriteLine("Please run `Rocket.Eco.Launcher.exe` at least once with the argument `-extract`!");
                    throw;
                }
            }

            Runtime.Bootstrap();

            if (!isExtraction)
            {
                StartServer(args);
            }
            else
            {
                Console.WriteLine("Extraction Finished.");
            }
        }

        static void StartServer(string[] args) => GetEcoAssembly().GetType("Eco.Server.Startup").GetMethod("Start", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { args });
        static Assembly GetEcoAssembly() => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals("EcoServer", StringComparison.InvariantCultureIgnoreCase));
        static Assembly GatherRocketDependencies(object obj, ResolveEventArgs args) => Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", args.Name.Remove(args.Name.IndexOf(",")) + ".dll"));
    }
}