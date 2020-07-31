using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SquidLib.SquidMath;
using SquidLib.SquidText;

namespace Test {
    public class LanguageTest {
        public void TestSupportedLanguages() {
            int registered = LanguageGen.Registry.Count;
            for (int i = 1; i < registered; i++) {
                LanguageGen lang = LanguageGen.Registry[Value.At, i];
                Console.WriteLine(lang.Name);
            }
        }
        public void TestSentences() {
            RNG random = new RNG("Language!");
            int registered = LanguageGen.Registry.Count;
            for (int i = 1; i < registered; i++) {
                LanguageGen lang = LanguageGen.Registry[Value.At, i];
                Console.WriteLine("===============================================================================");
                Console.WriteLine(lang.Name);
                Console.WriteLine();
                for (int j = 0; j < 10; j++) {
                    Console.WriteLine(lang.Sentence(random, 1, 30, 79));
                }
            }
        }
        public void WriteSentences() {
            RNG random = new RNG("Language!");
            int registered = LanguageGen.Registry.Count;
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < registered; i++) {
                LanguageGen lang = LanguageGen.Registry[Value.At, i];
                sb.AppendLine("===============================================================================");
                sb.AppendLine(lang.Name);
                sb.AppendLine();
                for (int j = 0; j < 10; j++) {
                    sb.AppendLine(lang.Sentence(random, 1, 30, 79));
                }
            }
            string temp = System.IO.Path.GetTempFileName();
            File.WriteAllText(temp, sb.ToString());
            Console.WriteLine(temp);
        }
        static void Main(string[] _) {
            LanguageTest test = new LanguageTest();
            //test.TestSupportedLanguages();
            test.TestSentences();
            test.WriteSentences();
        }
    }
}
