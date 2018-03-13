// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using OpenTK;

namespace SoftwareRenderer
{
    public class RenderContext : Bitmap
    {
        public RenderContext(int width, int height)
            : base(width, height)
        {

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

            bool handedness =
                minYVert.TriangleAreaTimesTwo(maxYVert, midYVert) >= 0;

            ScanTriangle(minYVert, midYVert, maxYVert, handedness);
        }

        protected void ScanTriangle(Vertex minYVert, Vertex midYVert,
                                Vertex maxYVert, bool handedness)
        {
            Gradients gradients = new Gradients(minYVert, midYVert, maxYVert);
            Edge topToBottom = new Edge(gradients, minYVert, maxYVert, 0);
            Edge topToMiddle = new Edge(gradients, minYVert, midYVert, 0);
            Edge middleToBottom = new Edge(gradients, midYVert, maxYVert, 1);

            ScanEdges(gradients, topToBottom, topToMiddle, handedness);
            ScanEdges(gradients, topToBottom, middleToBottom, handedness);
        }

        protected void ScanEdges(Gradients gradients, Edge a, Edge b,
                                 bool handedness)
        {
            Edge left = a;
            Edge right = b;

            if (handedness)
            {
                Edge temp = left;
                left = right;
                right = temp;
            }

            int yStart = b.YStart;
            int yEnd = b.YEnd;

            for (int j = yStart; j < yEnd; ++j)
            {
                DrawScanLine(gradients, left, right, j);
                left.Step();
                right.Step();
            }
        }

        protected void DrawScanLine(Gradients gradients, Edge left, Edge right,
                                    int j)
        {
            int xMin = (int)Math.Ceiling(left.X);
            int xMax = (int)Math.Ceiling(right.X);
            float xPrestep = xMin - left.X;
            Vector4 minColor = left.Color + gradients.ColorXStep * xPrestep;
            Vector4 maxColor = right.Color + gradients.ColorXStep * xPrestep;

            float lerpAmt = 0;
            float lerpStep = 1.0f / (xMax - xMin);
            for (int i = xMin; i < xMax; ++i)
            {
                Vector4 color = Vector4.Lerp(minColor, maxColor, lerpAmt);

                byte r = (byte)(color.X * 255 + 0.5f);
                byte g = (byte)(color.Y * 255 + 0.5f);
                byte b = (byte)(color.Z * 255 + 0.5f);

                DrawPixel(i, j, 0xFF, b, g, r);
                lerpAmt += lerpStep;
            }
        }
    }
}
