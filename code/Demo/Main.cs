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
            Terminal.Set($"window: title='SquidLibSharp Demo', size={width}x{height}; output: vsync=true; font: Iosevka.ttf, size=9x21, hinting=autohint");
            Terminal.Refresh();
            
            // how to configure a font:
            //BearLib.Terminal.Set("window: title='SquidLibSharp Demo', size=90x30; font: Rogue-Zodiac-12x24.png, size=12x24, codepage=custom.txt; output: vsync=true");
            while (keepRunning) {
                switch (Terminal.Read()) {
                    case Terminal.TK_ESCAPE:
                    case Terminal.TK_CLOSE:
                        keepRunning = false;
                        break;
                    case int val:
                        Terminal.Put(rng.NextInt(width), rng.NextInt(height), ArrayTools.LetterAt(val));
                        Terminal.Refresh();
                        break;
                }

            }
        }
    }
}
