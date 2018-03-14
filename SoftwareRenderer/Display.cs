// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

public class Display : GameWindow
{
    public RenderContext FrameBuffer { get; }
    public KeyboardState Input { get { return Keyboard.GetState(); } }

    public Display()
        : base(640, 480, GraphicsMode.Default, "Software Rendering",
               GameWindowFlags.FixedWindow)
    {
        FrameBuffer = new RenderContext(Width, Height);
    }

    public new void SwapBuffers()
    {
        GL.DrawPixels(FrameBuffer.Width, FrameBuffer.Height,
          PixelFormat.Bgr, PixelType.UnsignedByte, FrameBuffer.Bytes);
        base.SwapBuffers();
    }
}
