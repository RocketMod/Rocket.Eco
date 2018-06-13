using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ObjectBuilder2;
using Mono.Cecil;
using Rocket.Eco.Launcher.Patches;
using Rocket.Eco.Patching;
using Rocket.Eco.Patching.API;

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

        private static Assembly GatherRocketDependencies(object obj, ResolveEventArgs args) => Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", args.Name.Remove(args.Name.IndexOf(",", StringComparison.InvariantCultureIgnoreCase)) + ".dll"));

        public static void Main(string[] args)
        {
            IPatchingService patchingService = new PatchingService();

            AssemblyDefinition ecoServer = AssemblyDefinition.ReadAssembly("EcoServer.exe");

            CosturaHelper.ExtractCosturaAssemblies(ecoServer).ForEach(x => patchingService.RegisterAssembly(x));

            patchingService.RegisterPatch<UserPatch>();
            patchingService.RegisterPatch<ChatManagerPatch>();

            patchingService.Patch().ForEach(LoadAssemblyFromDefinition);

            patchingService.RegisterAssembly(ecoServer);
            patchingService.RegisterPatch<StartupPatch>();

            patchingService.Patch().ForEach(LoadAssemblyFromDefinition);

            Console.ReadLine();
        }

        private static void LoadAssemblyFromDefinition(AssemblyDefinition definition)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                definition.Write(stream);
                stream.Position = 0; //Is this needed?

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                Assembly.Load(buffer);
            }
        }

        /*
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
                catch { } //Assembly already loaded.

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

                    //PatchStartup(startup);

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
        */
    }
}