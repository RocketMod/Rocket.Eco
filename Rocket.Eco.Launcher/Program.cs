using System;
using System.Linq;
using System.IO;
using System.Reflection;

namespace Rocket.Eco.Launcher
{
    static class Program
    {
        public static void Main(string[] args)
        {
            string currentPath = Directory.GetCurrentDirectory();

            Assembly.LoadFile(Path.Combine(currentPath, "Rocket.Eco.dll"));
            Assembly.LoadFile(Path.Combine(currentPath, "EcoServer.exe"));

            AttachAssemblies();
            R.Bootstrap();
            StartServer(args);
        }

        //TODO: Remove this method as to administer the patches once I have the patcher finished.
        static void AttachAssemblies() => GetEcoAssembly().GetType("Costura.AssemblyLoader").GetMethod("Attach", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

        //TODO: These two need to stay, but could use a bit of modification.
        static void StartServer(string[] args) => GetEcoAssembly().GetType("Eco.Server.Startup").GetMethod("Start", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { args });
        static Assembly GetEcoAssembly() => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals("EcoServer", StringComparison.InvariantCultureIgnoreCase));
    }
}
