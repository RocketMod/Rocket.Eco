using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Rocket.Eco.EcoAssemblyExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Program));

            string directory = Directory.GetCurrentDirectory();

            string[] files = Directory.GetFiles(directory)?.Where(x => x.EndsWith(".compressed", StringComparison.InvariantCultureIgnoreCase))?.ToArray() ?? default(string[]);

            if (files.Length != 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        ExtractFile(files[i]);
                        Console.WriteLine($"Successfully extracted {files[i]}!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to extract {files[i]}!");
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                }

                Console.WriteLine("All files have been extracted!");
            }
            else
            {
                Console.WriteLine("No compressed files were found!");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static void ExtractFile(string file)
        {
            byte[] bytes;

            using (FileStream fileStream = File.Open(file, FileMode.Open))
            {
                using (DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress))
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
            }

            File.WriteAllBytes(file.Replace(".compressed", string.Empty).Replace("costura.", string.Empty), bytes);
        }
    }
}
