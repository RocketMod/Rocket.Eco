using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Rocket.Eco.Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            string CurrentDir = Directory.GetCurrentDirectory();
            string OutputDir = Path.Combine(CurrentDir, "Output/");
            string EcoServer = Path.Combine(CurrentDir, "EcoServer.exe");

            if (!File.Exists(EcoServer))
            {
                Console.WriteLine("Please place this executable next to EcoServer.exe!");
                EndProgram();
            }

            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }

            Assembly eco = Assembly.LoadFile(EcoServer);
            string[] resources = eco.GetManifestResourceNames().Where(x => x.EndsWith(".compressed", StringComparison.InvariantCultureIgnoreCase)).Where(x => x.StartsWith("costura.", StringComparison.InvariantCultureIgnoreCase)).ToArray();

            for (int i = 0; i < resources.Length; i++)
            {
                string finalName = resources[i].Replace(".compressed", "").Replace("costura.", "");

                try
                {
                    byte[] finalFile;

                    using (Stream stream = eco.GetManifestResourceStream(resources[i]))
                    {
                        finalFile = ExtractFile(stream);
                    }

                    File.WriteAllBytes(Path.Combine(OutputDir, finalName), finalFile);
                    Console.WriteLine($"Successfully extracted {finalName}!");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occured while extracting {finalName}!");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            EndProgram();
        }
        
        static byte[] ExtractFile(Stream asmStream)
        {
            byte[] bytes;

            using (DeflateStream deflateStream = new DeflateStream(asmStream, CompressionMode.Decompress))
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    byte[] array = new byte[81920];
                    int count;

                    while ((count = deflateStream.Read(array, 0, array.Length)) != 0)
                    {
                        memStream.Write(array, 0, count);
                    }

                    memStream.Position = 0;

                    bytes = new byte[memStream.Length];
                    memStream.Read(bytes, 0, bytes.Length);
                }
            }

            return bytes;
        }

        static void EndProgram()
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            Process.GetCurrentProcess().Kill();
        }
    }
}
