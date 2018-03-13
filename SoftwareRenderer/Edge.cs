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
        protected float texCoordX;
        protected float texCoordXStep;
        protected float texCoordY;
        protected float texCoordYStep;
        protected float oneOverZ;
        protected float oneOverZStep;

        public float X { get { return x; } }
        public int YStart { get { return yStart; } }
        public int YEnd { get { return yEnd; } }
        public float TexCoordX { get { return texCoordX; } }
        public float TexCoordY { get { return texCoordY; } }
        public float OneOverZ { get { return oneOverZ; } }

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

            texCoordX = gradients.TexCoordX[minYVertIndex] +
                        gradients.TexCoordXYStep * yPrestep +
                        gradients.TexCoordXXStep * xPrestep;
            texCoordXStep = gradients.TexCoordXYStep +
                            gradients.TexCoordXXStep * xStep;

            texCoordY = gradients.TexCoordY[minYVertIndex] +
                        gradients.TexCoordYYStep * yPrestep +
                        gradients.TexCoordYXStep * xPrestep;
            texCoordYStep = gradients.TexCoordYYStep +
                            gradients.TexCoordYXStep * xStep;

            oneOverZ = gradients.OneOverZ[minYVertIndex] +
                       gradients.OneOverZYStep * yPrestep +
                       gradients.OneOverZXStep * xPrestep;
            oneOverZStep = gradients.OneOverZYStep +
                           gradients.OneOverZXStep * xStep;
        }

        public void Step()
        {
            x += xStep;
            texCoordX += texCoordXStep;
            texCoordY += texCoordYStep;
            oneOverZ += oneOverZStep;
        }
    }
}
