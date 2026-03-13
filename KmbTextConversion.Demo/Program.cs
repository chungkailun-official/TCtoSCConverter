using System;
using KmbTextConversion;

namespace KmbTextConversion.Demo
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var converter = new ChineseScriptConverter();

            if (args.Length >= 2)
            {
                string mode = args[0].Trim().ToLowerInvariant();
                string text = args[1];

                if (mode == "t2s")
                {
                    Console.WriteLine(converter.ToSimplified(text));
                    return 0;
                }

                if (mode == "s2t")
                {
                    Console.WriteLine(converter.ToTraditional(text));
                    return 0;
                }

                Console.Error.WriteLine("Unknown mode. Use t2s or s2t.");
                return 1;
            }

            Console.WriteLine("KmbTextConversion Demo");
            Console.WriteLine("Usage: KmbTextConversion.Demo.exe t2s <text>");
            Console.WriteLine("       KmbTextConversion.Demo.exe s2t <text>");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  t2s: " + converter.ToSimplified("瞭解電腦程式與滑鼠"));
            Console.WriteLine("  s2t: " + converter.ToTraditional("了解电脑程序与鼠标"));
            Console.WriteLine("  kmb: " + converter.ToSimplified("尖沙咀碼頭,海港城 (YT912)"));
            return 0;
        }
    }
}
