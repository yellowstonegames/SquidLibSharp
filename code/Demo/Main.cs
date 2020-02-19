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
            Terminal.Set($"window: title='SquidLibSharp Demo', size={width}x{height}; output: vsync=true; font: Iosevka.ttf, size=9x21, hinting=autohint");
            SColor.LoadSColor();
            Terminal.Refresh();
            while (keepRunning) {
                switch (Terminal.Read()) {
                    case Terminal.TK_ESCAPE:
                    case Terminal.TK_CLOSE:
                        keepRunning = false;
                        break;
                    case int val:
                        string name = rng.RandomElement(SColor.Names);
                        System.Console.WriteLine(name);
                        Terminal.Color(Terminal.ColorFromName(name));
                        Terminal.Put(rng.NextInt(width), rng.NextInt(height), ArrayTools.LetterAt(val));
                        Terminal.Refresh();
                        break;
                }

            }
        }
    }
}
