using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BearLib;
using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace Demo {
    public class DungeonDemo {
        private static bool keepRunning = true;

        static void Main() {
            RNG rng = new RNG();

            Terminal.Open();
            //Terminal.Set("log: level=trace");
            int width = 120, height = 40;
            Terminal.Set($"window: title='SquidLibSharp Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=9x21, hinting=autohint");
            ColorHelper.BltColor.LoadAurora();
            Terminal.Refresh();
            int input = 0;
            WanderingRoomGenerator generator = new WanderingRoomGenerator(width, height, rng);
            generator.SetRoomType(DungeonRoom.WalledBoxRoom, 7.0);
            generator.SetRoomType(DungeonRoom.WalledRoundRoom, 5.0);
            //generator.SetRoomType(DungeonRoom.Cave, 5.0);
            Grid<char> grid = generator.Generate();
            Console.WriteLine(grid.Show());
            grid = LineKit.HashesToLines(grid, true);
            DateTime current = DateTime.Now, start = DateTime.Now;
            int frames = 1;
            while (keepRunning) {
                input = Terminal.Peek();
                if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE)
                    keepRunning = false;
                else {
                    if (Terminal.HasInput())
                        input = Terminal.Read();
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            Terminal.BkColor("lead");
                            if (grid[x, y] == '.')
                                Terminal.Color("cream");
                            else
                                Terminal.Color("chinchilla");
                            Terminal.Put(x, y, grid[x, y]);
                        }
                    }

                    frames++;
                    if (current.Millisecond > DateTime.Now.Millisecond) {
                        Terminal.Set($"window.title='{frames} FPS'");
                        frames = 0;
                    }
                    current = DateTime.Now;
                    //Terminal.Color(Terminal.ColorFromName(rng.RandomElement(SColor.AuroraNames)));
                    //Terminal.Put(rng.NextInt(width), rng.NextInt(height), ArrayTools.LetterAt(input));
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
    public class LetterDemo {
        private static bool keepRunning = true;

        // currently, you can view a rippling water area in ASCII, and can press Escape to close.
        static void Main() {
            RNG rng = new RNG();

            Terminal.Open();
            //Terminal.Set("log: level=trace");
            int width = 90, height = 30;
            Terminal.Set($"window: title='SquidLibSharp Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=9x21, hinting=autohint");
            ColorHelper.BltColor.LoadAurora();
            Terminal.Refresh();
            int input = 0;
            int bright;
            DateTime current = DateTime.Now, start = DateTime.Now;
            double time = 0.0;
            FastNoise noise = new FastNoise();
            noise.SetFractalOctaves(3);
            noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            noise.SetFrequency(0.5);
            char[] waterChars = new char[] { '~', '=', '~', '~', '~', ',', '~', ',' };
            int frames = 1;
            while (keepRunning) {
                input = Terminal.Peek();
                if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE)
                    keepRunning = false;
                else {
                    if (Terminal.HasInput())
                        input = Terminal.Read();
                    time = DateTime.Now.Subtract(start).TotalSeconds;
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            if (BlueNoise.GetSeeded(x, y, 1337) < 10) {
                                Terminal.BkColor("straw");
                                Terminal.Color("driftwood");
                                Terminal.Put(x, y, '.');
                            } else {
                                bright = (int)(noise.GetNoise(x * 0.25, y * 0.5, time * 0.75) * 80 + 74);
                                Terminal.Rgb(0x33, 0xAA, 0xDD);
                                Terminal.BkRgb(bright >> 1, bright + 40 + (bright >> 1), bright + 90 + (bright >> 1));
                                Terminal.Put(x, y, waterChars[bright >> 3 & 7]);
                            }
                        }
                    }

                    frames++;
                    if (current.Millisecond > DateTime.Now.Millisecond) {
                        Terminal.Set($"window.title='{frames} FPS'");
                        frames = 0;
                    }
                    current = DateTime.Now;
                    //Terminal.Color(Terminal.ColorFromName(rng.RandomElement(SColor.AuroraNames)));
                    //Terminal.Put(rng.NextInt(width), rng.NextInt(height), ArrayTools.LetterAt(input));
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
    public class LogoDemo {
        private static bool keepRunning = true;

        public static uint GetHSV(float hue, float saturation, float value) {
            if (value <= 0.0039f) {
                return 0xFF000000u;
            } else if (saturation <= 0.0039f) {
                uint bright = Math.Min(255U, Math.Max(0U, (uint)(value * 255.5f)));
                return 0xFF000000U | bright * 0x10101u;
            } else {
                float h = ((hue + 6f) % 1f) * 6f;
                uint i = (uint)h;
                value = Math.Min(255.5f, Math.Max(0f, value * 255.5f));
                saturation = Math.Min(1f, Math.Max(0f, saturation));
                uint a = (uint)(value * (1 - saturation));
                uint b = (uint)(value * (1 - saturation * (h - i)));
                uint c = (uint)(value * (1 - saturation * (1 - (h - i))));
                uint v = (uint)value;

                switch (i) {
                    case 0:
                        return 0xFF000000U | v << 16 | c << 8 | a;
                    case 1:
                        return 0xFF000000U | b << 16 | v << 8 | a;
                    case 2:
                        return 0xFF000000U | a << 16 | v << 8 | c;
                    case 3:
                        return 0xFF000000U | a << 16 | b << 8 | v;
                    case 4:
                        return 0xFF000000U | c << 16 | a << 8 | v;
                    default:
                        return 0xFF000000U | v << 16 | a << 8 | b;
                }
            }
        }


        // currently, you can view a logo in Unicode, and can press Escape to close.
        static void Main() {
            Terminal.Open();
            string[] big = new string[]{
                "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::",
                "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::",
                "::'######:::'#######::'##::::'##:'####:'########::'##:::::::'####:'########:::",
                ":'##... ##:'##.... ##: ##:::: ##:. ##:: ##.... ##: ##:::::::. ##:: ##.... ##::",
                ": ##:::..:: ##:::: ##: ##:::: ##:: ##:: ##:::: ##: ##:::::::: ##:: ##:::: ##::",
                ":. ######:: ##:::: ##: ##:::: ##:: ##:: ##:::: ##: ##:::::::: ##:: ########:::",
                "::..... ##: ##:'## ##: ##:::: ##:: ##:: ##:::: ##: ##:::::::: ##:: ##.... ##::",
                ":'##::: ##: ##:.. ##:: ##:::: ##:: ##:: ##:::: ##: ##:::::::: ##:: ##:::: ##::",
                ":. ######::. ##### ##:. #######::'####: ########:: ########:'####: ########:::",
                "::......::::.....:..:::.......:::....::........:::........::....::........::::",
                "::::::::::::::'######::'##::::'##::::'###::::'########::'########:::::::::::::",
                ":::::::::::::'##... ##: ##:::: ##:::'## ##::: ##.... ##: ##.... ##::::::::::::",
                "::::::::::::: ##:::..:: ##:::: ##::'##:. ##:: ##:::: ##: ##:::: ##::::::::::::",
                ":::::::::::::. ######:: #########:'##:::. ##: ########:: ########:::::::::::::",
                "::::::::::::::..... ##: ##.... ##: #########: ##.. ##::: ##.....::::::::::::::",
                ":::::::::::::'##::: ##: ##:::: ##: ##.... ##: ##::. ##:: ##:::::::::::::::::::",
                ":::::::::::::. ######:: ##:::: ##: ##:::: ##: ##:::. ##: ##:::::::::::::::::::",
                "::::::::::::::......:::..:::::..::..:::::..::..:::::..::..::::::::::::::::::::",
                "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::"
            };
            int width = big[0].Length, height = big.Length, cellWidth = (int)Math.Round(1280.0 / width), cellHeight = (int)Math.Round(640.0 / height);
            Grid<char> grid = new Grid<char>(width, height, ' ');
            char[] raw = grid.RawData();
            for (int i = 0; i < height; i++) {
                big[i].CopyTo(0, raw, i * width, width);
            }
            Grid<char> lined = LineKit.HashesToLines(grid, false);
            //// add ", use-box-drawing=true" to the below config string to correct the box drawing characters, but they won't line up.
            Terminal.Set($"window: title='SquidLibSharp Logo Demo', size={width}x{height}; output: vsync=true; font: Iosevka.ttf, size={cellWidth}x{cellHeight}, hinting=autohint");
            ColorHelper.BltColor.LoadAurora();
            List<int> byHue = ColorHelper.BltColor.AuroraNames.Select(Terminal.ColorFromName).OrderBy(color => color.GetHue()).Select(color => color.ToArgb()).ToList();
            Terminal.Refresh();
            int input = 0;
            FastNoise noise = new FastNoise();
            noise.SetFractalOctaves(3);
            noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            noise.SetFrequency(0.5);
            Color lightPurple = Terminal.ColorFromName("platinum"), deepPurple = Terminal.ColorFromName("purple_freesia");
            float lightHue = lightPurple.GetHue() / 360f, deepHue = deepPurple.GetHue() / 360f, lightSat = lightPurple.GetSaturation(), deepSat = deepPurple.GetSaturation(),
                lightBright = lightPurple.GetBrightness(), deepBright = deepPurple.GetBrightness();
            //DateTime current = DateTime.Now, start = DateTime.Now;
            //double time = 0.0;
            while (keepRunning) {
                input = Terminal.Peek();
                if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE)
                    keepRunning = false;
                else {
                    if (Terminal.HasInput()) {
                        input = Terminal.Read();
                        if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE) {
                            Terminal.Close();
                            return;
                        }
                    }
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            char c = big[y][x];
                            if (c != '#') {
                                //                                if (BlueNoise.GetSeeded(x, y, 1337) < 4) {
                                //                                    Terminal.BkColor("hot_sauce");
                                //                                    Terminal.Color("apricot");
                                //                                    Terminal.Put(x, y, '#');
                                //                                } else {
                                Terminal.Gray(BlueNoise.Get(x, y));
                                Terminal.BkColor("coal_black");
                                Terminal.Put(x, y, '.');
//                                }
                            } else {
                                Terminal.Color(GetHSV(lightHue + (float)noise.GetNoise(x, y) * 0.05f, lightSat + (float)noise.GetNoise(x, -y) * 0.2f, lightBright));
                                Terminal.BkColor(GetHSV(deepHue + (float)noise.GetNoise(x, y) * 0.05f, deepSat + (float)noise.GetNoise(x, -y) * 0.15f, deepBright + (float)noise.GetNoise(-x, y) * 0.15f));
                                //int argb = byHue[(x * 4 + y) % 255];
                                //Terminal.Color(argb);
                                //Terminal.BkColor((0xFF << 24) | (argb & 0xFEFEFE) >> 1);
                                Terminal.Put(x, y, lined[x, y]);
                            }

                        }
                    }

                    Terminal.Refresh();
                }
            }
            Terminal.Close();
        }
    }
    public class NoiseDemo {
        private static bool keepRunning = true;

        static void Main() {

            double time = 0.0;
            Terminal.Open();
            int width = 256, height = 256;
            //Terminal.Set($"window.size={width}x{height};");
            Terminal.Set($"window: title='SquidLibSharp Noise Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=1x1");
            FastNoise noise = new FastNoise();
            noise.SetFrequency(0.03125);
            noise.SetFractalOctaves(1);
            noise.SetNoiseType(FastNoise.NoiseType.Simplex);
            //noise.SetFractalOctaves(3);
            //noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            double frames = 1;
            DateTime current = DateTime.Now, startTime = DateTime.Now;
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
                    time = DateTime.Now.Subtract(startTime).TotalMilliseconds * 0.06180339887498949;
                    for (int x = 0; x < 256; x++) {
                        for (int y = 0; y < 256; y++) {
                            Terminal.BkGray((int)(noise.GetNoise(x, y, time) * 125 + 127.5));
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
