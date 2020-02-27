using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using BearLib;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SquidLib;
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
            //SColor.LoadAurora();
            Terminal.Refresh();
            int input = 0;
            int bright;
            DateTime current = DateTime.Now, start = DateTime.Now;
            double time = 0.0;
            FastNoise noise = new FastNoise();
            noise.SetFractalOctaves(3);
            noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            noise.SetFrequency(0.5);
            char[] waterChars = new char[] { '~', '=', '~', '~', '~', ',', '~', ','};
            int frames = 1;
            while (keepRunning) {
                input = Terminal.Peek();
                if (input == Terminal.TK_Q || input == Terminal.TK_ESCAPE || input == Terminal.TK_CLOSE)
                    keepRunning = false;
                else {
                    if (Terminal.HasInput())
                        input = Terminal.Read();
                    time = DateTime.Now.Subtract(start).TotalSeconds;
                    Terminal.Rgb(0x33, 0xAA, 0xDD);
                    for(int y = 0; y < height; y++) {
                        for(int x = 0; x < width; x++) {
                            bright = (int)(noise.GetNoise(x * 0.25, y * 0.5, time * 0.75) * 80 + 74);
                            Terminal.BkRgb(bright >> 1, bright + 40 + (bright >> 1), bright + 90 + (bright >> 1));
                            Terminal.Put(x, y, waterChars[bright >> 3 & 7]);
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
                    if (window.GetKey() == Key.Escape) {
                        window.Close();
                        break;
                    }
                    time++;
                    for (int i = 0, y = 0; y < 512; y++) {
                        for (int x = 0; x < 512; x++) {
                            window.colors[i++] = (byte)(noise.GetNoise((x + time) * 0.03125, (y + time) * 0.03125) * 127.5 + 127.5);
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

            FastNoise noise = new FastNoise();
            noise.SetFrequency(0.03125);
            noise.SetFractalOctaves(1);
            noise.SetNoiseType(FastNoise.NoiseType.Simplex);

            int time = 0;

            using (var window = new NoiseWindow()) {
                window.VSync = VSyncMode.Off;
                while (window.WindowUpdate()) {
                    if (window.GetKey() == Key.Escape) {
                        window.Close();
                        break;
                    }
                    time++;
                    for (int i = 0, y = 0; y < 512; y++) {
                        for (int x = 0; x < 512; x++) {
                            window.colors[i++] = (byte)(noise.GetSimplex(x + time, y + time) * 125 + 128);
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
        protected Key internal_last_key;
        protected FrameEventArgs render_args = new FrameEventArgs(); //This is a necessary step if you're not using the default GameWindow loop.
        protected bool resizing;
        protected int num_elements;
        protected static float half_height;
        protected static float half_width;

        private int id;
        internal byte[] colors = new byte[512 * 512];

        public NoiseWindow() : base(512, 512, GraphicsMode.Default, "0 FPS") {

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
        public int Rows { get { return internal_rows; } }
        public int Cols { get { return internal_cols; } }
        public bool KeyPressed { get { return internal_key_pressed; } }
        public Key GetKey() {
            if (internal_key_pressed) {
                internal_key_pressed = false;
                return internal_last_key;
            }
            return Key.Unknown;
        }
        public bool KeyIsDown(Key key) { return OpenTK.Input.Keyboard.GetState().IsKeyDown(key); }

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
        public void HoldUpdates() {
            hold_updates = true;
        }
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
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 512, 512, PixelFormat.Luminance, PixelType.UnsignedByte, colors);
            GL.DrawElements(PrimitiveType.Triangles, num_elements, DrawElementsType.UnsignedInt, IntPtr.Zero);
            SwapBuffers();
        }
        protected void LoadTexture() {
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Luminance, 512, 512, 0, PixelFormat.Luminance, PixelType.UnsignedByte, colors);

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
            float fi = ((float)flipped_row / half_height) - 1.0f;
            float fj = ((float)j / half_width) - 1.0f;
            float fi_plus1 = ((float)(flipped_row + 1) / half_height) - 1.0f;
            float fj_plus1 = ((float)(j + 1) / half_width) - 1.0f;
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
