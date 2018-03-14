// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;

public class Edge
{
    protected float xStep;
    protected float texCoordXStep;
    protected float texCoordYStep;
    protected float oneOverZStep;
    protected float depthStep;
    protected float lightAmtStep;

    public float X { get; protected set; }
    public int YStart { get; protected set; }
    public int YEnd { get; protected set; }
    public float TexCoordX { get; protected set; }
    public float TexCoordY { get; protected set; }
    public float OneOverZ { get; protected set; }
    public float Depth { get; protected set; }
    public float LightAmt { get; protected set; }

    public Edge(Gradients gradients, Vertex minYVert, Vertex maxYVert,
               int minYVertIndex)
    {
        YStart = (int)Math.Ceiling(minYVert.Y);
        YEnd = (int)Math.Ceiling(maxYVert.Y);

        float yDist = maxYVert.Y - minYVert.Y;
        float xDist = maxYVert.X - minYVert.X;

        float yPrestep = YStart - minYVert.Y;
        xStep = xDist / yDist;
        X = minYVert.X + yPrestep * xStep;
        float xPrestep = X - minYVert.X;

        Gradients g = gradients;

        TexCoordX = g.TexCoordX[minYVertIndex] +
                    g.TexCoordXYStep * yPrestep +
                    g.TexCoordXXStep * xPrestep;
        texCoordXStep = g.TexCoordXYStep + g.TexCoordXXStep * xStep;

        TexCoordY = g.TexCoordY[minYVertIndex] +
                    g.TexCoordYYStep * yPrestep +
                    g.TexCoordYXStep * xPrestep;
        texCoordYStep = g.TexCoordYYStep + g.TexCoordYXStep * xStep;

        OneOverZ = g.OneOverZ[minYVertIndex] + g.OneOverZYStep * yPrestep +
                   g.OneOverZXStep * xPrestep;
        oneOverZStep = g.OneOverZYStep + g.OneOverZXStep * xStep;

        Depth = g.Depth[minYVertIndex] + g.DepthYStep * yPrestep +
                g.DepthXStep * xPrestep;
        depthStep = g.DepthYStep + g.DepthXStep * xStep;

        LightAmt = g.LightAmt[minYVertIndex] + g.LightAmtYStep * yPrestep +
                   g.LightAmtXStep * xPrestep;
        lightAmtStep = g.LightAmtYStep + g.LightAmtXStep * xStep;
    }

    public void Step()
    {
        X += xStep;
        TexCoordX += texCoordXStep;
        TexCoordY += texCoordYStep;
        OneOverZ += oneOverZStep;
        Depth += depthStep;
        LightAmt += lightAmtStep;
    }
}
