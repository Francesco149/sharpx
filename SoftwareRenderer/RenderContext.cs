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
            Edge topToBottom = new Edge(minYVert, maxYVert);
            Edge topToMiddle = new Edge(minYVert, midYVert);
            Edge middleToBottom = new Edge(midYVert, maxYVert);

            ScanEdges(topToBottom, topToMiddle, handedness);
            ScanEdges(topToBottom, middleToBottom, handedness);
        }

        protected void ScanEdges(Edge a, Edge b, bool handedness)
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
                DrawScanLine(left, right, j);
                left.Step();
                right.Step();
            }
        }

        protected void DrawScanLine(Edge left, Edge right, int j)
        {
            int xMin = (int)Math.Ceiling(left.X);
            int xMax = (int)Math.Ceiling(right.X);

            for (int i = xMin; i < xMax; ++i)
            {
                DrawPixel(i, j, 0xFF, 0xFF, 0xFF, 0xFF);
            }
        }
    }
}
