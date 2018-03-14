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
        protected RenderContext frameBuffer;
        protected byte[] displayComponents;

        public RenderContext FrameBuffer { get { return frameBuffer; } }
        public KeyboardState Input { get { return Keyboard.GetState(); } }

        public Display()
            : base(640, 480, GraphicsMode.Default, "opentk",
                   GameWindowFlags.FixedWindow)
        {
            frameBuffer = new RenderContext(Width, Height);
            displayComponents = new byte[Width * Height * 3];
            frameBuffer.Clear(0);
            frameBuffer.DrawPixel(100, 100, 0, 0, 0, 0xFF);
        }

        public new void SwapBuffers()
        {
            frameBuffer.CopyToByteArray(displayComponents);
            GL.DrawPixels(frameBuffer.Width, frameBuffer.Height,
              PixelFormat.Bgr, PixelType.UnsignedByte, displayComponents);
            base.SwapBuffers();
        }
    }
}
