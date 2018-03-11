// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SoftwareRenderer
{
    public class Display : GameWindow
    {
        protected Bitmap frameBuffer;
        protected byte[] displayComponents;

        public Display()
            : base(800, 600, GraphicsMode.Default, "opentk",
                   GameWindowFlags.FixedWindow)
        {
            frameBuffer = new Bitmap(Width, Height);
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
