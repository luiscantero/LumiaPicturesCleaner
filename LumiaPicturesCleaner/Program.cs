#define DELETE //TODO: Comment out in production!

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LumiaPicturesCleaner
{
    class Program
    {
        private static int deletedFiles = 0;
        private static long totalBytes = 0;

        static void Main(string[] args)
        {
            // Use path from 1st argument or exe location.
            string path = GetParamOrExePath(args.FirstOrDefault());

            CleanAll(path);
        }

        private static string GetParamOrExePath(string paramPath)
        {
            return paramPath?.ToString() ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static void CleanAll(string path)
        {
            Stopwatch watch = Stopwatch.StartNew();

            try
            {
            // Delete all video thumbnails: *.tnl
            ProcessToDelete(path, "*.tnl");

            Console.WriteLine("");

                ProcessFiles(path, "*.mp4.thm");
                ProcessFiles(path, "*.nar");
            }
            catch (Exception e)
            {
                watch.Stop();
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine($"Done deleting {deletedFiles} .tnl and unused .mp4.thm/.nar files, saved {(double)totalBytes / 1024 / 1024:0.##} MB!");

            // Time taken.
            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            Console.WriteLine($"Total time taken: {ts.Minutes:00}:{ts.Seconds:00}");
        }

        private static void ProcessToDelete(string path, string searchPattern)
        {
            Array.ForEach(Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories), (file) =>
            {
                Console.WriteLine($"Deleting {Path.GetFileName(file)} ...");

                IncreaseCounters(file);
#if DELETE
                File.Delete(file);
#endif
            });
        }

        private static void IncreaseCounters(string file)
        {
            deletedFiles++;
            var fileInfo = new FileInfo(file);
            totalBytes += fileInfo.Length;
        }

        private static void ProcessFiles(string path, string extension)
        {
            string[] files = GetFiles(path, extension);

            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine($"Processing file {i + 1}/{files.Length}");

                string file = files[i];
                DeleteIfNoImage(file);
            }

            if (files.Length > 0)
            {
                Console.WriteLine("");
            }
        }

        private static void DeleteIfNoImage(string file)
        {
            string name = Path.GetFileName(file);

            if (name.Length > "WP_20150920_20_05_00_".Length)
            {
                string prefix = name.Substring(0, "WP_20150920_20_05_00_".Length);

                string folder = Path.GetDirectoryName(file);
                if (Directory.GetFiles(folder, prefix + "*.jpg").Length == 0 &&
                    Directory.GetFiles(folder, prefix + "*.dng").Length == 0)
                {
                    Console.WriteLine($"Deleting {name} ...");

                    IncreaseCounters(file);
#if DELETE
                    File.Delete(file);
#endif
                }
            }
        }

        private static string[] GetFiles(string path, string searchPattern)
        {
            Console.WriteLine($"Listing {searchPattern} files in {path} ...");
            Console.WriteLine("");
            string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);

            return files;
        }
    }
}
