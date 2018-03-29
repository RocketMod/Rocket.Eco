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

            //AttachAssemblies();
            Runtime.Bootstrap();

            if (args.Length == 0 || !args[0].Equals("-extract", StringComparison.InvariantCultureIgnoreCase)) 
            {
                StartServer(args);
            }
        }
        
        static void StartServer(string[] args) => GetEcoAssembly().GetType("Eco.Server.Startup").GetMethod("Start", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { args });
        static Assembly GetEcoAssembly() => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals("EcoServer", StringComparison.InvariantCultureIgnoreCase));
    }
}