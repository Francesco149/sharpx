// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

namespace SoftwareRenderer
{
    public class Gradients
    {
        protected float[] texCoordX;
        protected float[] texCoordY;
        protected float[] oneOverZ;

        protected float texCoordXXStep;
        protected float texCoordXYStep;
        protected float texCoordYXStep;
        protected float texCoordYYStep;
        protected float oneOverZXStep;
        protected float oneOverZYStep;

        public float[] TexCoordX { get { return texCoordX; } }
        public float[] TexCoordY { get { return texCoordY; } }
        public float[] OneOverZ { get { return oneOverZ; } }

        public float TexCoordXXStep { get { return texCoordXXStep; } }
        public float TexCoordXYStep { get { return texCoordXYStep; } }
        public float TexCoordYXStep { get { return texCoordYXStep; } }
        public float TexCoordYYStep { get { return texCoordYYStep; } }
        public float OneOverZXStep { get { return oneOverZXStep; } }
        public float OneOverZYStep { get { return oneOverZYStep; } }

        protected float CalcXStep(float[] values, Vertex minYVert,
                                  Vertex midYVert, Vertex maxYVert,
                                  float oneOverdX)
        {
            return
                ((values[1] - values[2]) * (minYVert.Y - maxYVert.Y) -
                (values[0] - values[2]) * (midYVert.Y - maxYVert.Y))
                * oneOverdX;
        }

        protected float CalcYStep(float[] values, Vertex minYVert,
                                  Vertex midYVert, Vertex maxYVert,
                                  float oneOverdY)
        {
            return
                ((values[1] - values[2]) * (minYVert.X - maxYVert.X) -
                (values[0] - values[2]) * (midYVert.X - maxYVert.X))
                * oneOverdY;
        }

        public Gradients(Vertex minYVert, Vertex midYVert, Vertex maxYVert)
        {
            texCoordX = new float[3];
            texCoordY = new float[3];
            oneOverZ = new float[3];

            oneOverZ[0] = 1.0f / minYVert.Position.W;
            oneOverZ[1] = 1.0f / midYVert.Position.W;
            oneOverZ[2] = 1.0f / maxYVert.Position.W;

            texCoordX[0] = minYVert.TexCoords.X * oneOverZ[0];
            texCoordX[1] = midYVert.TexCoords.X * oneOverZ[1];
            texCoordX[2] = maxYVert.TexCoords.X * oneOverZ[2];

            texCoordY[0] = minYVert.TexCoords.Y * oneOverZ[0];
            texCoordY[1] = midYVert.TexCoords.Y * oneOverZ[1];
            texCoordY[2] = maxYVert.TexCoords.Y * oneOverZ[2];

            float oneOverdX =
                1.0f /
                ((midYVert.X - maxYVert.X) * (minYVert.Y - maxYVert.Y) -
                 (minYVert.X - maxYVert.X) * (midYVert.Y - maxYVert.Y));

            float oneOverdY = -oneOverdX;

            texCoordXXStep =
                CalcXStep(texCoordX, minYVert, midYVert, maxYVert, oneOverdX);
            texCoordXYStep =
                CalcYStep(texCoordX, minYVert, midYVert, maxYVert, oneOverdY);

            texCoordYXStep =
                CalcXStep(texCoordY, minYVert, midYVert, maxYVert, oneOverdX);
            texCoordYYStep =
                CalcYStep(texCoordY, minYVert, midYVert, maxYVert, oneOverdY);

            oneOverZXStep =
                CalcXStep(oneOverZ, minYVert, midYVert, maxYVert, oneOverdX);
            oneOverZYStep =
                CalcYStep(oneOverZ, minYVert, midYVert, maxYVert, oneOverdY);
        }
    }
}
