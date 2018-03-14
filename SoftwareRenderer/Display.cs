// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

public class Display : GameWindow
{
    protected byte[] bgrPixels;

    public RenderContext FrameBuffer { get; }
    public KeyboardState Input { get { return Keyboard.GetState(); } }

    public Display()
        : base(640, 480, GraphicsMode.Default, "Software Rendering",
               GameWindowFlags.FixedWindow)
    {
        FrameBuffer = new RenderContext(Width, Height);
        bgrPixels = new byte[Width * Height * 3];
    }

    public new void SwapBuffers()
    {
        FrameBuffer.CopyToByteArray(bgrPixels);
        GL.DrawPixels(FrameBuffer.Width, FrameBuffer.Height,
          PixelFormat.Bgr, PixelType.UnsignedByte, bgrPixels);
        base.SwapBuffers();
    }
}
