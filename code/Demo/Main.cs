using BearLib;
using SquidLib;
using SquidLib.SquidMath;

namespace Demo {
    class Demo {
        private static bool keepRunning = true;

        // currently, you can press spacebar (or most keys) to display randomly-placed 'a' glyphs, and press Escape to close.
        static void Main() {
            RNG rng = new RNG();

            Terminal.Open();
            Terminal.Set("log: level=trace");
            int width = 90, height = 30;
            //Terminal.Set($"window.size={width}x{height};");
            Terminal.Set($"window: title='SquidLibSharp Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=9x21, hinting=autohint");
            SColor.LoadAurora();
            Terminal.Refresh();
            int input = 0;
            while (keepRunning) {
                input = Terminal.Peek();
                if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE)
                    keepRunning = false;
                else {
                    if(Terminal.HasInput())
                        input = Terminal.Read();
                    Terminal.Color(Terminal.ColorFromName(rng.RandomElement(SColor.AuroraNames)));
                    Terminal.Put(rng.NextInt(width), rng.NextInt(height), ArrayTools.LetterAt(input));
                    Terminal.Refresh();
                }
                //switch (Terminal.Read()) {
                //    case Terminal.TK_ESCAPE:
                //    case Terminal.TK_CLOSE:
                //        keepRunning = false;
                //        break;
                //    case int val:
                //        Terminal.Color(Terminal.ColorFromName(rng.RandomElement(SColor.AuroraNames)));
                //        Terminal.Put(rng.NextInt(width), rng.NextInt(height), ArrayTools.LetterAt(rng.NextInt()));
                //        Terminal.Refresh();
                //        break;
                //}

            }
        }
    }
}
