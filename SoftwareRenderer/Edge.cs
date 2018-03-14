// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;

public class Edge
{
    protected float xStep;
    protected float texUStep;
    protected float texVStep;
    protected float oneOvrZStep;
    protected float depthStep;
    protected float lightStep;

    public float X { get; protected set; }
    public int YMin { get; protected set; }
    public int YMax { get; protected set; }
    public float TexU { get; protected set; }
    public float TexV { get; protected set; }
    public float OneOvrZ { get; protected set; }
    public float Depth { get; protected set; }
    public float Light { get; protected set; }

    public Edge(Gradients gradients, Vertex minYVert, Vertex maxYVert,
        int minYVertIndex)
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
            g.TexU[minYVertIndex] +
            g.TexUXStep * xPrestep +
            g.TexUYStep * yPrestep;

        texVStep = g.TexVXStep * xStep + g.TexVYStep;
        TexV =
            g.TexV[minYVertIndex] +
            g.TexVXStep * xPrestep +
            g.TexVYStep * yPrestep;

        oneOvrZStep = g.OneOvrZXStep * xStep + g.OneOvrZYStep;
        OneOvrZ =
            g.OneOvrZ[minYVertIndex] +
            g.OneOvrZXStep * xPrestep +
            g.OneOvrZYStep * yPrestep;

        depthStep = g.DepthXStep * xStep + g.DepthYStep;
        Depth =
            g.Depth[minYVertIndex] +
            g.DepthXStep * xPrestep +
            g.DepthYStep * yPrestep;

        lightStep = g.LightXStep * xStep + g.LightYStep;
        Light =
            g.Light[minYVertIndex] +
            g.LightXStep * xPrestep +
            g.LightYStep * yPrestep;
    }

    public void Step()
    {
        X += xStep;
        TexU += texUStep;
        TexV += texVStep;
        OneOvrZ += oneOvrZStep;
        Depth += depthStep;
        Light += lightStep;
    }
}
