using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using MoreLinq.Extensions;
using Rocket.Eco.Launcher.Patches;
using Rocket.Eco.Launcher.Utils;
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

            FileStream stream = File.OpenRead("EcoServer.exe");

            AssemblyDefinition defn = AssemblyDefinition.ReadAssembly(stream);

            CosturaHelper.ExtractCosturaAssemblies(defn).ForEach(x => patchingService.RegisterAssembly(x));

            patchingService.RegisterPatch<UserPatch>();
            patchingService.RegisterPatch<ChatManagerPatch>();

            List<AssemblyDefinition> patches = patchingService.Patch().ToList();

            //patches.ForEach(x => Console.WriteLine(x.Name.Name));

            //This fixes only ONE of the errors.
            //AssemblyDefinition ecoShared = patches.First(x => x.Name.Name.Equals("Eco.Shared", StringComparison.InvariantCultureIgnoreCase));

            //patches.Remove(ecoShared);

            //LoadAssemblyFromDefinition(ecoShared);

            patches.ForEach(LoadAssemblyFromDefinition);

            CosturaHelper.DisposeStreams();

            patchingService.RegisterAssembly(defn);
            patchingService.RegisterPatch<StartupPatch>();

            patchingService.Patch().ForEach(LoadAssemblyFromDefinition);

            stream.Dispose();

#if DEBUG
            AppDomain.CurrentDomain.GetAssemblies().ForEach(x => Console.WriteLine(x.FullName));
#endif

            AppDomain.CurrentDomain.AssemblyResolve -= GatherRocketDependencies;
            /*
            AppDomain.CurrentDomain.GetAssemblies()
                     .First(x => x.GetName().Name.Equals("EcoServer"))
                     .GetType("Eco.Server.Startup")
                     .GetMethod("Start", BindingFlags.Static | BindingFlags.Public)
                     .Invoke(null, new object[]
                         {args.Where(x => !x.Equals("-extract", StringComparison.InvariantCultureIgnoreCase)).ToArray()});

    */
            Console.WriteLine("Houston, we have control!");

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