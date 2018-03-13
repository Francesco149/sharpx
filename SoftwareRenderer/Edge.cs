// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using OpenTK;

namespace SoftwareRenderer
{
    public class Edge
    {
        protected float x;
        protected float xStep;
        protected int yStart;
        protected int yEnd;
        protected Vector4 color;
        protected Vector4 colorStep;

        public float X { get { return x; } }
        public int YStart { get { return yStart; } }
        public int YEnd { get { return yEnd; } }
        public Vector4 Color { get { return color; } }

        public Edge(Gradients gradients, Vertex minYVert, Vertex maxYVert,
                   int minYVertIndex)
        {
            yStart = (int)Math.Ceiling(minYVert.Y);
            yEnd = (int)Math.Ceiling(maxYVert.Y);

            float yDist = maxYVert.Y - minYVert.Y;
            float xDist = maxYVert.X - minYVert.X;

            float yPrestep = yStart - minYVert.Y;
            xStep = xDist / yDist;
            x = minYVert.X + yPrestep * xStep;
            float xPrestep = x - minYVert.X;

            color = gradients.Color[minYVertIndex] +
                    gradients.ColorYStep * yPrestep +
                    gradients.ColorXStep * xPrestep;
            colorStep = gradients.ColorYStep + gradients.ColorXStep * xStep;
        }

        public void Step()
        {
            x += xStep;
            color += colorStep;
        }
    }
}
