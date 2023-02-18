using MandelbrotGL.Graphics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MandelbrotGL
{
    /// <summary>
    /// Game window
    /// </summary>
    public class GLFrame : GameWindow
    {
        /// <summary>
        /// Lock object for rasterizer instance
        /// </summary>
        private static readonly object _rasterizerLock = new();

        /// <summary>
        /// Rasterizer
        /// </summary>
        private RasterizerBase _rasterizer = new HWRasterizer();

        /// <summary>
        /// Renderer flag
        /// </summary>
        private bool _isHwRenderer = true;

        /// <summary>
        /// Mouse button state flag
        /// </summary>
        private bool _mouseButtonPressed = false;

        /// <summary>
        /// Last FPS update time
        /// </summary>
        private DateTime _lastFpsUpdateTime;

        /// <summary>
        /// Current frames count
        /// </summary>
        private int _framesCount;

        /// <summary>
        /// Current FPS
        /// </summary>
        private double _fps;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="gameWindowSettings">Game window settings</param>
        /// <param name="nativeWindowSettings">Native window settings</param>
        public GLFrame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) 
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        /// <inheritdoc/>
        protected override void OnLoad()
        {
            base.OnLoad();
            _rasterizer.Init();
        }

        /// <inheritdoc/>
        protected override void OnUnload()
        {
            base.OnUnload();
            _rasterizer.Destroy();
        }

        /// <inheritdoc/>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            _rasterizer.Drawcall();

            SwapBuffers();

            _framesCount++;
            var totalSeconds = (DateTime.UtcNow - _lastFpsUpdateTime).TotalSeconds;

            // update FPS counter each half a second
            if (totalSeconds >= 0.5)
            {
                _fps = _framesCount / totalSeconds;
                _framesCount = 0;
                _lastFpsUpdateTime = DateTime.UtcNow;
                Title = $"Mandelbrot Fractal Zoom | OpenGL {(_isHwRenderer ? "Hardware" : "Software")} | Zoom: {1 / _rasterizer.ScaleFactor} | FPS: {_fps:F2}";
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _rasterizer.UpdateScale((float)e.OffsetY);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouseButtonPressed)
                _rasterizer.AddOffsets(e.DeltaX / (Size.X / 2), -e.DeltaY / (Size.Y / 2));

        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            _mouseButtonPressed = e.IsPressed;
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            _mouseButtonPressed = e.IsPressed;
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch(e.Key)
            {
                case Keys.F5:
                    lock(_rasterizerLock)
                    {
                        _rasterizer.Destroy();
                        _rasterizer = _isHwRenderer ? new SWRasterizer() : new HWRasterizer();
                        _rasterizer.Init();
                        _isHwRenderer = !_isHwRenderer;

                        Console.WriteLine($"Use rasterizer: {(_isHwRenderer ? "hardware" : "software")}");
                    }
                    break;

                case Keys.F12:
                    lock (_rasterizerLock)
                    {
                        Console.WriteLine("Resetting user input data...");
                        _rasterizer.Reset();
                    }
                    break;

                default: break;
            }
        }
    }
}
