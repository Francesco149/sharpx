// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using System.Collections.Generic;
using OpenTK;

public class RenderContext : Bitmap
{
    protected float[] zBuffer;

    public RenderContext(int width, int height)
        : base(width, height)
    {
        zBuffer = new float[width * height];
    }

    public void ClearDepthBuffer()
    {
        for (int i = 0; i < zBuffer.Length; ++i)
            zBuffer[i] = float.MaxValue;
    }

    public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Bitmap texture)
    {
        bool allInside =
            v1.IsInsideViewFrustum() &&
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

        bool anyVertsLeft =
            ClipPolygonAxis(verts, buffer, 0) &&
            ClipPolygonAxis(verts, buffer, 1) &&
            ClipPolygonAxis(verts, buffer, 2);

        if (anyVertsLeft)
        {
            for (int i = 1; i < verts.Count - 1; ++i)
                FillTriangle(verts[0], verts[i], verts[i + 1], texture);
        }
    }

    protected bool ClipPolygonAxis(List<Vertex> vertices,
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

    public void ClipPolygonComponent(List<Vertex> vertices, int componentIndex,
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

    protected void Swap(ref Vertex a, ref Vertex b)
    {
        Vertex tmp = b;
        b = a;
        a = tmp;
    }

    protected void FillTriangle(Vertex v1, Vertex v2, Vertex v3, Bitmap texture)
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

    protected void ScanTriangle(Vertex minYVert, Vertex midYVert,
        Vertex maxYVert, bool handedness, Bitmap texture)
    {
        var gradients = new Gradients(minYVert, midYVert, maxYVert);

        Edge topBot = new Edge(gradients, minYVert, maxYVert, 0);
        Edge topMid = new Edge(gradients, minYVert, midYVert, 0);
        Edge midBot = new Edge(gradients, midYVert, maxYVert, 1);

        ScanEdges(gradients, topBot, topMid, handedness, texture);
        ScanEdges(gradients, topBot, midBot, handedness, texture);
    }

    protected void ScanEdges(Gradients gradients, Edge a, Edge b,
        bool handedness, Bitmap texture)
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

    protected void DrawScanLine(Gradients gradients, Edge left, Edge right,
        int j, Bitmap texture)
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
