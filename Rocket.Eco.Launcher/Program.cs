using System;
using System.Linq;
using System.IO;
using System.Reflection;

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
                        if (string.Equals(interatedName.Name, assemblyName.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals(interatedName.CultureInfo.Name ?? "", assemblyName.CultureInfo.Name ?? "", StringComparison.InvariantCultureIgnoreCase))
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

            foreach (string file in Directory.GetFiles(Path.Combine(currentPath, "Rocket", "Binaries")).Where(x => x.EndsWith(".dll")))
            {
                try
                {
                    Assembly.LoadFile(file);
                }
                catch { }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= GatherRocketDependencies;

            Assembly.LoadFile(Path.Combine(currentPath, "EcoServer.exe"));
            
            Runtime.Bootstrap();

            if (args.Length == 0 || !args.Contains("-extract", StringComparer.InvariantCultureIgnoreCase))
            {
                StartServer(args);
            }
        }

        static void StartServer(string[] args) => GetEcoAssembly().GetType("Eco.Server.Startup").GetMethod("Start", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { args });
        static Assembly GetEcoAssembly() => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals("EcoServer", StringComparison.InvariantCultureIgnoreCase));
        static Assembly GatherRocketDependencies(object obj, ResolveEventArgs args) => Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "Rocket", "Binaries", args.Name.Remove(args.Name.IndexOf(",")) + ".dll"));
    }
}