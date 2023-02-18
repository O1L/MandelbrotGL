namespace MandelbrotGL.Graphics
{
    /// <summary>
    /// Rasterizer base class
    /// </summary>
    public abstract class RasterizerBase
    {
        /// <summary>
        /// Shader coefficients
        /// </summary>
        protected ShaderCoefficients Coefficients = new(2.0f, 0.0f, 0.0f);

        /// <summary>
        /// Gets current scale factor
        /// </summary>
        public float ScaleFactor => Coefficients.ScaleFactor;

        /// <summary>
        /// Initialize rasterizer
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Destroy rasterizer
        /// </summary>
        public abstract void Destroy();

        /// <summary>
        /// Updates scale data
        /// </summary>
        /// <param name="value">Scale update value</param>
        public void UpdateScale(float value)
        {
            var step = value / 50;
            Coefficients.ScaleFactor += step * Coefficients.ScaleFactor;
        }

        /// <summary>
        /// Adds positin offsets
        /// </summary>
        /// <param name="x">Offset X</param>
        /// <param name="y">offset Y</param>
        public void AddOffsets(float x, float y)
        {
            Coefficients.OffsetX += x * Coefficients.ScaleFactor;
            Coefficients.OffsetY += y * Coefficients.ScaleFactor;
        }

        /// <summary>
        /// Resets current scale factor and position
        /// </summary>
        public void Reset()
        {
            Coefficients = new(2.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Draws current pixel data
        /// </summary>
        public abstract void Drawcall();
    }

    /// <summary>
    /// Shader coefficients data
    /// </summary>
    public struct ShaderCoefficients
    {
        /// <summary>
        /// Scale factor
        /// </summary>
        public float ScaleFactor;

        /// <summary>
        /// Offset X
        /// </summary>
        public float OffsetX;

        /// <summary>
        /// Offset y
        /// </summary>
        public float OffsetY;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="scaleFactor">Scale factor</param>
        /// <param name="offsetX">Offset X</param>
        /// <param name="offsetY">Offset y</param>
        public ShaderCoefficients(float scaleFactor, float offsetX, float offsetY)
        {
            ScaleFactor = scaleFactor;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        public override string ToString()
            => $"Scale: {ScaleFactor}, OffsetX: {OffsetX}, OffsetY: {OffsetY}";
    }

    /// <summary>
    /// Vertex data
    /// </summary>
    public struct VertexData
    {
        /// <summary>
        /// X data
        /// </summary>
        public float X;

        /// <summary>
        /// Y data
        /// </summary>
        public float Y;
    };

    /// <summary>
    /// Fragment shader uniform data
    /// </summary>
    public struct UniformData
    {
        /// <summary>
        /// Max iterations id
        /// </summary>
        public int MaxIterations;

        /// <summary>
        /// Zoom factor id
        /// </summary>
        public int Zoom;

        /// <summary>
        /// Offset X
        /// </summary>
        public int OffsetX;

        /// <summary>
        /// Offset Y
        /// </summary>
        public int OffsetY;
    }
}
