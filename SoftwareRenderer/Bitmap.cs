// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

namespace SoftwareRenderer
{
    public class Bitmap
    {
        protected int width;
        protected int height;
        protected readonly byte[] components;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Bitmap(int width, int height)
        {
            this.width = width;
            this.height = height;
            components = new byte[width * height * 4];
        }

        public void Clear(byte shade)
        {
            for (int i = 0; i < components.Length; ++i)
            {
                components[i] = shade;
            }
        }

        public void DrawPixel(int x, int y, byte a, byte b, byte g, byte r)
        {
            y = height - y - 1; // flip y because OpenGL flips textures
            int index = (x + y * width) * 4;
            components[index] = a;
            components[index + 1] = b;
            components[index + 2] = g;
            components[index + 3] = r;
        }

        public void CopyToByteArray(byte[] dest)
        {
            for (int i = 0; i < width * height; ++i)
            {
                dest[i * 3] = components[i * 4 + 1];
                dest[i * 3 + 1] = components[i * 4 + 2];
                dest[i * 3 + 2] = components[i * 4 + 3];
            }
        }
    }
}
