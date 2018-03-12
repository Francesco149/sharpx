// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

namespace SoftwareRenderer
{
    public class RenderContext : Bitmap
    {
        protected readonly int[] scanBuffer;

        public RenderContext(int width, int height)
            : base(width, height)
        {
            scanBuffer = new int[height * 2];
        }

        public void DrawScanBuffer(int yCord, int xMin, int xMax)
        {
            scanBuffer[yCord * 2] = xMin;
            scanBuffer[yCord * 2 + 1] = xMax;
        }

        public void FillShape(int yMin, int yMax)
        {
            for (int j = yMin; j < yMax; ++j)
            {
                int xMin = scanBuffer[j * 2];
                int xMax = scanBuffer[j * 2 + 1];

                for (int i = xMin; i < xMax; ++i)
                {
                    DrawPixel(i, j, 0xFF, 0xFF, 0xFF, 0xFF);
                }
            }
        }
    }
}
