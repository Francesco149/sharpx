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
        protected float[] depth;
        protected float[] lightAmt;

        protected float texCoordXXStep;
        protected float texCoordXYStep;
        protected float texCoordYXStep;
        protected float texCoordYYStep;
        protected float oneOverZXStep;
        protected float oneOverZYStep;
        protected float depthXStep;
        protected float depthYStep;
        protected float lightAmtXStep;
        protected float lightAmtYStep;

        public float[] TexCoordX { get { return texCoordX; } }
        public float[] TexCoordY { get { return texCoordY; } }
        public float[] OneOverZ { get { return oneOverZ; } }
        public float[] Depth { get { return depth; } }
        public float[] LightAmt { get { return lightAmt; } }

        public float TexCoordXXStep { get { return texCoordXXStep; } }
        public float TexCoordXYStep { get { return texCoordXYStep; } }
        public float TexCoordYXStep { get { return texCoordYXStep; } }
        public float TexCoordYYStep { get { return texCoordYYStep; } }
        public float OneOverZXStep { get { return oneOverZXStep; } }
        public float OneOverZYStep { get { return oneOverZYStep; } }
        public float DepthXStep { get { return depthXStep; } }
        public float DepthYStep { get { return depthYStep; } }
        public float LightAmtXStep { get { return lightAmtXStep; } }
        public float LightAmtYStep { get { return lightAmtYStep; } }

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

        protected float Saturate(float val)
        {
            if (val < 0)
            {
                return 0;
            }
            if (val > 1)
            {
                return 1;
            }
            return val;
        }

        public Gradients(Vertex minYVert, Vertex midYVert, Vertex maxYVert)
        {
            texCoordX = new float[3];
            texCoordY = new float[3];
            oneOverZ = new float[3];
            depth = new float[3];
            lightAmt = new float[3];

            oneOverZ[0] = 1.0f / minYVert.Position.W;
            oneOverZ[1] = 1.0f / midYVert.Position.W;
            oneOverZ[2] = 1.0f / maxYVert.Position.W;

            texCoordX[0] = minYVert.TexCoords.X * oneOverZ[0];
            texCoordX[1] = midYVert.TexCoords.X * oneOverZ[1];
            texCoordX[2] = maxYVert.TexCoords.X * oneOverZ[2];

            texCoordY[0] = minYVert.TexCoords.Y * oneOverZ[0];
            texCoordY[1] = midYVert.TexCoords.Y * oneOverZ[1];
            texCoordY[2] = maxYVert.TexCoords.Y * oneOverZ[2];

            depth[0] = minYVert.Position.Z;
            depth[1] = midYVert.Position.Z;
            depth[2] = maxYVert.Position.Z;

            Vector4 lightDir = new Vector4(0, 0, 1, 0);
            lightAmt[0] =
                Saturate(Vector4.Dot(minYVert.Normal, lightDir)) * 0.9f + 0.1f;
            lightAmt[1] =
                Saturate(Vector4.Dot(midYVert.Normal, lightDir)) * 0.9f + 0.1f;
            lightAmt[2] =
                Saturate(Vector4.Dot(maxYVert.Normal, lightDir)) * 0.9f + 0.1f;

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

            depthXStep =
                CalcXStep(depth, minYVert, midYVert, maxYVert, oneOverdX);
            depthYStep =
                CalcYStep(depth, minYVert, midYVert, maxYVert, oneOverdY);

            lightAmtXStep =
                CalcXStep(lightAmt, minYVert, midYVert, maxYVert, oneOverdX);
            lightAmtYStep =
                CalcYStep(lightAmt, minYVert, midYVert, maxYVert, oneOverdY);
        }
    }
}
