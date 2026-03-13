using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KmbTextConversion;

namespace KmbTextConversion.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            var failures = new List<string>();

            Run("Converts KMB stop name to simplified", failures, () =>
            {
                var converter = new ChineseScriptConverter();
                AssertEqual("尖沙咀码头,海港城 (YT912)", converter.ToSimplified("尖沙咀碼頭,海港城 (YT912)"));
            });

            Run("Converts mixed traditional sentence", failures, () =>
            {
                var converter = new ChineseScriptConverter();
                AssertEqual("了解电脑程序与鼠标", converter.ToSimplified("瞭解電腦程式與滑鼠"));
            });

            Run("Converts simplified back to traditional", failures, () =>
            {
                var converter = new ChineseScriptConverter();
                AssertEqual("瞭解電腦程序與鼠標", converter.ToTraditional("了解电脑程序与鼠标"));
            });

            Run("Parses quoted CSV fields", failures, () =>
            {
                var fields = KmbDictionaryBuilder.ParseCsvLine("1,\"尖沙咀碼頭,海港城 (YT912)\",往竹園邨,C88E34");
                AssertEqual(4, fields.Count);
                AssertEqual("尖沙咀碼頭,海港城 (YT912)", fields[1]);
            });

            Run("Builds dictionary files from route CSV folder", failures, () =>
            {
                string tempRoot = Path.Combine(Path.GetTempPath(), "KmbTextConversion.Tests", Guid.NewGuid().ToString("N"));
                string inputFolder = Path.Combine(tempRoot, "input");
                string outputFolder = Path.Combine(tempRoot, "output");
                Directory.CreateDirectory(inputFolder);

                string csvPath = Path.Combine(inputFolder, "1.csv");
                File.WriteAllText(
                    csvPath,
                    "bus_number,stop_name,direction,stop_id,stop_name_sc,direction_sc\r\n" +
                    "1,\"尖沙咀碼頭,海港城 (YT912)\",往竹園邨,C88E34E485B43EFB,\"尖沙咀码头,海港城 (YT912)\",往竹园邨\r\n" +
                    "1,寶靈街,往尖沙咀碼頭,ABCDEF,宝灵街,往尖沙咀码头\r\n",
                    new UTF8Encoding(true));

                KmbDictionaryBuildResult result = KmbDictionaryBuilder.BuildFromRouteCsvFolder(inputFolder, outputFolder);

                AssertTrue(File.Exists(result.PhraseFilePath), "Phrase file was not created.");
                AssertTrue(File.Exists(result.CharacterFilePath), "Character file was not created.");
                AssertTrue(result.PhraseCount >= 3, "Expected phrase count to include stop and direction pairs.");

                string phraseFile = File.ReadAllText(result.PhraseFilePath, Encoding.UTF8);
                AssertTrue(phraseFile.Contains("尖沙咀碼頭,海港城 (YT912)\t尖沙咀码头,海港城 (YT912)"), "Missing stop phrase pair.");
                AssertTrue(phraseFile.Contains("往竹園邨\t往竹园邨"), "Missing direction phrase pair.");
            });

            if (failures.Count == 0)
            {
                Console.WriteLine("All tests passed.");
                return 0;
            }

            Console.Error.WriteLine("Test failures: " + failures.Count);
            foreach (string failure in failures)
            {
                Console.Error.WriteLine("- " + failure);
            }

            return 1;
        }

        private static void Run(string name, IList<string> failures, Action test)
        {
            try
            {
                test();
                Console.WriteLine("[PASS] " + name);
            }
            catch (Exception ex)
            {
                failures.Add(name + ": " + ex.Message);
                Console.WriteLine("[FAIL] " + name);
            }
        }

        private static void AssertEqual(string expected, string actual)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Expected '" + expected + "' but got '" + actual + "'.");
            }
        }

        private static void AssertEqual(int expected, int actual)
        {
            if (expected != actual)
            {
                throw new InvalidOperationException("Expected '" + expected + "' but got '" + actual + "'.");
            }
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
