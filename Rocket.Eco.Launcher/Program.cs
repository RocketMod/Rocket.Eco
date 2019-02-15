using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using MoreLinq;
using Rocket.Eco.Launcher.Patches;
using Rocket.Eco.Launcher.Utils;
using Rocket.Launcher.Patches;
using Rocket.Patching;
using Rocket.Patching.API;

namespace Rocket.Eco.Launcher
{
    internal static class Program
    {
        static IEnumerable<AssemblyDefinition> _assemblies;

        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += GatherRocketDependencies;

            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
            {
                AssemblyDefinition defn = _assemblies?.FirstOrDefault(x => x.FullName == args.Name);

                if (defn != null) return LoadAssemblyFromDefinition(defn);

                AssemblyName assemblyName = new AssemblyName(args.Name);
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                try
                {
                    return (from assembly in assemblies
                            let iteratedName = assembly.GetName()
                            where string.Equals(iteratedName.Name, assemblyName.Name,
                                    StringComparison.InvariantCultureIgnoreCase)
                                && string.Equals(iteratedName.CultureInfo?.Name ?? "",
                                    assemblyName.CultureInfo?.Name ?? "",
                                    StringComparison.InvariantCultureIgnoreCase)
                            select assembly).FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            };
        }

        private static Assembly GatherRocketDependencies(object obj, ResolveEventArgs args)
        {
            if (args.Name.Contains("System.Net.Http"))
            {
                return null;
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries",
                args.Name.Remove(args.Name.IndexOf(",", StringComparison.InvariantCultureIgnoreCase)) + ".dll");

            return !File.Exists(path) ? null : Assembly.LoadFile(path);
        }

        public static void Main(string[] args)
        {
            IPatchingService patchingService = new PatchingService();

            FileStream stream = File.OpenRead("EcoServer.exe");

            AssemblyDefinition defn = AssemblyDefinition.ReadAssembly(stream);

            CosturaHelper.ExtractCosturaAssemblies(defn).ForEach(x => patchingService.RegisterAssembly(x));

            patchingService.RegisterPatch<UserPatch>();
            patchingService.RegisterPatch<ChatManagerPatch>();
            patchingService.RegisterPatch<RuntimeCompilerPatch>();

            _assemblies = patchingService.Patch().ToList();

            patchingService.RegisterAssembly(defn);
            patchingService.RegisterPatch<StartupPatch>();
            
            List<string> newArgs = args.ToList();

            LoadAssemblyFromDefinition(patchingService.Patch().First());

            newArgs.Add("-nogui");
            
            AppDomain.CurrentDomain.GetAssemblies()
                     .First(x => x.GetName().Name.Equals("EcoServer"))
                     .GetType("Eco.Server.Startup")
                     .GetMethod("Start", BindingFlags.Static | BindingFlags.Public)
                     .Invoke(null, new object[]
                             {newArgs.ToArray()});
            
            new Runtime().BootstrapAsync().GetAwaiter().GetResult();
            
            CosturaHelper.DisposeStreams();
            stream.Dispose();
        }

        private static Assembly LoadAssemblyFromDefinition(AssemblyDefinition definition)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                definition.Write(stream);

                stream.Position = 0; //Is this needed?

                byte[] buffer = new byte[stream.Length];

                stream.Read(buffer, 0, buffer.Length);

                return Assembly.Load(buffer);
            }
        }
    }
}