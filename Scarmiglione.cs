using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Scarmiglione
{
    public class Shader
    {
        public int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            // Load shader data from individual shader files
            string VertexShaderSource = File.ReadAllText(vertexPath);

            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            var VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Attempt to compile each shader, write error to console if failed
            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine(infoLog);
            }

            // Create shader program from loaded shaders to be used by application
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

            // Cleanup (when LinkProgram happens, shaders are copied onto program)
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // Disposal code
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class Game : GameWindow
    {
        private Stopwatch _timer;
        int VertexBufferObject;
        int ElementBufferObject;
        int VertexArrayObject;
        private Shader shader;

        float[] vertices = {
            0.5f,  0.5f,  0.0f,     // top right
            0.5f, -0.5f,  0.0f,     // bottom right
           -0.5f, -0.5f,  0.0f,     // bottom left
           -0.5f,  0.5f,  0.0f      // top left
        };
        uint[] indices = {
            0, 1, 3,    // first triangle
            1, 2, 3     // second triangle
        };

        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { }

        protected override void OnLoad()                            // Runs once when the window first opens
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);


            // Code
            int nrAttributes = 0;
            GL.GetInteger(GetPName.MaxVertexAttribs, out nrAttributes);
            Console.WriteLine("Maximum number of vertex attributes supported: " + nrAttributes);

            // Buffer initialization
            // - vertex buffer
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // - element buffer
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);


            shader = new Shader("shader.vert", "shader.frag");

            shader.Use();

            _timer = new Stopwatch();
            _timer.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)     // Runs each time a frame is rendered
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Code

            shader.Use();

            double timeValue = _timer.Elapsed.TotalSeconds;
            float greenValue = (float)Math.Sin(timeValue) / 2.0f + 0.5f;
            int vertexColorLocation = GL.GetUniformLocation(shader.Handle, "ourColor");
            GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);

            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)         // Runs each time the window is resized
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            KeyboardState input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            shader.Dispose();
        }
    }

    public class Program
    {
    static void Main(string[] args)
        {
            using (Game game = new Game(800, 600, "LearnOpenTK"))
            {
                game.Run();
            }
        }
    }
}
