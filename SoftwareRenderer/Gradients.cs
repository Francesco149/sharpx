// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

namespace SoftwareRenderer
{
    public class Gradients
    {
        public float[] TexCoordX { get; }
        public float[] TexCoordY { get; }
        public float[] OneOverZ { get; }
        public float[] Depth { get; }
        public float[] LightAmt { get; }

        public float TexCoordXXStep { get; }
        public float TexCoordXYStep { get; }
        public float TexCoordYXStep { get; }
        public float TexCoordYYStep { get; }
        public float OneOverZXStep { get; }
        public float OneOverZYStep { get; }
        public float DepthXStep { get; }
        public float DepthYStep { get; }
        public float LightAmtXStep { get; }
        public float LightAmtYStep { get; }

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
            TexCoordX = new float[3];
            TexCoordY = new float[3];
            OneOverZ = new float[3];
            Depth = new float[3];
            LightAmt = new float[3];

            OneOverZ[0] = 1.0f / minYVert.Position.W;
            OneOverZ[1] = 1.0f / midYVert.Position.W;
            OneOverZ[2] = 1.0f / maxYVert.Position.W;

            TexCoordX[0] = minYVert.TexCoords.X * OneOverZ[0];
            TexCoordX[1] = midYVert.TexCoords.X * OneOverZ[1];
            TexCoordX[2] = maxYVert.TexCoords.X * OneOverZ[2];

            TexCoordY[0] = minYVert.TexCoords.Y * OneOverZ[0];
            TexCoordY[1] = midYVert.TexCoords.Y * OneOverZ[1];
            TexCoordY[2] = maxYVert.TexCoords.Y * OneOverZ[2];

            Depth[0] = minYVert.Position.Z;
            Depth[1] = midYVert.Position.Z;
            Depth[2] = maxYVert.Position.Z;

            Vector4 lightDir = new Vector4(0, 0, 1, 0);
            LightAmt[0] =
                Saturate(Vector4.Dot(minYVert.Normal, lightDir)) * 0.9f + 0.1f;
            LightAmt[1] =
                Saturate(Vector4.Dot(midYVert.Normal, lightDir)) * 0.9f + 0.1f;
            LightAmt[2] =
                Saturate(Vector4.Dot(maxYVert.Normal, lightDir)) * 0.9f + 0.1f;

            float oneOverdX =
                1.0f /
                ((midYVert.X - maxYVert.X) * (minYVert.Y - maxYVert.Y) -
                 (minYVert.X - maxYVert.X) * (midYVert.Y - maxYVert.Y));

            float oneOverdY = -oneOverdX;

            TexCoordXXStep =
                CalcXStep(TexCoordX, minYVert, midYVert, maxYVert, oneOverdX);
            TexCoordXYStep =
                CalcYStep(TexCoordX, minYVert, midYVert, maxYVert, oneOverdY);

            TexCoordYXStep =
                CalcXStep(TexCoordY, minYVert, midYVert, maxYVert, oneOverdX);
            TexCoordYYStep =
                CalcYStep(TexCoordY, minYVert, midYVert, maxYVert, oneOverdY);

            OneOverZXStep =
                CalcXStep(OneOverZ, minYVert, midYVert, maxYVert, oneOverdX);
            OneOverZYStep =
                CalcYStep(OneOverZ, minYVert, midYVert, maxYVert, oneOverdY);

            DepthXStep =
                CalcXStep(Depth, minYVert, midYVert, maxYVert, oneOverdX);
            DepthYStep =
                CalcYStep(Depth, minYVert, midYVert, maxYVert, oneOverdY);

            LightAmtXStep =
                CalcXStep(LightAmt, minYVert, midYVert, maxYVert, oneOverdX);
            LightAmtYStep =
                CalcYStep(LightAmt, minYVert, midYVert, maxYVert, oneOverdY);
        }
    }
}
