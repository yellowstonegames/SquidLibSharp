namespace Demo {
    class Demo {
        // currently, you can press spacebar (or most keys) to display randomly-placed 'a' glyphs, and press Escape to close.
        static void Main(string[] args) {
            BearLib.Terminal.Open();
            BearLib.Terminal.Set("log: level=trace");
            BearLib.Terminal.Set("window: title='SquidLibSharp Demo', size=90x30; output: vsync=true");
            // how to configure a font:
            //BearLib.Terminal.Set("window: title='SquidLibSharp Demo', size=90x30; font: Rogue-Zodiac-12x24.png, size=12x24, codepage=custom.txt; output: vsync=true");
            while (!BearLib.Terminal.Check(BearLib.Terminal.TK_ESCAPE)) {
                BearLib.Terminal.Read();
                BearLib.Terminal.Put((int)(System.DateTime.UtcNow.Ticks / 151 % 90), (int)(System.DateTime.UtcNow.Ticks / 0xDED % 30), 'a');
                BearLib.Terminal.Refresh();
            }
        }
    }
}
