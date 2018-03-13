// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

namespace SoftwareRenderer
{
    public class Gradients
    {
        protected float[] texCoordX;
        protected float[] texCoordY;

        protected float texCoordXXStep;
        protected float texCoordXYStep;
        protected float texCoordYXStep;
        protected float texCoordYYStep;

        public float[] TexCoordX { get { return texCoordX; } }
        public float[] TexCoordY { get { return texCoordY; } }

        public float TexCoordXXStep { get { return texCoordXXStep; } }
        public float TexCoordXYStep { get { return texCoordXYStep; } }
        public float TexCoordYXStep { get { return texCoordYXStep; } }
        public float TexCoordYYStep { get { return texCoordYYStep; } }

        public Gradients(Vertex minYVert, Vertex midYVert, Vertex maxYVert)
        {
            texCoordX = new float[3];
            texCoordY = new float[3];

            texCoordX[0] = minYVert.TexCoords.X;
            texCoordX[1] = midYVert.TexCoords.X;
            texCoordX[2] = maxYVert.TexCoords.X;

            texCoordY[0] = minYVert.TexCoords.Y;
            texCoordY[1] = midYVert.TexCoords.Y;
            texCoordY[2] = maxYVert.TexCoords.Y;

            float oneOverdX =
                1.0f /
                ((midYVert.X - maxYVert.X) * (minYVert.Y - maxYVert.Y) -
                 (minYVert.X - maxYVert.X) * (midYVert.Y - maxYVert.Y));

            float oneOverdY = -oneOverdX;

            texCoordXXStep =
                ((texCoordX[1] - texCoordX[2]) * (minYVert.Y - maxYVert.Y) -
                 (texCoordX[0] - texCoordX[2]) * (midYVert.Y - maxYVert.Y))
                * oneOverdX;

            texCoordXYStep =
                ((texCoordX[1] - texCoordX[2]) * (minYVert.X - maxYVert.X) -
                 (texCoordX[0] - texCoordX[2]) * (midYVert.X - maxYVert.X))
                * oneOverdY;

            texCoordYXStep =
                ((texCoordY[1] - texCoordY[2]) * (minYVert.Y - maxYVert.Y) -
                 (texCoordY[0] - texCoordY[2]) * (midYVert.Y - maxYVert.Y))
                * oneOverdX;

            texCoordYYStep =
                ((texCoordY[1] - texCoordY[2]) * (minYVert.X - maxYVert.X) -
                 (texCoordY[0] - texCoordY[2]) * (midYVert.X - maxYVert.X))
                * oneOverdY;
        }
    }
}
