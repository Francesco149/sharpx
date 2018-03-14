// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System.Drawing;

public class Bitmap
{
    public int Width { get; }
    public int Height { get; }
    public byte[] Bytes { get; }

    public Bitmap(int width, int height)
    {
        Width = width;
        Height = height;
        Bytes = new byte[width * height * 4];
    }

    public Bitmap(string fileName)
    {
        var image = new System.Drawing.Bitmap(fileName);
        Width = image.Width;
        Height = image.Height;
        Bytes = new byte[Width * Height * 4];

        for (int j = 0; j < Height; ++j)
        {
            for (int i = 0; i < Width; ++i)
            {
                Color pixel = image.GetPixel(i, j);

                Bytes[(j * Width + i) * 4 + 0] = pixel.A;
                Bytes[(j * Width + i) * 4 + 1] = pixel.B;
                Bytes[(j * Width + i) * 4 + 2] = pixel.G;
                Bytes[(j * Width + i) * 4 + 3] = pixel.R;
            }
        }
    }

    public void Clear(byte shade)
    {
        for (int i = 0; i < Bytes.Length; ++i)
            Bytes[i] = shade;
    }

    public void DrawPixel(int x, int y, byte a, byte b, byte g, byte r)
    {
        int index = (x + y * Width) * 4;
        Bytes[index] = a;
        Bytes[index + 1] = b;
        Bytes[index + 2] = g;
        Bytes[index + 3] = r;
    }

    public void CopyPixel(int destX, int destY, int srceX, int srceY,
        Bitmap srce, float light)
    {
        int dstIndex = (destX + destY * this.Width) * 4;
        int srcIndex = (srceX + srceY * srce.Width) * 4;
        Bytes[dstIndex] = srce.Bytes[srcIndex];
        Bytes[dstIndex + 1] = (byte)(light * srce.Bytes[srcIndex + 1]);
        Bytes[dstIndex + 2] = (byte)(light * srce.Bytes[srcIndex + 2]);
        Bytes[dstIndex + 3] = (byte)(light * srce.Bytes[srcIndex + 3]);
    }

    public void CopyToByteArray(byte[] dest)
    {
        for (int j = 0; j < Height; ++j)
        {
            for (int i = 0; i < Width; ++i)
            {
                // OpenGL flips the frame buffer vertically
                int k = Height - j - 1;

                int srcIndex = j * Width + i;
                int dstIndex = k * Width + i;

                dest[dstIndex * 3 + 0] = Bytes[srcIndex * 4 + 1];
                dest[dstIndex * 3 + 1] = Bytes[srcIndex * 4 + 2];
                dest[dstIndex * 3 + 2] = Bytes[srcIndex * 4 + 3];
            }
        }
    }
}
