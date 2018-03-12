﻿// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

namespace SoftwareRenderer
{
    public class Vertex
    {
        protected Vector4 pos;

        public float X { get { return pos.X; } }
        public float Y { get { return pos.Y; } }

        public Vertex(float x, float y, float z)
        {
            pos = new Vector4(x, y, z, 1);
        }

        public Vertex(Vector4 pos)
        {
            this.pos = pos;
        }

        public Vertex Transform(Matrix4 transform)
        {
            return new Vertex(Vector4.Transform(pos, transform));
        }

        public Vertex PerspectiveDivide()
        {
            return new Vertex(
                new Vector4(pos.X / pos.W, pos.Y / pos.W, pos.Z / pos.W, pos.W)
            );
        }

        public float TriangleAreaTimesTwo(Vertex b, Vertex c)
        {
            float x1 = b.X - X;
            float y1 = b.Y - Y;

            float x2 = c.X - X;
            float y2 = c.Y - Y;

            return x1 * y2 - x2 * y1;
        }
    }
}
