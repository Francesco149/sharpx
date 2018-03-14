// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System.Drawing;

public class Bitmap
{
    public int Width { get; }
    public int Height { get; }
    public byte[] Components { get; }

    public Bitmap(int width, int height)
    {
        Width = width;
        Height = height;
        Components = new byte[width * height * 4];
    }

    public Bitmap(string fileName)
    {
        var image = new System.Drawing.Bitmap(fileName);
        Width = image.Width;
        Height = image.Height;
        Components = new byte[Width * Height * 4];

        for (int j = 0; j < Height; ++j)
        {
            for (int i = 0; i < Width; ++i)
            {
                Color pixel = image.GetPixel(i, j);

                Components[(j * Width + i) * 4] = pixel.A;
                Components[(j * Width + i) * 4 + 1] = pixel.B;
                Components[(j * Width + i) * 4 + 2] = pixel.G;
                Components[(j * Width + i) * 4 + 3] = pixel.R;
            }
        }
    }

    public void Clear(byte shade)
    {
        for (int i = 0; i < Components.Length; ++i)
        {
            Components[i] = shade;
        }
    }

    public void DrawPixel(int x, int y, byte a, byte b, byte g, byte r)
    {
        int index = (x + y * Width) * 4;
        Components[index] = a;
        Components[index + 1] = b;
        Components[index + 2] = g;
        Components[index + 3] = r;
    }

    public void CopyPixel(int destX, int destY, int srcX, int srcY,
                          Bitmap src, float lightAmt)
    {
        int destIndex = (destX + destY * Width) * 4;
        int srcIndex = (srcX + srcY * src.Width) * 4;
        Components[destIndex] = src.Components[srcIndex];
        Components[destIndex + 1] =
            (byte)(lightAmt * src.Components[srcIndex + 1]);
        Components[destIndex + 2] =
            (byte)(lightAmt * src.Components[srcIndex + 2]);
        Components[destIndex + 3] =
            (byte)(lightAmt * src.Components[srcIndex + 3]);
    }

    public void CopyToByteArray(byte[] dest)
    {
        for (int j = 0; j < Height; ++j)
        {
            for (int i = 0; i < Width; ++i)
            {
                // OpenGL flips the frame buffer vertically
                int sourceIndex = j * Width + i;
                int destIndex = (Height - j - 1) * Width + i;

                dest[destIndex * 3] = Components[sourceIndex * 4 + 1];
                dest[destIndex * 3 + 1] = Components[sourceIndex * 4 + 2];
                dest[destIndex * 3 + 2] = Components[sourceIndex * 4 + 3];
            }
        }
    }
}
