// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

namespace SoftwareRenderer
{
    public class Gradients
    {
        protected Vector4[] color;
        protected Vector4 colorXStep;
        protected Vector4 colorYStep;

        public Vector4[] Color { get { return color; } }
        public Vector4 ColorXStep { get { return colorXStep; } }
        public Vector4 ColorYStep { get { return colorYStep; } }

        public Gradients(Vertex minYVert, Vertex midYVert, Vertex maxYVert)
        {
            color = new Vector4[3];

            color[0] = minYVert.Color;
            color[1] = midYVert.Color;
            color[2] = maxYVert.Color;

            float oneOverdX =
                1.0f /
                ((midYVert.X - maxYVert.X) * (minYVert.Y - maxYVert.Y) -
                 (minYVert.X - maxYVert.X) * (midYVert.Y - maxYVert.Y));

            float oneOverdY = -oneOverdX;

            colorXStep =
                ((color[1] - color[2]) * (minYVert.Y - maxYVert.Y) -
                 (color[0] - color[2]) * (midYVert.Y - maxYVert.Y))
                * oneOverdX;

            colorYStep =
                ((color[1] - color[2]) * (minYVert.X - maxYVert.X) -
                 (color[0] - color[2]) * (midYVert.X - maxYVert.X))
                * oneOverdY;
        }
    }
}
