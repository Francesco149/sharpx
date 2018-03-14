// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

public class Gradients
{
    public float[] TexU { get; } = new float[3];
    public float[] TexV { get; } = new float[3];
    public float[] OneOvrZ { get; } = new float[3];
    public float[] Depth { get; } = new float[3];
    public float[] Light { get; } = new float[3];

    public float TexUXStep { get; }
    public float TexUYStep { get; }
    public float TexVXStep { get; }
    public float TexVYStep { get; }
    public float OneOvrZXStep { get; }
    public float OneOvrZYStep { get; }
    public float DepthXStep { get; }
    public float DepthYStep { get; }
    public float LightXStep { get; }
    public float LightYStep { get; }

    protected float XStep(float[] values, Vertex minYVert, Vertex midYVert,
        Vertex maxYVert, float oneOverdX)
    {
        return (
            (values[1] - values[2]) * (minYVert.Y - maxYVert.Y) -
            (values[0] - values[2]) * (midYVert.Y - maxYVert.Y)
        ) * oneOverdX;
    }

    protected float YStep(float[] values, Vertex minYVert, Vertex midYVert,
        Vertex maxYVert, float oneOverdY)
    {
        return (
            (values[1] - values[2]) * (minYVert.X - maxYVert.X) -
            (values[0] - values[2]) * (midYVert.X - maxYVert.X)
        ) * oneOverdY;
    }

    protected float Saturate(float val) { return MathHelper.Clamp(val, 0, 1); }

    public Gradients(Vertex minYVert, Vertex midYVert, Vertex maxYVert)
    {
        OneOvrZ[0] = 1.0f / minYVert.Position.W;
        OneOvrZ[1] = 1.0f / midYVert.Position.W;
        OneOvrZ[2] = 1.0f / maxYVert.Position.W;

        TexU[0] = minYVert.TexCoords.X * OneOvrZ[0];
        TexU[1] = midYVert.TexCoords.X * OneOvrZ[1];
        TexU[2] = maxYVert.TexCoords.X * OneOvrZ[2];

        TexV[0] = minYVert.TexCoords.Y * OneOvrZ[0];
        TexV[1] = midYVert.TexCoords.Y * OneOvrZ[1];
        TexV[2] = maxYVert.TexCoords.Y * OneOvrZ[2];

        Depth[0] = minYVert.Position.Z;
        Depth[1] = midYVert.Position.Z;
        Depth[2] = maxYVert.Position.Z;

        Vector4 lightDir = new Vector4(0, 0, 1, 0);
        Light[0] = Saturate(Vector4.Dot(minYVert.Normal, lightDir)) * .9f + .1f;
        Light[1] = Saturate(Vector4.Dot(midYVert.Normal, lightDir)) * .9f + .1f;
        Light[2] = Saturate(Vector4.Dot(maxYVert.Normal, lightDir)) * .9f + .1f;

        float oneOvrdX =
            1.0f / (
                (midYVert.X - maxYVert.X) * (minYVert.Y - maxYVert.Y) -
                (minYVert.X - maxYVert.X) * (midYVert.Y - maxYVert.Y)
            );

        float oneOverdY = -oneOvrdX;

        TexUXStep = XStep(TexU, minYVert, midYVert, maxYVert, oneOvrdX);
        TexUYStep = YStep(TexU, minYVert, midYVert, maxYVert, oneOverdY);

        TexVXStep = XStep(TexV, minYVert, midYVert, maxYVert, oneOvrdX);
        TexVYStep = YStep(TexV, minYVert, midYVert, maxYVert, oneOverdY);

        OneOvrZXStep = XStep(OneOvrZ, minYVert, midYVert, maxYVert, oneOvrdX);
        OneOvrZYStep = YStep(OneOvrZ, minYVert, midYVert, maxYVert, oneOverdY);

        DepthXStep = XStep(Depth, minYVert, midYVert, maxYVert, oneOvrdX);
        DepthYStep = YStep(Depth, minYVert, midYVert, maxYVert, oneOverdY);

        LightXStep = XStep(Light, minYVert, midYVert, maxYVert, oneOvrdX);
        LightYStep = YStep(Light, minYVert, midYVert, maxYVert, oneOverdY);
    }
}
