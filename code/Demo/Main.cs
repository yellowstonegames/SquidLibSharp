using System;
using System.Drawing;
using BearLib;
using SquidLib;
using SquidLib.SquidMath;

namespace Demo {
    public class LetterDemo {
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
                    if (Terminal.HasInput())
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
    public class NoiseDemo {
        private static bool keepRunning = true;

        // currently, you can press spacebar (or most keys) to display randomly-placed 'a' glyphs, and press Escape to close.
        static void Main() {
            RNG rng = new RNG();
            double time = 0.0;
            Terminal.Open();
            int width = 512, height = 512;
            //Terminal.Set($"window.size={width}x{height};");
            Terminal.Set($"window: title='SquidLibSharp Noise Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=1x1");
            Color[] grayscale = new Color[256];
            for(int i = 0; i < 256; i++) {
                grayscale[i] = Color.FromArgb(i, i, i);
            }
            FastNoise noise = new FastNoise();
            noise.SetFrequency(0.03125);
            noise.SetFractalOctaves(3);
            noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            double frames = 1;
            DateTime current = DateTime.Now;
            Terminal.Refresh();
            int input;
            while (keepRunning) {
                input = Terminal.Peek();
                if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE)
                    keepRunning = false;
                else {
                    if (Terminal.HasInput()) {
                        _ = Terminal.Read();
                    }
                    // this is a really bad practice; I just want to get an idea of how fast or slow this is.
                    time++;
                    for (int x = 0; x < 512; x++) {
                        for(int y = 0; y < 512; y++) {
                            Terminal.BkColor(grayscale[(int)(noise.GetNoise(x + time, y + time) * 125 + 127.5)]);
                            Terminal.Put(x, y, ' ');
                        }
                    }
                    frames++;
                    if (current.Millisecond > DateTime.Now.Millisecond) {
                        Terminal.Set($"window.title='{frames} FPS'");
                        frames = 0;
                    }
                    current = DateTime.Now;
                    Terminal.Refresh();
                }
            }
        }
    }
}
