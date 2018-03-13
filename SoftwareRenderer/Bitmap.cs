// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System.Drawing;

namespace SoftwareRenderer
{
    public class Bitmap
    {
        protected int width;
        protected int height;
        protected readonly byte[] components;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public byte[] Components { get { return components; } }

        public Bitmap(int width, int height)
        {
            this.width = width;
            this.height = height;
            components = new byte[width * height * 4];
        }

        public Bitmap(string fileName)
        {
            var image = new System.Drawing.Bitmap(fileName);
            width = image.Width;
            height = image.Height;
            components = new byte[width * height * 4];

            for (int j = 0; j < height; ++j)
            {
                for (int i = 0; i < width; ++i)
                {
                    Color pixel = image.GetPixel(i, j);

                    components[(j * width + i) * 4] = pixel.A;
                    components[(j * width + i) * 4 + 1] = pixel.B;
                    components[(j * width + i) * 4 + 2] = pixel.G;
                    components[(j * width + i) * 4 + 3] = pixel.R;
                }
            }
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
            int index = (x + y * width) * 4;
            components[index] = a;
            components[index + 1] = b;
            components[index + 2] = g;
            components[index + 3] = r;
        }

        public void CopyPixel(int destX, int destY, int srcX, int srcY,
                              Bitmap src)
        {
            int destIndex = (destX + destY * width) * 4;
            int srcIndex = (srcX + srcY * src.Width) * 4;
            components[destIndex] = src.Components[srcIndex];
            components[destIndex + 1] = src.Components[srcIndex + 1];
            components[destIndex + 2] = src.Components[srcIndex + 2];
            components[destIndex + 3] = src.components[srcIndex + 3];
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
