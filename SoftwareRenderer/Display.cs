// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace SoftwareRenderer
{
    public class Display : GameWindow
    {
        protected byte[] displayComponents;

        public RenderContext FrameBuffer { get; }
        public KeyboardState Input { get { return Keyboard.GetState(); } }

        public Display()
            : base(640, 480, GraphicsMode.Default, "Software Rendering",
                   GameWindowFlags.FixedWindow)
        {
            FrameBuffer = new RenderContext(Width, Height);
            displayComponents = new byte[Width * Height * 3];
        }

        public new void SwapBuffers()
        {
            FrameBuffer.CopyToByteArray(displayComponents);
            GL.DrawPixels(FrameBuffer.Width, FrameBuffer.Height,
              PixelFormat.Bgr, PixelType.UnsignedByte, displayComponents);
            base.SwapBuffers();
        }
    }
}
