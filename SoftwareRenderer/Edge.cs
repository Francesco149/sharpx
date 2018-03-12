﻿// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;

namespace SoftwareRenderer
{
    public class Edge
    {
        protected float x;
        protected float xStep;
        protected int yStart;
        protected int yEnd;

        public float X { get { return x; } }
        public int YStart { get { return yStart; } }
        public int YEnd { get { return yEnd; } }

        public Edge(Vertex minYVert, Vertex maxYVert)
        {
            yStart = (int)Math.Ceiling(minYVert.Y);
            yEnd = (int)Math.Ceiling(maxYVert.Y);

            float yDist = maxYVert.Y - minYVert.Y;
            float xDist = maxYVert.X - minYVert.X;

            float yPrestep = yStart - minYVert.Y;
            xStep = xDist / yDist;
            x = minYVert.X + yPrestep * xStep;
        }

        public void Step()
        {
            x += xStep;
        }
    }
}
