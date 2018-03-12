// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

namespace SoftwareRenderer
{
    public class Vertex
    {
        public float X, Y;

        public Vertex(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float TriangleArea(Vertex b, Vertex c)
        {
            float x1 = b.X - X;
            float y1 = b.Y - Y;

            float x2 = c.X - X;
            float y2 = c.Y - Y;

            return x1 * y2 - x2 * y1;
        }
    }
}
