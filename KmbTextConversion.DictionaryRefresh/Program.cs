using System;
using System.Globalization;
using System.IO;
using KmbTextConversion;

namespace KmbTextConversion.DictionaryRefresh
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: KmbTextConversion.DictionaryRefresh.exe <route-csv-folder> [output-folder]");
                return 1;
            }

            string inputFolder = Path.GetFullPath(args[0]);
            if (!Directory.Exists(inputFolder))
            {
                Console.Error.WriteLine("Input folder not found: " + inputFolder);
                return 1;
            }

            string outputFolder = args.Length >= 2
                ? Path.GetFullPath(args[1])
                : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "KmbTextConversion", "Data"));

            KmbDictionaryBuildResult result = KmbDictionaryBuilder.BuildFromRouteCsvFolder(inputFolder, outputFolder);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Phrase pairs: {0}", result.PhraseCount));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Character pairs: {0}", result.CharacterCount));
            Console.WriteLine("Wrote: " + result.PhraseFilePath);
            Console.WriteLine("Wrote: " + result.CharacterFilePath);
            return 0;
        }
    }
}
