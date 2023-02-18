using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MandelbrotGL.Graphics
{
    /// <summary>
    /// Software rasterizer implementation
    /// </summary>
    public class SWRasterizer : RasterizerBase
    {
        /// <inheritdoc/>
        public override void Init()
        {
        }
        
        /// <inheritdoc/>
        public override void Destroy()
        {
        }

        /// <inheritdoc/>
        public override void Drawcall()
        {
            const int w = 600;
            const int h = 600;

            var texture = CreateTexture();
            var pixels = GetPixels(w, h);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0, PixelFormat.Rgba, PixelType.Float, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Begin(PrimitiveType.Quads);
            {
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);

                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);
            }
            GL.End();

            GL.DeleteTextures(1, ref texture);
        }

        /// <summary>
        /// Linearly interpolate between two values (see reference: https://registry.khronos.org/OpenGL-Refpages/gl4/html/mix.xhtml)
        /// </summary>
        /// <param name="x">Color 1</param>
        /// <param name="y">Color 2</param>
        /// <param name="a">The value to use to interpolate between x and y</param>
        /// <returns></returns>
        public static Color4 Mix(Color4 x, Color4 y, float a) =>
            new(x.R * (1 - a) + y.R * a,
                x.G * (1 - a) + y.G * a,
                x.B * (1 - a) + y.B * a,
                x.A * (1 - a) + y.A * a);

        /// <summary>
        /// Compute the fractional part of the argument (see reference: https://registry.khronos.org/OpenGL-Refpages/gl4/html/fract.xhtml)
        /// </summary>
        /// <param name="x">Value to evaluate</param>
        /// <returns></returns>
        public static float Fract(float x) => 
            x - (float)Math.Truncate(x);

        /// <summary>
        /// Creates next fractal pixel data (emulates fragment shader routine)
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns></returns>
        private Color4 GetColor(float x, float y)
        {
            var real = x * Coefficients.ScaleFactor + Coefficients.OffsetX;
            var imag = y * Coefficients.ScaleFactor + Coefficients.OffsetY;
            var creal = real;
            var cimag = imag;

            var magnitude = 0.0f;
            float iterations;
            for (iterations = 0.0f; iterations < 1000; iterations++)
            {
                var tempReal = real;

                real = (tempReal * tempReal) - (imag * imag) + creal;
                imag = 2.0f * tempReal * imag + cimag;
                magnitude = (real * real) + (imag * imag);

                if (magnitude >= 4.0)
                    break;
            }

            if (magnitude < 4.0)
                return new Color4(0.0f, 0.0f, 0.0f, 0.0f);

            var interpolation = Fract(iterations * 0.05f);

            var color1 = new Color4(0.5f, 0.0f, 1.5f, 0.0f);
            var color2 = new Color4(0.0f, 1.5f, 0.0f, 0.0f);

            var color = Mix(color1, color2, interpolation);

            return new(color.R, color.G, color.B, 1.0f);
        }

        /// <summary>
        /// Gets pointer to pixel data
        /// </summary>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <returns>Pointer to pixel data</returns>
        private IntPtr GetPixels(int width, int height)
        {
            int getIndex(int x, int y, int s)
                => (x < 0 || x >= width || y < 0 || y >= height) ? -1 : x * 4 + y * s;

            var bpp = 32;
            var stride = (width * bpp + 7) / 8;
            var pixels = new float[height * stride];
            var scale = Coefficients.ScaleFactor / Math.Min(width, height);

            Parallel.For(0, width, (i) =>
            {
                var y = (height / 2 - i) * scale - Coefficients.OffsetY;
                for (var j = 0; j < height; j++)
                {
                    var x = (j - width / 2) * scale - Coefficients.OffsetX;
                    var color = GetColor(x, y);

                    var index = getIndex(j, i, stride);
                    if (index >= 0 && index + 3 < pixels.Length)
                    {
                        lock(pixels)
                        {
                            pixels[index + 0] = color.R;
                            pixels[index + 1] = color.G;
                            pixels[index + 2] = color.B;
                            pixels[index + 3] = color.A;
                        }
                    };
                }
            });

            var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                return handle.AddrOfPinnedObject();
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Creates texture
        /// </summary>
        /// <returns></returns>
        private static int CreateTexture()
        {
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.Texture2D);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out int texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            return texture;
        }
    }
}
