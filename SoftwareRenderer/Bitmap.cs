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
        Bytes = new byte[width * height * 3];
    }

    public Bitmap(string fileName)
    {
        var image = new System.Drawing.Bitmap(fileName);
        Width = image.Width;
        Height = image.Height;
        Bytes = new byte[Width * Height * 3];

        for (int j = 0; j < Height; ++j)
        {
            for (int i = 0; i < Width; ++i)
            {
                Color pixel = image.GetPixel(i, j);

                Bytes[(j * Width + i) * 3 + 0] = pixel.B;
                Bytes[(j * Width + i) * 3 + 1] = pixel.G;
                Bytes[(j * Width + i) * 3 + 2] = pixel.R;
            }
        }
    }

    public void Clear(byte shade)
    {
        for (int i = 0; i < Bytes.Length; ++i)
            Bytes[i] = shade;
    }

    public void CopyPixel(int destX, int destY, int srceX, int srceY,
        Bitmap srce, float light)
    {
        destY = Height - destY - 1; // OpenGL's DrawPixels flips images
        int dstIndex = (destX + destY * this.Width) * 3;
        int srcIndex = (srceX + srceY * srce.Width) * 3;
        Bytes[dstIndex + 0] = (byte)(light * srce.Bytes[srcIndex + 0]);
        Bytes[dstIndex + 1] = (byte)(light * srce.Bytes[srcIndex + 1]);
        Bytes[dstIndex + 2] = (byte)(light * srce.Bytes[srcIndex + 2]);
    }
}
