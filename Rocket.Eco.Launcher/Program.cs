using System.IO;
using System.Reflection;

namespace Rocket.Eco.Launcher
{
    class MainClass
    {
        static Assembly asm;

        public static void Main(string[] args)
        {
            string file = Path.Combine(Directory.GetCurrentDirectory(), "EcoServer.exe");
            asm = Assembly.LoadFile(file);

            MethodInfo attachAssemblies = asm.GetType("Costura.AssemblyLoader").GetMethod("Attach", BindingFlags.Static | BindingFlags.Public);
            MethodInfo startServer = asm.GetType("Eco.Server.Startup").GetMethod("Start", BindingFlags.Static | BindingFlags.Public);

            attachAssemblies.Invoke(null, null);
            RocketAttacher.InitialzeRocketHook();
            startServer.Invoke(null, new object[] { args });
        }
    }
}
