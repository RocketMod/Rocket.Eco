using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

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
                        if (string.Equals(interatedName.Name, assemblyName.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals((interatedName.CultureInfo == null) ? "" : interatedName.CultureInfo.Name, (assemblyName.CultureInfo == null) ? "" : assemblyName.CultureInfo.Name, StringComparison.InvariantCultureIgnoreCase))
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
                var monoAssemblyResolver = new DefaultAssemblyResolver();
                monoAssemblyResolver.AddSearchDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco"));

                AssemblyDefinition ecoServer = AssemblyDefinition.ReadAssembly(Path.Combine(currentPath, "EcoServer.exe"), new ReaderParameters { AssemblyResolver = monoAssemblyResolver });

                try
                {
                    TypeDefinition pluginManager = ecoServer.MainModule.GetType("Eco.Server.PluginManager");
                    TypeDefinition startup = ecoServer.MainModule.GetType("Eco.Server.Startup");

                    PatchPluginManagerConstructor(pluginManager);
                    PatchStartup(startup);

                    string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "Eco", "EcoServer.exe");

                    ecoServer.Write(outputDir);

                    Assembly.LoadFile(outputDir);
                }
                catch
                {
                    Console.WriteLine("Launching failed, you may not have extracted. Running the extraction now...");

                    Thread.Sleep(2000);

                    var info = new ProcessStartInfo
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

            Eco.isExtraction = isExtraction;

            var listArgs = args.ToList();
            listArgs.RemoveAll(x => x.Equals("-extract", StringComparison.InvariantCultureIgnoreCase));

            Eco.launchArgs = listArgs.ToArray();

            Runtime.Bootstrap();
        }

        static void PatchPluginManagerConstructor(TypeDefinition definition)
        {
            ILProcessor il = definition.Methods.First(x => x.Name == ".ctor").Body.GetILProcessor();

            Instruction[] inject = new Instruction[]
            {
                il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetProperty("Instance").GetGetMethod())),
                il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetMethod("_EmitEcoInit", BindingFlags.Instance | BindingFlags.NonPublic)))
            };

            for (int i = 0; i < inject.Length; i++)
            {
                il.InsertBefore(il.Body.Instructions[il.Body.Instructions.Count - 1], inject[i]);
            }
        }

        static void PatchStartup(TypeDefinition definition)
        {
            ILProcessor il = definition.Methods.First(x => x.Name == "Start").Body.GetILProcessor();

            Instruction[] inject = new Instruction[]
            {
                il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetProperty("Instance").GetGetMethod())),
                il.Create(OpCodes.Call, definition.Module.ImportReference(typeof(Eco).GetMethod("_AwaitInput", BindingFlags.Instance | BindingFlags.NonPublic))),
                il.Create(OpCodes.Ret)
            };

            int index = default(int);

            for (int i = il.Body.Instructions.Count - 1; i != 0; i--)
            {
                if (il.Body.Instructions[i].OpCode == OpCodes.Newobj)
                {
                    index = i;
                    break;
                }
            }

            for (int i = index; i < il.Body.Instructions.Count; i++)
            {
                il.Remove(il.Body.Instructions[i]);
            }

            for (int i = 0; i < inject.Length; i++)
            {
                il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], inject[i]);
            }
        }

        static Assembly GatherRocketDependencies(object obj, ResolveEventArgs args) => Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", args.Name.Remove(args.Name.IndexOf(",")) + ".dll"));
    }
}