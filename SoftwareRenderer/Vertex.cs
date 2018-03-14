// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using OpenTK;

public class Vertex
{
    protected Vector4 pos;
    protected Vector4 texCoords;
    protected Vector4 normal;

    public float X { get { return pos.X; } }
    public float Y { get { return pos.Y; } }
    public Vector4 Position { get { return pos; } }
    public Vector4 TexCoords { get { return texCoords; } }
    public Vector4 Normal { get { return normal; } }

    public Vertex(Vector4 pos, Vector4 texCoords, Vector4 normal)
    {
        this.pos = pos;
        this.texCoords = texCoords;
        this.normal = normal;
    }

    public Vertex Transform(Matrix4 transform, Matrix4 normalTransform)
    {
        return new Vertex(
            Vector4.Transform(pos, transform), texCoords,
            Vector4.Transform(normal, normalTransform)
        );
    }

    public Vertex PerspectiveDivide()
    {
        return
            new Vertex(new Vector4(pos.Xyz / pos.W, pos.W), texCoords, normal);
    }

    public float TriangleAreaTimesTwo(Vertex b, Vertex c)
    {
        float x1 = b.X - X;
        float y1 = b.Y - Y;

        float x2 = c.X - X;
        float y2 = c.Y - Y;

        return x1 * y2 - x2 * y1;
    }

    public Vertex Lerp(Vertex other, float lerpAmt)
    {
        return new Vertex(
            Vector4.Lerp(pos, other.Position, lerpAmt),
            Vector4.Lerp(texCoords, other.TexCoords, lerpAmt),
            Vector4.Lerp(normal, other.Normal, lerpAmt)
        );
    }

    public bool IsInsideViewFrustum()
    {
        return Math.Abs(pos.X) <= Math.Abs(pos.W) &&
               Math.Abs(pos.Y) <= Math.Abs(pos.W) &&
               Math.Abs(pos.Z) <= Math.Abs(pos.W);
    }

    public float this[int index]
    {
        get { return pos[index]; }
        set { pos[index] = value; }
    }
}
