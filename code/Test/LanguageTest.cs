using System;
using System.Collections.Generic;

using SquidLib.SquidMath;
using SquidLib.SquidText;

namespace Test {
    public class LanguageTest {
        public void TestSentences() {
            RNG random = new RNG("Language!");
            int registered = LanguageGen.Registry.Count;
            for(int i = 1; i < registered; i++) {
                LanguageGen lang = LanguageGen.Registry[Value.At, i];
                Console.WriteLine("===============================================================================");
                Console.WriteLine(lang.Name.ToUpperInvariant());
                for (int j = 0; j < 10; j++) {
                    Console.WriteLine(lang.Sentence(random, 1, 30, 79));
                }
            }
        }
        static void Main(string[] _) {
            LanguageTest test = new LanguageTest();
            test.TestSentences();
        }
    }
}
