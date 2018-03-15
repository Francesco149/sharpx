// C# implementation of [thebennybox's software renderer][1] in < 1000 LOC
//
// ![](https://i.imgur.com/8hKZiP0.png)
//
// to celebrate benny's return I decided to go through his tutorials again
//
// you can checkout individual lessons with ```git checkout lesson-12```
//
// lesson branches are kept as close as possible to benny's code, the master
// branch is heavily refactored to my own coding style
//
// no particular reason why I used C#, I was just showing a friend how to use
// OpenTK to get started with the series and ended up playing with it myself
//
// compile and run with
// ```sh
// msbuild -m -p:Configuration=Release
// mono --aot bin/Release/SoftwareRenderer.exe
// mono bin/Release/SoftwareRenderer.exe
// ```
//
// # license
// this is free and unencumbered software released into the internal domain.
// see the attached UNLICENSE or http://unlicense.org/
//
// [1]: https://www.youtube.com/playlist?list=PLEETnX-uPtBUbVOok816vTl1K9vV1GgH5

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL; // only used to blit the frame buffer to a window
using OpenTK.Input;

/* -------------------------------------------------------------------------- */

static
class Matrix4Utils
{
    internal static
    Matrix4 ScreenSpace(float halfWidth, float halfHeight)
    {
        return new Matrix4(
            halfWidth, 0, 0, 0,
            0, -halfHeight, 0, 0,
            0, 0, 1, 0,
            halfWidth - 0.5f, halfHeight - 0.5f, 0, 1
        );
    }

    internal static
    Matrix4 Perspective(float fov, float aspect, float zNear, float zFar)
    {
        float tanHalfFOV = (float)Math.Tan(fov / 2);
        float zRange = zNear - zFar;

        return new Matrix4(
            1 / (tanHalfFOV * aspect), 0, 0, 0,
            0, 1 / tanHalfFOV, 0, 0,
            0, 0, (-zNear - zFar) / zRange, 1,
            0, 0, 2 * zFar * zNear / zRange, 0
        );
    }
}

/* -------------------------------------------------------------------------- */

class Transform
{
    internal Vector4 Position { get; }
    internal Quaternion Rotation { get; }
    internal Vector4 Scale { get; }

    internal
    Transform(Vector4? pos = null, Quaternion? rot = null,
        Vector4? scale = null)
    {
        Position = pos ?? new Vector4(0, 0, 0, 1);
        Rotation = rot ?? new Quaternion(0, 0, 0, 1);
        Scale = scale ?? new Vector4(1, 1, 1, 1);
    }

    internal
    Transform SetPos(Vector4 newPos)
    {
        return new Transform(newPos, Rotation, Scale);
    }

    internal
    Transform Rotate(Quaternion newRot)
    {
        return new Transform(Position, (newRot * Rotation).Normalized(), Scale);
    }

    internal
    Matrix4 Matrix
    {
        get
        {
            Vector4 pos = Position;
            return Matrix4.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                   Matrix4.CreateFromQuaternion(Rotation) *
                   Matrix4.CreateTranslation(pos.X, pos.Y, pos.Z);
        }
    }
}

/* -------------------------------------------------------------------------- */

class Bitmap
{
    internal int Width { get; }
    internal int Height { get; }
    internal byte[] Bytes { get; }

    internal
    Bitmap(int width, int height)
    {
        Width = width;
        Height = height;
        Bytes = new byte[width * height * 3];
    }

    internal
    Bitmap(string fileName)
    {
        var image = new System.Drawing.Bitmap(fileName);
        Width = image.Width;
        Height = image.Height;
        Bytes = new byte[Width * Height * 3];

        for (int j = 0; j < Height; ++j)
        {
            for (int i = 0; i < Width; ++i)
            {
                Color pixel = image.GetPixel(i, j);

                Bytes[(j * Width + i) * 3 + 0] = pixel.B;
                Bytes[(j * Width + i) * 3 + 1] = pixel.G;
                Bytes[(j * Width + i) * 3 + 2] = pixel.R;
            }
        }
    }

    internal
    void Clear(byte shade)
    {
        for (int i = 0; i < Bytes.Length; ++i)
            Bytes[i] = shade;
    }

    protected
    void CopyPixel(int destX, int destY, int srceX, int srceY,
        Bitmap srce, float light)
    {
        destY = Height - destY - 1; // OpenGL's DrawPixels flips images
        int dstIndex = (destX + destY * this.Width) * 3;
        int srcIndex = (srceX + srceY * srce.Width) * 3;
        Bytes[dstIndex + 0] = (byte)(light * srce.Bytes[srcIndex + 0]);
        Bytes[dstIndex + 1] = (byte)(light * srce.Bytes[srcIndex + 1]);
        Bytes[dstIndex + 2] = (byte)(light * srce.Bytes[srcIndex + 2]);
    }
}

/* -------------------------------------------------------------------------- */

class Vertex
{
    internal float X { get { return Position.X; } }
    internal float Y { get { return Position.Y; } }
    internal Vector4 Position;
    internal Vector4 TexCoords { get; }
    internal Vector4 Normal { get; }

    internal
    float this[int index]
    {
        get { return Position[index]; }
        set { Position[index] = value; }
    }

    internal
    Vertex(Vector4 pos, Vector4 texCoords, Vector4 normal)
    {
        Position = pos;
        TexCoords = texCoords;
        Normal = normal;
    }

    internal
    Vertex Transform(Matrix4 transform, Matrix4 normalTransform)
    {
        return new Vertex(
            Vector4.Transform(Position, transform),
            TexCoords,
            Vector4.Transform(Normal, normalTransform)
        );
    }

    internal
    Vertex PerspectiveDivide()
    {
        var dividedPos = new Vector4(Position.Xyz / Position.W, Position.W);
        return new Vertex(dividedPos, TexCoords, Normal);
    }

    internal
    float TriangleAreaTimesTwo(Vertex b, Vertex c)
    {
        float x1 = b.X - X;
        float y1 = b.Y - Y;
        float x2 = c.X - X;
        float y2 = c.Y - Y;
        return x1 * y2 - x2 * y1;
    }

    internal
    Vertex Lerp(Vertex other, float lerpAmt)
    {
        return new Vertex(
            Vector4.Lerp(Position, other.Position, lerpAmt),
            Vector4.Lerp(TexCoords, other.TexCoords, lerpAmt),
            Vector4.Lerp(Normal, other.Normal, lerpAmt)
        );
    }

    internal
    bool IsInsideViewFrustum()
    {
        return Math.Abs(Position.X) <= Math.Abs(Position.W) &&
               Math.Abs(Position.Y) <= Math.Abs(Position.W) &&
               Math.Abs(Position.Z) <= Math.Abs(Position.W);
    }
}

/* -------------------------------------------------------------------------- */

class Gradients
{
    internal float[] TexU { get; } = new float[3];
    internal float[] TexV { get; } = new float[3];
    internal float[] OneOvrZ { get; } = new float[3];
    internal float[] Depth { get; } = new float[3];
    internal float[] Light { get; } = new float[3];

    internal float TexUXStep { get; }
    internal float TexUYStep { get; }
    internal float TexVXStep { get; }
    internal float TexVYStep { get; }
    internal float OneOvrZXStep { get; }
    internal float OneOvrZYStep { get; }
    internal float DepthXStep { get; }
    internal float DepthYStep { get; }
    internal float LightXStep { get; }
    internal float LightYStep { get; }

    internal
    Gradients(Vertex minYVert, Vertex midYVert, Vertex maxYVert)
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

        float oneOvrdY = -oneOvrdX;

        TexUXStep = XStep(TexU, minYVert, midYVert, maxYVert, oneOvrdX);
        TexUYStep = YStep(TexU, minYVert, midYVert, maxYVert, oneOvrdY);

        TexVXStep = XStep(TexV, minYVert, midYVert, maxYVert, oneOvrdX);
        TexVYStep = YStep(TexV, minYVert, midYVert, maxYVert, oneOvrdY);

        OneOvrZXStep = XStep(OneOvrZ, minYVert, midYVert, maxYVert, oneOvrdX);
        OneOvrZYStep = YStep(OneOvrZ, minYVert, midYVert, maxYVert, oneOvrdY);

        DepthXStep = XStep(Depth, minYVert, midYVert, maxYVert, oneOvrdX);
        DepthYStep = YStep(Depth, minYVert, midYVert, maxYVert, oneOvrdY);

        LightXStep = XStep(Light, minYVert, midYVert, maxYVert, oneOvrdX);
        LightYStep = YStep(Light, minYVert, midYVert, maxYVert, oneOvrdY);
    }

    float XStep(float[] values, Vertex minYVert, Vertex midYVert,
        Vertex maxYVert, float oneOverdX)
    {
        return (
            (values[1] - values[2]) * (minYVert.Y - maxYVert.Y) -
            (values[0] - values[2]) * (midYVert.Y - maxYVert.Y)
        ) * oneOverdX;
    }

    float YStep(float[] values, Vertex minYVert, Vertex midYVert,
        Vertex maxYVert, float oneOverdY)
    {
        return (
            (values[1] - values[2]) * (minYVert.X - maxYVert.X) -
            (values[0] - values[2]) * (midYVert.X - maxYVert.X)
        ) * oneOverdY;
    }

    float Saturate(float val) { return MathHelper.Clamp(val, 0, 1); }
}

/* -------------------------------------------------------------------------- */

class Edge
{
    readonly float xStep;
    readonly float texUStep;
    readonly float texVStep;
    readonly float oneOvrZStep;
    readonly float depthStep;
    readonly float lightStep;

    internal int YMin { get; }
    internal int YMax { get; }

    internal float X { get; private set; }
    internal float TexU { get; private set; }
    internal float TexV { get; private set; }
    internal float OneOvrZ { get; private set; }
    internal float Depth { get; private set; }
    internal float Light { get; private set; }

    internal
    Edge(Gradients gradients, Vertex minYVert, Vertex maxYVert, int minYVertIdx)
    {
        YMin = (int)Math.Ceiling(minYVert.Y);
        YMax = (int)Math.Ceiling(maxYVert.Y);

        float yDist = maxYVert.Y - minYVert.Y;
        float xDist = maxYVert.X - minYVert.X;

        float yPrestep = YMin - minYVert.Y;
        xStep = xDist / yDist;
        X = minYVert.X + yPrestep * xStep;
        float xPrestep = X - minYVert.X;

        Gradients g = gradients;

        texUStep = g.TexUXStep * xStep + g.TexUYStep;
        TexU =
            g.TexU[minYVertIdx] +
            g.TexUXStep * xPrestep +
            g.TexUYStep * yPrestep;

        texVStep = g.TexVXStep * xStep + g.TexVYStep;
        TexV =
            g.TexV[minYVertIdx] +
            g.TexVXStep * xPrestep +
            g.TexVYStep * yPrestep;

        oneOvrZStep = g.OneOvrZXStep * xStep + g.OneOvrZYStep;
        OneOvrZ =
            g.OneOvrZ[minYVertIdx] +
            g.OneOvrZXStep * xPrestep +
            g.OneOvrZYStep * yPrestep;

        depthStep = g.DepthXStep * xStep + g.DepthYStep;
        Depth =
            g.Depth[minYVertIdx] +
            g.DepthXStep * xPrestep +
            g.DepthYStep * yPrestep;

        lightStep = g.LightXStep * xStep + g.LightYStep;
        Light =
            g.Light[minYVertIdx] +
            g.LightXStep * xPrestep +
            g.LightYStep * yPrestep;
    }

    internal
    void Step()
    {
        X += xStep;
        TexU += texUStep;
        TexV += texVStep;
        OneOvrZ += oneOvrZStep;
        Depth += depthStep;
        Light += lightStep;
    }
}

/* -------------------------------------------------------------------------- */

class RenderContext : Bitmap
{
    readonly float[] zBuffer;

    internal
    RenderContext(int width, int height)
        : base(width, height)
    {
        zBuffer = new float[width * height];
    }

    internal
    void ClearDepthBuffer()
    {
        for (int i = 0; i < zBuffer.Length; ++i)
            zBuffer[i] = float.MaxValue;
    }

    internal
    void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Bitmap texture)
    {
        bool allInside = v1.IsInsideViewFrustum() &&
                         v2.IsInsideViewFrustum() &&
                         v3.IsInsideViewFrustum();

        if (allInside) // lewd
        {
            FillTriangle(v1, v2, v3, texture);
            return;
        }

        var verts = new List<Vertex>();
        var buffer = new List<Vertex>();

        verts.Add(v1);
        verts.Add(v2);
        verts.Add(v3);

        bool anyVertsLeft = ClipPolygonAxis(verts, buffer, 0) &&
                            ClipPolygonAxis(verts, buffer, 1) &&
                            ClipPolygonAxis(verts, buffer, 2);

        if (anyVertsLeft)
        {
            for (int i = 1; i < verts.Count - 1; ++i)
                FillTriangle(verts[0], verts[i], verts[i + 1], texture);
        }
    }

    bool ClipPolygonAxis(List<Vertex> vertices,
        List<Vertex> buffer, int componentIndex)
    {
        ClipPolygonComponent(vertices, componentIndex, 1, buffer);
        vertices.Clear();

        if (buffer.Count == 0)
            return false;

        ClipPolygonComponent(buffer, componentIndex, -1, vertices);
        buffer.Clear();

        return vertices.Count != 0;
    }

    void ClipPolygonComponent(List<Vertex> vertices, int componentIndex,
        float componentFactor, List<Vertex> result)
    {
        Vertex prevVertex = vertices[vertices.Count - 1];
        float prevComponent = prevVertex[componentIndex] * componentFactor;
        bool prevInside = prevComponent <= prevVertex.Position.W;

        foreach (Vertex it in vertices)
        {
            Vertex currVertex = it;
            float currComponent = currVertex[componentIndex] * componentFactor;
            bool currInside = currComponent <= currVertex.Position.W;

            if (currInside ^ prevInside)
            {
                float lerpAmt =
                    (prevVertex.Position.W - prevComponent)
                    / (
                        (prevVertex.Position.W - prevComponent) -
                        (currVertex.Position.W - currComponent)
                    );

                result.Add(prevVertex.Lerp(currVertex, lerpAmt));
            }

            if (currInside)
                result.Add(currVertex);

            prevVertex = currVertex;
            prevComponent = currComponent;
            prevInside = currInside;
        }
    }

    void Swap(ref Vertex a, ref Vertex b)
    {
        Vertex tmp = b;
        b = a;
        a = tmp;
    }

    void FillTriangle(Vertex v1, Vertex v2, Vertex v3, Bitmap texture)
    {
        var screenSpace = Matrix4Utils.ScreenSpace(Width / 2, Height / 2);
        Matrix4 iden = Matrix4.Identity;
        Vertex minYVert = v1.Transform(screenSpace, iden).PerspectiveDivide();
        Vertex midYVert = v2.Transform(screenSpace, iden).PerspectiveDivide();
        Vertex maxYVert = v3.Transform(screenSpace, iden).PerspectiveDivide();

        if (minYVert.TriangleAreaTimesTwo(maxYVert, midYVert) >= 0)
            return;

        if (maxYVert.Y < midYVert.Y) Swap(ref maxYVert, ref midYVert);
        if (midYVert.Y < minYVert.Y) Swap(ref midYVert, ref minYVert);
        if (maxYVert.Y < midYVert.Y) Swap(ref maxYVert, ref midYVert);

        float a = minYVert.TriangleAreaTimesTwo(maxYVert, midYVert);
        ScanTriangle(minYVert, midYVert, maxYVert, a >= 0, texture);
    }

    void ScanTriangle(Vertex minYVert, Vertex midYVert, Vertex maxYVert,
        bool handedness, Bitmap texture)
    {
        var gradients = new Gradients(minYVert, midYVert, maxYVert);

        Edge topBot = new Edge(gradients, minYVert, maxYVert, 0);
        Edge topMid = new Edge(gradients, minYVert, midYVert, 0);
        Edge midBot = new Edge(gradients, midYVert, maxYVert, 1);

        ScanEdges(gradients, topBot, topMid, handedness, texture);
        ScanEdges(gradients, topBot, midBot, handedness, texture);
    }

    void ScanEdges(Gradients gradients, Edge a, Edge b, bool handedness,
        Bitmap texture)
    {
        Edge left = a;
        Edge right = b;

        if (handedness)
        {
            Edge temp = left;
            left = right;
            right = temp;
        }

        int yStart = b.YMin;
        int yEnd = b.YMax;

        for (int j = yStart; j < yEnd; ++j)
        {
            DrawScanLine(gradients, left, right, j, texture);
            left.Step();
            right.Step();
        }
    }

    void DrawScanLine(Gradients gradients, Edge left, Edge right, int j,
        Bitmap texture)
    {
        int xMin = (int)Math.Ceiling(left.X);
        int xMax = (int)Math.Ceiling(right.X);
        float xPrestep = xMin - left.X;
        float xDist = right.X - left.X;

        float texUXStep = gradients.TexUXStep;
        float texVXStep = gradients.TexVXStep;
        float oneOvrZXStep = gradients.OneOvrZXStep;
        float depthXStep = gradients.DepthXStep;
        float lightXStep = gradients.LightXStep;

        float texU = left.TexU + texUXStep * xPrestep;
        float texV = left.TexV + texVXStep * xPrestep;
        float oneOverZ = left.OneOvrZ + oneOvrZXStep * xPrestep;
        float depth = left.Depth + depthXStep * xPrestep;
        float light = left.Light + lightXStep * xPrestep;

        for (int i = xMin; i < xMax; ++i)
        {
            int index = i + j * Width;

            if (depth < zBuffer[index])
            {
                zBuffer[index] = depth;

                float z = 1.0f / oneOverZ;
                int srcX = (int)(texU * z * (texture.Width - 1) + 0.5f);
                int srcY = (int)(texV * z * (texture.Height - 1) + 0.5f);

                CopyPixel(i, j, srcX, srcY, texture, light);
            }

            oneOverZ += oneOvrZXStep;
            texU += texUXStep;
            texV += texVXStep;
            depth += depthXStep;
            light += lightXStep;
        }
    }
}

/* -------------------------------------------------------------------------- */

class Mesh
{
    internal List<Vertex> Vertices { get; } = new List<Vertex>();
    internal List<int> Indices { get; } = new List<int>();

    internal static
    Mesh FromFile(string fileName)
    {
        string ext = Path.GetExtension(fileName);

        switch (ext)
        {
        case ".obj": return new OBJModel(fileName).ToMesh();
        }

        throw new ArgumentException($"Unknown Mesh format {ext}");
    }

    internal
    void Draw(RenderContext context, Matrix4 viewProjection, Matrix4 transform,
        Bitmap texture)
    {
        Matrix4 mvp = transform * viewProjection;

        for (int i = 0; i < Indices.Count; i += 3)
        {
            context.DrawTriangle(
                Vertices[Indices[i + 0]].Transform(mvp, transform),
                Vertices[Indices[i + 1]].Transform(mvp, transform),
                Vertices[Indices[i + 2]].Transform(mvp, transform),
                texture
            );
        }
    }
}

/* -------------------------------------------------------------------------- */

class OBJIndex
{
    internal int VertexIndex { get; }
    internal int TexCoordIndex { get; }
    internal int NormalIndex { get; }

    internal
    OBJIndex(int vertexIndex, int texCoordIndex, int normalIndex)
    {
        VertexIndex = vertexIndex;
        TexCoordIndex = texCoordIndex;
        NormalIndex = normalIndex;
    }

    override public
    bool Equals(object obj)
    {
        var index = (OBJIndex)obj;

        return VertexIndex == index.VertexIndex &&
               TexCoordIndex == index.TexCoordIndex &&
               NormalIndex == index.NormalIndex;
    }

    override public
    int GetHashCode()
    {
        const int BASE = 17;
        const int MULTIPLIER = 31;

        int result = BASE;
        result = MULTIPLIER * result + VertexIndex;
        result = MULTIPLIER * result + TexCoordIndex;
        result = MULTIPLIER * result + NormalIndex;
        return result;
    }
}

class OBJModel
{
    readonly List<Vector4> positions = new List<Vector4>();
    readonly List<Vector4> texCoords = new List<Vector4>();
    readonly List<Vector4> normals = new List<Vector4>();
    readonly List<OBJIndex> indices = new List<OBJIndex>();
    bool hasTexCoords;
    bool hasNormals;

    internal
    OBJModel(string fileName)
    {
        CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        using (var f = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        using (var bs = new BufferedStream(f))
        using (var reader = new StreamReader(bs))
        {
            for (string line; (line = reader.ReadLine()) != null;)
                ParseLine(line);
        }

        Thread.CurrentThread.CurrentCulture = oldCulture;
    }

    string[] SplitTrimEmpty(string str)
    {
        char[] cset = { ' ' };
        return str.Split(cset, StringSplitOptions.RemoveEmptyEntries);
    }

    float[] ParseFloats(string[] tokens, int start = 1, int n = 3)
    {
        float[] res = new float[n];

        for (int i = 0; i < n; ++i)
            res[i] = float.Parse(tokens[start + i]);

        return res;
    }

    void ParseLine(string line)
    {
        string[] tokens = SplitTrimEmpty(line);

        if (tokens.Length == 0 || tokens[0] == "#")
            return;

        float[] v;

        switch (tokens[0])
        {
        case "v":
            v = ParseFloats(tokens);
            positions.Add(new Vector4(v[0], v[1], v[2], 1));
            break;

        case "vt":
            v = ParseFloats(tokens, 1, 2);
            texCoords.Add(new Vector4(v[0], v[1], 0, 0));
            break;

        case "vn":
            v = ParseFloats(tokens);
            normals.Add(new Vector4(v[0], v[1], v[2], 0));
            break;

        case "f":
            for (int i = 0; i < tokens.Length - 3; ++i)
            {
                indices.Add(ParseOBJIndex(tokens[1 + 0]));
                indices.Add(ParseOBJIndex(tokens[2 + i]));
                indices.Add(ParseOBJIndex(tokens[3 + i]));
            }
            break;
        }
    }

    OBJIndex ParseOBJIndex(string token)
    {
        string[] values = token.Split(new char[] { '/' });

        int vertexIndex = int.Parse(values[0]) - 1;
        int texCoordIndex = 0, normalIndex = 0;

        if (values.Length > 1)
        {
            if (values[1] != "")
            {
                hasTexCoords = true;
                texCoordIndex = int.Parse(values[1]) - 1;
            }

            if (values[2] != "")
            {
                hasNormals = true;
                normalIndex = int.Parse(values[2]) - 1;
            }
        }

        return new OBJIndex(vertexIndex, texCoordIndex, normalIndex);
    }

    internal
    Mesh ToMesh()
    {
        var result = new Mesh();
        var indexMap = new Dictionary<OBJIndex, int>();

        for (int i = 0; i < indices.Count; ++i)
        {
            OBJIndex index = indices[i];

            Vector4 position = positions[index.VertexIndex];

            Vector4 texCoord = hasTexCoords ?
                texCoords[index.TexCoordIndex]
                : new Vector4(0, 0, 0, 0);

            Vector4 normal = hasNormals ?
                normals[index.NormalIndex]
                : new Vector4(0, 0, 0, 0);

            bool found = indexMap.TryGetValue(index, out int vertexIndex);

            if (!found)
            {
                vertexIndex = result.Vertices.Count;
                indexMap[index] = vertexIndex;
                result.Vertices.Add(new Vertex(position, texCoord, normal));
            }

            result.Indices.Add(vertexIndex);
        }

        return result;
    }
}

/* -------------------------------------------------------------------------- */

class Camera
{
    static readonly Vector4 X_AXIS = new Vector4(1, 0, 0, 0);
    static readonly Vector4 Y_AXIS = new Vector4(0, 1, 0, 0);
    static readonly Vector4 Z_AXIS = new Vector4(0, 0, 1, 0);

    Transform transform = new Transform();
    Matrix4 projection;

    internal Camera(Matrix4 projection) { this.projection = projection; }

    internal
    Matrix4 ViewProjection
    {
        get
        {
            var inverseRot = Quaternion.Conjugate(transform.Rotation);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(inverseRot);

            Vector4 inversePos = transform.Position * -1;
            Matrix4 translation = Matrix4.CreateTranslation(inversePos.Xyz);

            return translation * rotation * projection;
        }
    }

    internal
    void Update(KeyboardState input, float deltaTime)
    {
        float dt = deltaTime;
        Transform t = transform;

        const float SPEED = 5.0f;
        if (input.IsKeyDown(Key.W)) Move(t.Rotation * Z_AXIS, +SPEED * dt);
        if (input.IsKeyDown(Key.S)) Move(t.Rotation * Z_AXIS, -SPEED * dt);
        if (input.IsKeyDown(Key.D)) Move(t.Rotation * X_AXIS, +SPEED * dt);
        if (input.IsKeyDown(Key.A)) Move(t.Rotation * X_AXIS, -SPEED * dt);

        const float SENS = 2.66f;
        const Key R = Key.Right, L = Key.Left, D = Key.Down, U = Key.Up;
        if (input.IsKeyDown(R)) Rotate(Y_AXIS, +SENS * dt);
        if (input.IsKeyDown(L)) Rotate(Y_AXIS, -SENS * dt);
        if (input.IsKeyDown(D)) Rotate(t.Rotation * X_AXIS, +SENS * dt);
        if (input.IsKeyDown(U)) Rotate(t.Rotation * X_AXIS, -SENS * dt);
    }

    void Move(Vector4 dir, float amt)
    {
        transform = transform.SetPos(transform.Position + dir * amt);
    }

    void Rotate(Vector4 axis, float angle)
    {
        transform = transform.Rotate(Quaternion.FromAxisAngle(axis.Xyz, angle));
    }
}

/* -------------------------------------------------------------------------- */

class Display : GameWindow
{
    internal RenderContext FrameBuffer { get; }
    internal KeyboardState Input { get { return Keyboard.GetState(); } }

    internal
    Display()
        : base(640, 480, GraphicsMode.Default, "Software Rendering",
               GameWindowFlags.FixedWindow)
    {
        FrameBuffer = new RenderContext(Width, Height);
    }

    internal
    new void SwapBuffers()
    {
        GL.DrawPixels(FrameBuffer.Width, FrameBuffer.Height,
            PixelFormat.Bgr, PixelType.UnsignedByte, FrameBuffer.Bytes);
        base.SwapBuffers();
    }
}

/* -------------------------------------------------------------------------- */

class MainClass
{
    static void UnusedParameters(params Object[] o) { if (o != null) { } }

    internal
    static void Main(string[] args)
    {
        UnusedParameters(args);

        Display display = new Display();
        RenderContext target = display.FrameBuffer;

        Bitmap texture = new Bitmap("./res/bricks.jpg");
        Bitmap texture2 = new Bitmap("./res/bricks2.jpg");

        Mesh monkeyMesh = Mesh.FromFile("./res/smoothMonkey0.obj");
        Mesh terrainMesh = Mesh.FromFile("./res/terrain2.obj");

        Transform monkeyTransform = new Transform(new Vector4(0, 0, 3, 1));
        Transform terrainTransform = new Transform(new Vector4(0, -1, 0, 1));

        Camera camera = new Camera(
            Matrix4Utils.Perspective(
                MathHelper.DegreesToRadians(70),
                (float)target.Width / target.Height,
                0.1f, 1000.0f
            )
        );

        long previousTime = DateTime.UtcNow.Ticks;

        display.UpdateFrame += delegate (object s, FrameEventArgs e)
        {
            UnusedParameters(s, e);

            long currentTime = DateTime.UtcNow.Ticks;
            float delta = (float)((currentTime - previousTime) / 10000000.0);
            previousTime = currentTime;

            camera.Update(display.Input, delta);
            Matrix4 vp = camera.ViewProjection;

            monkeyTransform = monkeyTransform.Rotate(
                Quaternion.FromAxisAngle(new Vector3(0, 1, 0), delta)
            );

            target.Clear(0x00);
            target.ClearDepthBuffer();

            monkeyMesh.Draw(target, vp, monkeyTransform.Matrix, texture2);
            terrainMesh.Draw(target, vp, terrainTransform.Matrix, texture);

            display.SwapBuffers();
        };

        display.Run();
    }
}
