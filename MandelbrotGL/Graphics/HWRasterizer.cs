using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MandelbrotGL.Graphics
{
    /// <summary>
    /// hardware rasterizer implementation
    /// </summary>
    public class HWRasterizer : RasterizerBase
    {
        /// <summary>
        /// Shader program id
        /// </summary>
        private int _program;

        /// <summary>
        /// Attribute vertex id
        /// </summary>
        private int _attribVertex;

        /// <summary>
        /// Uniform data
        /// </summary>
        private UniformData _uniform = new();

        /// <summary>
        /// Vertex Buffer Object
        /// </summary>
        private int _vbo;

        /// <summary>
        /// Verticies data
        /// </summary>
        private readonly VertexData[] _verticies = new[]
        {
            new VertexData { X = 1.0f, Y = 1.0f },
            new VertexData { X = 1.0f, Y = -1.0f },
            new VertexData { X = -1.0f, Y = -1.0f },
            new VertexData { X = -1.0f, Y = 1.0f }
        };

        /// <inheritdoc/>
        public override void Init()
        {
            GL.ClearColor(Color4.Black);
            InitVBO();
            InitShader();
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            DestroyShader();
            DestroyVBO();
        }

        /// <inheritdoc/>
        public override void Drawcall()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // activate shader
            GL.UseProgram(_program);

            // fill shader data
            FillUniformsData();

            // use attrib vertex array and VBO
            GL.EnableVertexAttribArray((uint)_attribVertex);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // draw data
            GL.VertexAttribPointer((uint)_attribVertex, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DrawArrays(PrimitiveType.Polygon, 0, _verticies.Length);

            // destroy
            GL.DisableVertexAttribArray((uint)_attribVertex);
            GL.UseProgram(0);
        }

        /// <summary>
        /// Log shaders info
        /// </summary>
        /// <param name="tag">Log tag</param>
        /// <param name="shader">Shader id</param>
        private static void LogShader(string tag, int shader)
        {
            GL.GetShaderInfoLog(shader, out var info);

            if (!string.IsNullOrWhiteSpace(info))
                Console.WriteLine("{0} shader info: {1}", tag, info);
        }

        /// <summary>
        /// Checks OpenGL errors
        /// </summary>
        private static void CheckGlError()
        {
            var errCode = GL.GetError();
            if (errCode != ErrorCode.NoError)
                throw new Exception($"OpenGL error: {errCode}");
        }

        /// <summary>
        /// Initializes shaders
        /// </summary>
        private void InitShader()
        {
            var vsSource = File.ReadAllText("Graphics/Shaders/Mandelbrot.vert.glsl");
            var fsSource = File.ReadAllText("Graphics/Shaders/Mandelbrot.frag.glsl");

            // create vertex shader
            var vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vsSource);
            GL.CompileShader(vs);
            LogShader("Vertex", vs);

            // create fragment shader
            var fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fsSource);
            GL.CompileShader(fs);
            LogShader("Fragment", fs);

            // create program and attach shaders
            _program = GL.CreateProgram();
            GL.AttachShader(_program, vs);
            GL.AttachShader(_program, fs);
            GL.LinkProgram(_program);

            // get attrib vertex location
            _attribVertex = GL.GetAttribLocation(_program, "position");
            if (_attribVertex == -1)
            {
                Console.WriteLine($"Could not bind attrib position");
                return;
            }

            // creare uniform data
            _uniform = new()
            {
                MaxIterations = GL.GetUniformLocation(_program, "max_iterations"),
                Zoom = GL.GetUniformLocation(_program, "zoom"),
                OffsetX = GL.GetUniformLocation(_program, "offset_x"),
                OffsetY = GL.GetUniformLocation(_program, "offset_y")
            };

            // check uniform
            if (_uniform.MaxIterations == -1 ||
                _uniform.Zoom == -1 ||
                _uniform.OffsetX == -1 ||
                _uniform.OffsetY == -1)
            {
                Console.WriteLine($"Could not bind uniform!");
                return;
            }

            CheckGlError();
        }

        /// <summary>
        /// Initialize vertex buffer object
        /// </summary>
        private void InitVBO()
        {
            GL.GenBuffers(1, out _vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // pass vertex data to buffer
            var handle = GCHandle.Alloc(_verticies, GCHandleType.Pinned);
            try
            {
                GL.BufferData(BufferTarget.ArrayBuffer, _verticies.Length * 8, handle.AddrOfPinnedObject(), BufferUsageHint.StaticDraw);
                CheckGlError();
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Destroy shader
        /// </summary>
        private void DestroyShader()
        {
            GL.UseProgram(0);
            GL.DeleteProgram(_program);
        }

        /// <summary>
        /// Destroy buffers
        /// </summary>
        private void DestroyVBO()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffers(1, ref _vbo);
        }

        /// <summary>
        /// Pass uniforms data to shader
        /// </summary>
        private void FillUniformsData()
        {
            GL.Uniform1(_uniform.MaxIterations, 1000.0f);
            GL.Uniform1(_uniform.Zoom, Coefficients.ScaleFactor);
            GL.Uniform1(_uniform.OffsetX, Coefficients.OffsetX);
            GL.Uniform1(_uniform.OffsetY, Coefficients.OffsetY);
        }
    }
}
