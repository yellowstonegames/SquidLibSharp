using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SquidLib.SquidMath;

namespace TkDemo {
    //public static class NoiseDemoTK {
    //    private static void Main() {
    //        DateTime current = DateTime.Now, startTime = DateTime.Now;
    //        int frames = 0;

    //        SimplexNoise noise = new SimplexNoise();

    //        double time = 0;

    //        using (var window = new NoiseWindow()) {
    //            window.VSync = VSyncMode.Off;
    //            while (window.WindowUpdate()) {
    //                if (window.GetKey() == OpenTK.Input.Key.Escape) {
    //                    window.Close();
    //                    break;
    //                }
    //                time = DateTime.Now.Subtract(startTime).TotalSeconds * 0.6180339887498949;
    //                for (int i = 0, y = 0; y < window.Height; y++) {
    //                    for (int x = 0; x < window.Width; x++) {
    //                        window.colors[i++] = (byte)(noise.GetNoise(x * 0.03125, y * 0.03125, time) * 127.5 + 127.5);
    //                    }
    //                }
    //                frames++;
    //                if (current.Millisecond > DateTime.Now.Millisecond) {
    //                    window.Title = ($"{frames} FPS");
    //                    frames = 0;
    //                }
    //                current = DateTime.Now;
    //            }
    //        }
    //    }
    //}
    public static class FastNoiseDemoTK {
        private static void Main() {
            DateTime current = DateTime.Now, startTime = DateTime.Now;
            int frames = 0;

            FastNoise noise = new FastNoise(543210);
            noise.SetFrequency(0.03125);
            noise.SetNoiseType(FastNoise.NoiseType.Simplex);

//            FastNoise warp = new FastNoise(12345);
//            warp.SetFrequency(0.03125 * 2.0);
//            warp.SetNoiseType(FastNoise.NoiseType.CubicFractal);
//            warp.SetFractalOctaves(2);
//            warp.SetFractalType(FastNoise.FractalType.Billow);
//            warp.SetFractalLacunarity(2);
//            warp.SetFractalGain(4);
//
            double time = 0;
            double result;
            using (var window = new NoiseWindow()) {
                window.VSync = VSyncMode.Off;
                while (window.WindowUpdate()) {
                    if (window.GetKey() == OpenTK.Input.Key.Escape) {
                        window.Close();
                        break;
                    }
                    time = DateTime.Now.Subtract(startTime).TotalSeconds * (32.0 * 0.6180339887498949);
                    for (int i = 0, y = 0; y < window.Height; y++) {
                        for (int x = 0; x < window.Width; x++) {
                            result = noise.GetSimplex(x, y, time);
                            //result = noise.GetSimplex(
                            //    time,
                            //    time * -0.3333333333333333 + y * 0.9428090415820634,
                            //    time * -0.3333333333333333 + y * -0.4714045207910317 + x * 0.816496580927726,
                            //    time * -0.3333333333333333 + y * -0.4714045207910317 + x * -0.816496580927726);

                            //result = noise.GetSimplex(x + y + time, y - x - time, time - x - y, x - y - time);
                            //                            result = noise.GetSimplex(x, y, time);

                            //if (result < -1.0) Console.WriteLine($"Result too low! {result}");
                            //if (result > 1.0) Console.WriteLine($"Result too high! {result}");
                            window.colors[i++] = (byte)(result * 127.5 + 127.5);
                            //window.colors[i++] = (byte)(noise.GetSimplex(x, y, 0.375 * time, warp.GetNoise(-x, -y, -0.5 * time) * 200) * 125 + 128);
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
