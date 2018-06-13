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

            AssemblyDefinition ecoServerDefinition = AssemblyDefinition.ReadAssembly("EcoServer.exe");

            CosturaHelper.ExtractCosturaAssemblies(ecoServerDefinition).ForEach(x => patchingService.RegisterAssembly(x));

            patchingService.RegisterPatch<UserPatch>();
            patchingService.RegisterPatch<ChatManagerPatch>();

            patchingService.Patch().ForEach(LoadAssemblyFromDefinition);

            patchingService.RegisterAssembly(ecoServerDefinition);
            patchingService.RegisterPatch<StartupPatch>();

            patchingService.Patch().ForEach(LoadAssemblyFromDefinition);

            Assembly ecoServer = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "EcoServer");


            //Runtime.Bootstrap();
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
    }
}