using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.CompilerServices.SymbolWriter;
using MoreLinq.Extensions;
using Rocket.Eco.Launcher.Patches;
using Rocket.Eco.Launcher.Utils;
using Rocket.Launcher.Patches;
using Rocket.Patching;
using Rocket.Patching.API;

namespace Rocket.Eco.Launcher
{
    internal static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += GatherRocketDependencies;

            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
            {
                AssemblyName assemblyName = new AssemblyName(args.Name);
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                
                /*
                if (assemblyName.Name == "System.Net.Http")
                {
                    Console.WriteLine("some dumbass requested this assembly");
                    return Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", "System.Net.Http.dll"));
                }
                */

                try
                {
                    var asm = (from assembly in assemblies
                               let interatedName = assembly.GetName()
                               where string.Equals(interatedName.Name, assemblyName.Name,
                                       StringComparison.InvariantCultureIgnoreCase)
                                   && string.Equals(interatedName.CultureInfo?.Name ?? "",
                                       assemblyName.CultureInfo?.Name ?? "",
                                       StringComparison.InvariantCultureIgnoreCase)
                               select assembly).FirstOrDefault();

                    if (asm != null)
                    {
                        return asm;
                    }

                    if ((assemblyName.Flags & AssemblyNameFlags.Retargetable) != AssemblyNameFlags.None)
                    {
                        return Assembly.Load(assemblyName);
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            };
        }

        private static Assembly GatherRocketDependencies(object obj, ResolveEventArgs args)
        {
            //Console.WriteLine(args.Name);
            return Assembly.LoadFile(
                Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries",
                    args.Name.Remove(args.Name.IndexOf(",", StringComparison.InvariantCultureIgnoreCase)) + ".dll"));
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

            List<AssemblyDefinition> patches = patchingService.Patch().ToList();

            patches.ForEach(LoadAssemblyFromDefinition);

            CosturaHelper.DisposeStreams();

            patchingService.RegisterAssembly(defn);
            patchingService.RegisterPatch<StartupPatch>();

            patchingService.Patch().ForEach(LoadAssemblyFromDefinition);

            stream.Dispose();

            AppDomain.CurrentDomain.AssemblyResolve -= GatherRocketDependencies;

            List<string> newArgs = args.ToList();
            newArgs.Add("-nogui");
            try
            {
                AppDomain.CurrentDomain.GetAssemblies()
                         .First(x => x.GetName().Name.Equals("EcoServer"))
                         .GetType("Eco.Server.Startup")
                         .GetMethod("Start", BindingFlags.Static | BindingFlags.Public)
                         .Invoke(null, new object[]
                             {newArgs.ToArray()});

                //foreach (string file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Rocket",
                //   "Binaries"))) Assembly.LoadFile(file);
            }
            catch(Exception e)
            {
                var aggregated = e.InnerException as AggregateException;

                foreach (var innerAggregated in aggregated.InnerExceptions)
                {
                    Console.WriteLine(innerAggregated);
                }
                //Console.WriteLine(e);
            }

            new Runtime().BootstrapAsync().GetAwaiter().GetResult();
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