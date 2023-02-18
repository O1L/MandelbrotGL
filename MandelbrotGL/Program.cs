using MandelbrotGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

var frame = new GLFrame(GameWindowSettings.Default, 
    new NativeWindowSettings
    {
        Size = new(600, 600),
        API = ContextAPI.OpenGL,
        Flags = ContextFlags.ForwardCompatible,
        Profile = ContextProfile.Compatability
    });

frame.Run();

