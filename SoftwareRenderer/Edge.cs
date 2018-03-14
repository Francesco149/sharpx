// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;

namespace SoftwareRenderer
{
    public class Edge
    {
        protected float xStep;
        protected float texCoordXStep;
        protected float texCoordYStep;
        protected float oneOverZStep;
        protected float depthStep;
        protected float lightAmtStep;

        public float X { get; protected set; }
        public int YStart { get; protected set; }
        public int YEnd { get; protected set; }
        public float TexCoordX { get; protected set; }
        public float TexCoordY { get; protected set; }
        public float OneOverZ { get; protected set; }
        public float Depth { get; protected set; }
        public float LightAmt { get; protected set; }

        public Edge(Gradients gradients, Vertex minYVert, Vertex maxYVert,
                   int minYVertIndex)
        {
            YStart = (int)Math.Ceiling(minYVert.Y);
            YEnd = (int)Math.Ceiling(maxYVert.Y);

            float yDist = maxYVert.Y - minYVert.Y;
            float xDist = maxYVert.X - minYVert.X;

            float yPrestep = YStart - minYVert.Y;
            xStep = xDist / yDist;
            X = minYVert.X + yPrestep * xStep;
            float xPrestep = X - minYVert.X;

            TexCoordX = gradients.TexCoordX[minYVertIndex] +
                        gradients.TexCoordXYStep * yPrestep +
                        gradients.TexCoordXXStep * xPrestep;
            texCoordXStep = gradients.TexCoordXYStep +
                            gradients.TexCoordXXStep * xStep;

            TexCoordY = gradients.TexCoordY[minYVertIndex] +
                        gradients.TexCoordYYStep * yPrestep +
                        gradients.TexCoordYXStep * xPrestep;
            texCoordYStep = gradients.TexCoordYYStep +
                            gradients.TexCoordYXStep * xStep;

            OneOverZ = gradients.OneOverZ[minYVertIndex] +
                       gradients.OneOverZYStep * yPrestep +
                       gradients.OneOverZXStep * xPrestep;
            oneOverZStep = gradients.OneOverZYStep +
                           gradients.OneOverZXStep * xStep;

            Depth = gradients.Depth[minYVertIndex] +
                    gradients.DepthYStep * yPrestep +
                    gradients.DepthXStep * xPrestep;
            depthStep = gradients.DepthYStep +
                        gradients.DepthXStep * xStep;

            LightAmt = gradients.LightAmt[minYVertIndex] +
                       gradients.LightAmtYStep * yPrestep +
                       gradients.LightAmtXStep * xPrestep;
            lightAmtStep = gradients.LightAmtYStep +
                           gradients.LightAmtXStep * xStep;
        }

        public void Step()
        {
            X += xStep;
            TexCoordX += texCoordXStep;
            TexCoordY += texCoordYStep;
            OneOverZ += oneOverZStep;
            Depth += depthStep;
            LightAmt += lightAmtStep;
        }
    }
}
