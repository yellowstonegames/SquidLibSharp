using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using BearLib;

using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace Demo {
    public static class DemoChooser {
        public static void Main() {
            List<(string text, Action action)> demos = new List<(string text, Action action)> {
                ("Dungeon", DungeonDemo.Main),
                ("Letter", LetterDemo.Main),
                ("Logo", LogoDemo.Main),
                ("Noise", NoiseDemo.Main),
                ("Polynomino", Polynomino.Main),
                ("Glyph", GlyphDemo.Main)
            };

            Console.WriteLine("Welcome to the SquidLibSharp Demos!");
            Console.WriteLine("Please choose a demo:");
            ShowOptions(demos);
            int exitChoice = demos.Count + 1;

            int input;
            while (!int.TryParse(Console.ReadLine().Trim(), out input) || input < 1 || input > exitChoice) {
                Console.WriteLine($"Input not recognized, please try again.");
                ShowOptions(demos);
            }

            if (input != exitChoice) {
                Console.WriteLine();
                Console.WriteLine("Press ESC to exit demo.");
                demos[input - 1].action.Invoke();
            }
        }

        private static void ShowOptions(List<(string text, Action action)> demos) {
            for (int i = 0; i < demos.Count; i++) {
                Console.WriteLine($"{i + 1} - {demos[i].text}");
            }
            int exitChoice = demos.Count + 1;
            Console.WriteLine($"{exitChoice} - Quit");
            Console.Write($"Choose 1-{exitChoice}: ");
            Console.Out.Flush();
        }
    }

    public static class DungeonDemo {
        private static bool keepRunning = true;

        public static void Main() {
            RNG rng = new RNG();

            Terminal.Open();
            //Terminal.Set("log: level=trace");
            int width = 120, height = 40;
            Terminal.Set($"window: title='SquidLibSharp Dungeon Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=9x21, hinting=autohint");
            ColorHelper.BltColor.LoadAurora();
            Terminal.Refresh();
            int input = 0;
            WanderingRoomGenerator generator = new WanderingRoomGenerator(width, height, rng);
            generator.SetRoomType(DungeonRoom.WalledBoxRoom, 7.0);
            generator.SetRoomType(DungeonRoom.WalledRoundRoom, 5.0);
            //generator.SetRoomType(DungeonRoom.Cave, 5.0);
            Grid<char> grid = generator.Generate();
            //Console.WriteLine(grid.Show());
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
                    Terminal.Refresh();
                }

            }
        }
    }

    public static class LetterDemo {
        private static bool keepRunning = true;

        // currently, you can view a rippling water area in ASCII, and can press Escape to close.
        public static void Main() {
            RNG rng = new RNG();

            Terminal.Open();
            //Terminal.Set("log: level=trace");
            int width = 90, height = 30;
            Terminal.Set($"window: title='SquidLibSharp Letter Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=9x21, hinting=autohint");
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

    public static class GlyphDemo {
        private static bool keepRunning = true;
        private static RNG rng = new RNG();

        private static Color[] mixers = new Color[] { Color.White, Color.Black, Color.AliceBlue, Color.DarkSlateGray, Color.Gray };

        public static void Main() {

            Terminal.Open();
            //Terminal.Set("log: level=trace");
            int width = 120, height = 40;
            Terminal.Set($"window: title='SquidLibSharp Glyph Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=9x21, hinting=autohint");
            ColorHelper.BltColor.LoadAurora();
            Terminal.Refresh();
            int input = 0;

            int finishedDelay = 1000;
            int strokeDelay = 30;
            DateTime start = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(finishedDelay));
            bool drawingStrokes = false;

            int size = Math.Min(width, height) - 2;
            Color color = Color.White;
            SquidLib.SquidGrid.Region painting = new SquidLib.SquidGrid.Region();
            List<List<Coord>> strokes = new List<List<Coord>>();
            Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));
            Elias los = new Elias();

            while (keepRunning) {
                input = Terminal.Peek();
                if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE) {
                    keepRunning = false;
                } else {
                    if (Terminal.HasInput()) {
                        input = Terminal.Read();
                    }
                    if (!drawingStrokes && start.AddMilliseconds(finishedDelay) < DateTime.UtcNow) {
                        Terminal.Clear();
                        color = Terminal.ColorFromName(rng.RandomElement(ColorHelper.BltColor.AuroraNames));
                        strokes = new List<List<Coord>>();

                        int points = rng.NextInt(3, 9);
                        double pointDistance = size / points;
                        for (int x = 0; x < points; x++) {
                            for (int y = 0; y < points; y++) {
                                Direction dir = rng.RandomElement(directions); // need to include NONE value to preserve original "skipping" behavior
                                if (dir == Direction.None) {
                                    continue;
                                }
                                List<Coord> line = los.Line(
                                    x * pointDistance,
                                    y * pointDistance,
                                    (x + dir.DeltaX()) * pointDistance,
                                    (y + dir.DeltaY()) * pointDistance);

                                // don't want no short short lines
                                if (line.Count < 2) {
                                    continue;
                                }

                                strokes.Add(new List<Coord>(line));
                            }
                        }

                        Terminal.Refresh();
                        rng.Shuffle(strokes); // make the draw order not just upper left to lower right
                        drawingStrokes = true;
                        start = DateTime.UtcNow.AddMilliseconds(-strokeDelay - 1); // cause the first stroke to draw right away
                    }
                    if (drawingStrokes && start.AddMilliseconds(strokeDelay) < DateTime.UtcNow) {
                        start = DateTime.UtcNow;
                        if (strokes.Count < 1) {
                            drawingStrokes = false;
                            continue;
                        }
                        List<Coord> stroke = strokes.First();
                        strokes.Remove(stroke);

                        Color blendColor = rng.RandomElement(mixers);

                        Terminal.Color(Blend(color, blendColor, rng.NextDouble(0.3)));

                        painting = new SquidLib.SquidGrid.Region(size, size);
                        painting.AddAll(stroke);
                        painting.Expand8Way(1);

                        painting.ToList().ForEach(p => Terminal.Put(p.X + 1, p.Y + 1, '#'));
                        Terminal.Refresh();
                    }
                }
            }
        }

        private static int Blend(int a, int b, double coef) {
            double cf = Math.Max(Math.Min(coef, 1), 0);
            return (int)(a + (b - a) * cf);
        }

        public static Color Blend(Color color1, Color color2, double coef) {
            return Color.FromArgb(Blend(color1.A, color2.A, coef),
                    Blend(color1.R, color2.R, coef),
                    Blend(color1.G, color2.G, coef),
                    Blend(color1.B, color2.B, coef));
        }
    }

    public static class LogoDemo {
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
        public static void Main() {
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

    public static class NoiseDemo {
        private static bool keepRunning = true;

        public static void Main() {

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
