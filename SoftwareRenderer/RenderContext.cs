// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using OpenTK;

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

        protected void ScanConvertLine(Vertex minYVert, Vertex maxYVert,
                                      int whichSide)
        {
            int yStart = (int)Math.Ceiling(minYVert.Y);
            int yEnd = (int)Math.Ceiling(maxYVert.Y);
            int xStart = (int)Math.Ceiling(minYVert.X);
            int xEnd = (int)Math.Ceiling(maxYVert.X);

            float yDist = maxYVert.Y - minYVert.Y;
            float xDist = maxYVert.X - minYVert.X;

            if (yDist <= 0)
            {
                return;
            }

            float xStep = xDist / yDist;
            float yPrestep = yStart - minYVert.Y;
            float curX = minYVert.X + yPrestep * xStep;

            for (int j = yStart; j < yEnd; ++j)
            {
                scanBuffer[j * 2 + whichSide] = (int)Math.Ceiling(curX);
                curX += xStep;
            }
        }

        public void FillTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            Matrix4 screenSpaceTransform =
                Matrix4Utils.InitScreenSpaceTransform(Width / 2, Height / 2);

            Vertex minYVert = v1.Transform(screenSpaceTransform)
                                .PerspectiveDivide();
            Vertex midYVert = v2.Transform(screenSpaceTransform)
                                .PerspectiveDivide();
            Vertex maxYVert = v3.Transform(screenSpaceTransform)
                                .PerspectiveDivide();

            if (maxYVert.Y < midYVert.Y)
            {
                Vertex tmp = maxYVert;
                maxYVert = midYVert;
                midYVert = tmp;
            }

            if (midYVert.Y < minYVert.Y)
            {
                Vertex tmp = midYVert;
                midYVert = minYVert;
                minYVert = tmp;
            }

            if (maxYVert.Y < midYVert.Y)
            {
                Vertex tmp = maxYVert;
                maxYVert = midYVert;
                midYVert = tmp;
            }

            float area = minYVert.TriangleAreaTimesTwo(maxYVert, midYVert);
            int handedness = area >= 0 ? 1 : 0;

            ScanConvertTriangle(minYVert, midYVert, maxYVert, handedness);
            FillShape((int)Math.Ceiling(minYVert.Y),
                      (int)Math.Ceiling(maxYVert.Y));
        }

        public void ScanConvertTriangle(Vertex minYVert, Vertex midYVert,
                                        Vertex maxYVert, int handedness)
        {
            ScanConvertLine(minYVert, maxYVert, 0 + handedness);
            ScanConvertLine(minYVert, midYVert, 1 - handedness);
            ScanConvertLine(midYVert, maxYVert, 1 - handedness);
        }
    }
}
