using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BearLib;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace Demo {
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
            Color lightPurple = Terminal.ColorFromName("orchid"), deepPurple = Terminal.ColorFromName("purple_freesia");
            float lightHue = lightPurple.GetHue() / 360f, deepHue = deepPurple.GetHue() / 360f, lightSat = lightPurple.GetSaturation(), deepSat = deepPurple.GetSaturation(),
                lightBright = lightPurple.GetBrightness(), deepBright = deepPurple.GetBrightness();
            //DateTime current = DateTime.Now, start = DateTime.Now;
            //double time = 0.0;
            while (keepRunning) {
                input = Terminal.Peek();
                if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE)
                    keepRunning = false;
                else {
                    if (Terminal.HasInput())
                        input = Terminal.Read();
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
                                Terminal.Color(GetHSV(lightHue + (float)noise.GetNoise(x, y) * 0.03125f, lightSat + (float)noise.GetNoise(x, -y) * 0.1f, lightBright + (float)noise.GetNoise(-x, y) * 0.125f));
                                Terminal.BkColor(GetHSV(deepHue + (float)noise.GetNoise(x, y) * 0.02f, deepSat + (float)noise.GetNoise(x, -y) * 0.075f, deepBright + (float)noise.GetNoise(-x, y) * 0.1f));
                                //int argb = byHue[(x * 4 + y) % 255];
                                //Terminal.Color(argb);
                                //Terminal.BkColor((0xFF << 24) | (argb & 0xFEFEFE) >> 1);
                                Terminal.Put(x, y, LineKit.ToHeavy[lined[x, y]]);
                            }

                        }
                    }

                    Terminal.Refresh();
                }
            }
        }
    }
    public class NoiseDemo {
        private static bool keepRunning = true;

        static void Main() {

            RNG rng = new RNG();
            double time = 0.0;
            Terminal.Open();
            int width = 512, height = 512;
            //Terminal.Set($"window.size={width}x{height};");
            Terminal.Set($"window: title='SquidLibSharp Noise Demo', size={width}x{height}; output: vsync=false; font: Iosevka.ttf, size=1x1");
            int[] grayscale = new int[256];
            for (int i = 0; i < 256; i++) {
                grayscale[i] = Color.FromArgb(i, i, i).ToArgb();
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
                        for (int y = 0; y < 512; y++) {
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
    public static class NoiseDemoTK {
        private static void Main() {
            DateTime current = DateTime.Now;
            int frames = 0;

            SimplexNoise noise = new SimplexNoise();

            int time = 0;

            using (var window = new NoiseWindow()) {
                window.VSync = VSyncMode.Off;
                while (window.WindowUpdate()) {
                    if (window.GetKey() == OpenTK.Input.Key.Escape) {
                        window.Close();
                        break;
                    }
                    time++;
                    for (int i = 0, y = 0; y < 512; y++) {
                        for (int x = 0; x < 512; x++) {
                            window.colors[i++] = (byte)(noise.GetNoise(x * 0.03125, y * 0.03125, time * 0.03125) * 127.5 + 127.5);
                        }
                    }
                    frames++;
                    if (current.Millisecond > DateTime.Now.Millisecond) {
                        window.Title = ($"{frames} FPS");
                        frames = 0;
                    }
                    current = DateTime.Now;
                }
            }
        }
    }
    public static class FastNoiseDemoTK {
        private static void Main() {
            DateTime current = DateTime.Now;
            int frames = 0;

            FastNoise noise = new FastNoise(), warp = new FastNoise(12345);
            noise.SetFrequency(0.03125 * 0.5);
            noise.SetNoiseType(FastNoise.NoiseType.Simplex);
            warp.SetFrequency(0.03125 * 2.0);
            warp.SetNoiseType(FastNoise.NoiseType.CubicFractal);
            warp.SetFractalOctaves(2);
            warp.SetFractalType(FastNoise.FractalType.Billow);
            warp.SetFractalLacunarity(2);
            warp.SetFractalGain(4);

            int time = 0;

            using (var window = new NoiseWindow()) {
                window.VSync = VSyncMode.Off;
                while (window.WindowUpdate()) {
                    if (window.GetKey() == OpenTK.Input.Key.Escape) {
                        window.Close();
                        break;
                    }
                    time++;
                    for (int i = 0, y = 0; y < window.Height; y++) {
                        for (int x = 0; x < window.Width; x++) {
                            //window.colors[i++] = (byte)(noise.GetSimplex(x + y + time, y - x - time, time - x - y, x - y - time) * 125 + 128);
                            window.colors[i++] = (byte)(noise.GetSimplex(x, y, 0.375 * time, warp.GetNoise(-x, -y, -0.5 * time) * 200) * 125 + 128);
                        }
                    }
                    frames++;
                    if (current.Millisecond > DateTime.Now.Millisecond) {
                        window.Title = ($"{frames} FPS");
                        frames = 0;
                    }
                    current = DateTime.Now;
                }
            }
        }
    }

    /*Copyright (c) 2015  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
    public class NoiseWindow : GameWindow {
        protected int internal_rows;
        protected int internal_cols;
        protected int first_changed_row, last_changed_row, first_changed_col, last_changed_col;
        protected bool hold_updates;
        protected bool internal_key_pressed;
        protected OpenTK.Input.Key internal_last_key;
        protected FrameEventArgs render_args = new FrameEventArgs(); //This is a necessary step if you're not using the default GameWindow loop.
        protected bool resizing;
        protected int num_elements;
        protected static float half_height;
        protected static float half_width;

        private int id;
        internal byte[] colors;

        public NoiseWindow() : base(256, 256, GraphicsMode.Default, "0 FPS") {
            colors = new byte[Width * Height];
            VSync = VSyncMode.On;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            internal_rows = 1;
            internal_cols = 1;
            half_height = 0.5f;
            half_width = 0.5f;
            ResetChangedPositions();
            KeyDown += (sender, e) => {
                if (!internal_key_pressed) {
                    internal_key_pressed = true;
                    internal_last_key = e.Key;
                }
            };
            LoadTexture();
            LoadShaders();
            CreateVBO(1, 1);
            Visible = true;
            //WindowBorder = WindowBorder.Fixed;
        }
        public int Rows => internal_rows;
        public int Cols => internal_cols;
        public bool KeyPressed => internal_key_pressed;
        public OpenTK.Input.Key GetKey() {
            if (internal_key_pressed) {
                internal_key_pressed = false;
                return internal_last_key;
            }
            return OpenTK.Input.Key.Unknown;
        }
        public bool KeyIsDown(OpenTK.Input.Key key) => OpenTK.Input.Keyboard.GetState().IsKeyDown(key);

        private readonly float[] positions = new float[]{
                        -1f,-1f,0f,1, 1,1,1,1, 1,1,1,1,
                        -1f,1f,0f,0, 1,1,1,1, 1,1,1,1,
                        1f,1f,1f,0, 1,1,1,1, 1,1,1,1,
                        1f,-1f,1f,1, 1,1,1,1, 1,1,1,1
                };
        protected void UpdateGLBuffer() {
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, new IntPtr(sizeof(float) * 48), positions);
            ResetChangedPositions();

        }
        protected void ResetChangedPositions() {
            first_changed_row = internal_rows; //these 4 are set to out of bounds values.
            first_changed_col = internal_cols;
            last_changed_row = -1;
            last_changed_col = -1;
        }
        public void HoldUpdates() => hold_updates = true;
        public void ResumeUpdates() {
            hold_updates = false;
            UpdateGLBuffer();
        }
        public bool WindowUpdate() {
            ProcessEvents();
            if (IsExiting) {
                return false;
            }
            Render();
            return true;
        }
        protected void Render() {
            base.OnRenderFrame(render_args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            //GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, PixelFormat.Luminance, PixelType.UnsignedByte, colors);
            GL.DrawElements(PrimitiveType.Triangles, num_elements, DrawElementsType.UnsignedInt, IntPtr.Zero);
            SwapBuffers();
        }
        protected void LoadTexture() {
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Luminance, Width, Height, 0, PixelFormat.Luminance, PixelType.UnsignedByte, colors);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }
        protected void LoadShaders() {
            int vertex_shader = GL.CreateShader(ShaderType.VertexShader);
            int fragment_shader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(vertex_shader,
                @"#version 120
attribute vec4 position;
attribute vec2 texcoord;
attribute vec4 color;
attribute vec4 bgcolor;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
texcoord_fs = texcoord;
color_fs = color;
bgcolor_fs = bgcolor;
gl_Position = position;
}
");
            GL.ShaderSource(fragment_shader,
                @"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
gl_FragColor = texture2D(texture,texcoord_fs);
}
");
            GL.CompileShader(vertex_shader);
            GL.CompileShader(fragment_shader);
            int compiled;
            GL.GetShader(vertex_shader, ShaderParameter.CompileStatus, out compiled);
            if (compiled < 1) {
                Console.Error.WriteLine(GL.GetShaderInfoLog(vertex_shader));
                throw new Exception("vertex shader compilation failed");
            }
            GL.GetShader(fragment_shader, ShaderParameter.CompileStatus, out compiled);
            if (compiled < 1) {
                Console.Error.WriteLine(GL.GetShaderInfoLog(fragment_shader));
                throw new Exception("fragment shader compilation failed");
            }
            int shader_program = GL.CreateProgram();
            GL.AttachShader(shader_program, vertex_shader);
            GL.AttachShader(shader_program, fragment_shader);
            GL.BindAttribLocation(shader_program, 0, "position");
            GL.BindAttribLocation(shader_program, 1, "texcoord");
            GL.BindAttribLocation(shader_program, 2, "color");
            GL.BindAttribLocation(shader_program, 3, "bgcolor");
            GL.LinkProgram(shader_program);
            GL.UseProgram(shader_program);
        }
        protected void CreateVBO(int rows, int cols) {
            float[] f = new float[48]; //4 vertices, 12 pieces of data.
            num_elements = 6;
            int[] indices = new int[num_elements];
            int i = 0;
            int j = 0;
            int idx = (j + i * cols) * 48;
            int flipped_row = (rows - 1) - i;
            float fi = (flipped_row / half_height) - 1.0f;
            float fj = (j / half_width) - 1.0f;
            float fi_plus1 = ((flipped_row + 1) / half_height) - 1.0f;
            float fj_plus1 = ((j + 1) / half_width) - 1.0f;
            float[] values = new float[] { fj, fi, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0,
                        fj, fi_plus1, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0,
                        fj_plus1, fi_plus1, 1.0f, 0, 1, 1, 1, 1, 0, 0, 0, 0,
                        fj_plus1, fi, 1.0f, 1, 1, 1, 1, 1, 0, 0, 0, 0
                    };
            values.CopyTo(f, idx); //x, y, s, t, r, g, b, a, bgr, bgg, bgb, bga

            int idx4 = (j + i * cols) * 4;
            int idx6 = (j + i * cols) * 6;
            indices[idx6] = idx4;
            indices[idx6 + 1] = idx4 + 1;
            indices[idx6 + 2] = idx4 + 2;
            indices[idx6 + 3] = idx4;
            indices[idx6 + 4] = idx4 + 2;
            indices[idx6 + 5] = idx4 + 3;
            int vert_id;
            int elem_id;
            GL.GenBuffers(1, out vert_id);
            GL.GenBuffers(1, out elem_id);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vert_id);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elem_id);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * f.Length), f, BufferUsageHint.StreamDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(int) * indices.Length), indices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 12, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 12, new IntPtr(sizeof(float) * 2));
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, sizeof(float) * 12, new IntPtr(sizeof(float) * 4));
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, sizeof(float) * 12, new IntPtr(sizeof(float) * 8));

            UpdateGLBuffer();
        }
    }
}
