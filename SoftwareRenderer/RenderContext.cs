// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using System.Collections.Generic;
using OpenTK;

namespace SoftwareRenderer
{
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
            {
                zBuffer[i] = float.MaxValue;
            }
        }

        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3,
                                 Bitmap texture)
        {
            if (v1.IsInsideViewFrustum() && v2.IsInsideViewFrustum() &&
                v3.IsInsideViewFrustum())
            {
                FillTriangle(v1, v2, v3, texture);
                return;
            }

            List<Vertex> vertices = new List<Vertex>();
            List<Vertex> auxilliaryList = new List<Vertex>();

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            if (ClipPolygonAxis(vertices, auxilliaryList, 0) &&
                ClipPolygonAxis(vertices, auxilliaryList, 1) &&
                ClipPolygonAxis(vertices, auxilliaryList, 2))
            {
                Vertex initialVertex = vertices[0];

                for (int i = 1; i < vertices.Count - 1; ++i)
                {
                    FillTriangle(initialVertex, vertices[i], vertices[i + 1],
                                 texture);
                }
            }
        }

        protected bool ClipPolygonAxis(List<Vertex> vertices,
                                       List<Vertex> auxillaryList,
                                       int componentIndex)
        {
            ClipPolygonComponent(vertices, componentIndex, 1, auxillaryList);
            vertices.Clear();

            if (auxillaryList.Count == 0)
            {
                return false;
            }

            ClipPolygonComponent(auxillaryList, componentIndex, -1, vertices);
            auxillaryList.Clear();

            return vertices.Count != 0;
        }

        public void ClipPolygonComponent(List<Vertex> vertices,
                                         int componentIndex,
                                         float componentFactor,
                                         List<Vertex> result)
        {
            Vertex previousVertex = vertices[vertices.Count - 1];
            float previousComponent =
                previousVertex[componentIndex] * componentFactor;
            bool previousInside =
                previousComponent <= previousVertex.Position.W;

            foreach (Vertex it in vertices)
            {
                Vertex currentVertex = it;
                float currentComponent =
                    currentVertex[componentIndex] * componentFactor;
                bool currentInside =
                    currentComponent <= currentVertex.Position.W;

                if (currentInside ^ previousInside)
                {
                    float lerpAmt =
                        (previousVertex.Position.W - previousComponent)
                        / ((previousVertex.Position.W - previousComponent) -
                           (currentVertex.Position.W - currentComponent));

                    result.Add(previousVertex.Lerp(currentVertex, lerpAmt));
                }

                if (currentInside)
                {
                    result.Add(currentVertex);
                }

                previousVertex = currentVertex;
                previousComponent = currentComponent;
                previousInside = currentInside;
            }
        }

        protected void FillTriangle(Vertex v1, Vertex v2, Vertex v3,
                                 Bitmap texture)
        {
            Matrix4 screenSpaceTransform =
                Matrix4Utils.InitScreenSpaceTransform(Width / 2, Height / 2);
            Matrix4 identity = Matrix4.Identity;
            Vertex minYVert = v1.Transform(screenSpaceTransform, identity)
                                .PerspectiveDivide();
            Vertex midYVert = v2.Transform(screenSpaceTransform, identity)
                                .PerspectiveDivide();
            Vertex maxYVert = v3.Transform(screenSpaceTransform, identity)
                                .PerspectiveDivide();

            if (minYVert.TriangleAreaTimesTwo(maxYVert, midYVert) >= 0)
            {
                return;
            }

            if (maxYVert.Y < midYVert.Y)
            {
                Vertex tmp = maxYVert;
                maxYVert = midYVert;
                midYVert = tmp;
            }

            if (midYVert.Y < minYVert.Y)
            {
                Vertex tmp = midYVert;
                midYVert = minYVert;
                minYVert = tmp;
            }

            if (maxYVert.Y < midYVert.Y)
            {
                Vertex tmp = maxYVert;
                maxYVert = midYVert;
                midYVert = tmp;
            }

            bool handedness =
                minYVert.TriangleAreaTimesTwo(maxYVert, midYVert) >= 0;

            ScanTriangle(minYVert, midYVert, maxYVert, handedness, texture);
        }

        protected void ScanTriangle(Vertex minYVert, Vertex midYVert,
                                Vertex maxYVert, bool handedness,
                                Bitmap texture)
        {
            Gradients gradients = new Gradients(minYVert, midYVert, maxYVert);
            Edge topToBottom = new Edge(gradients, minYVert, maxYVert, 0);
            Edge topToMiddle = new Edge(gradients, minYVert, midYVert, 0);
            Edge middleToBottom = new Edge(gradients, midYVert, maxYVert, 1);

            ScanEdges(gradients, topToBottom, topToMiddle, handedness, texture);
            ScanEdges(gradients, topToBottom, middleToBottom, handedness,
                      texture);
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

            int yStart = b.YStart;
            int yEnd = b.YEnd;

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
            float texCoordXXStep = gradients.TexCoordXXStep;
            float texCoordYXStep = gradients.TexCoordYXStep;
            float oneOverZXStep = gradients.OneOverZXStep;
            float depthXStep = gradients.DepthXStep;
            float lightAmtXStep = gradients.LightAmtXStep;

            float texCoordX = left.TexCoordX + texCoordXXStep * xPrestep;
            float texCoordY = left.TexCoordY + texCoordYXStep * xPrestep;
            float oneOverZ = left.OneOverZ + oneOverZXStep * xPrestep;
            float depth = left.Depth + depthXStep * xPrestep;
            float lightAmt = left.LightAmt + lightAmtXStep * xPrestep;

            for (int i = xMin; i < xMax; ++i)
            {
                int index = i + j * Width;
                if (depth < zBuffer[index])
                {
                    zBuffer[index] = depth;
                    float z = 1.0f / oneOverZ;
                    int srcX =
                        (int)(texCoordX * z * (texture.Width - 1) + 0.5f);
                    int srcY =
                        (int)(texCoordY * z * (texture.Height - 1) + 0.5f);

                    CopyPixel(i, j, srcX, srcY, texture, lightAmt);
                }

                oneOverZ += oneOverZXStep;
                texCoordX += texCoordXXStep;
                texCoordY += texCoordYXStep;
                depth += depthXStep;
                lightAmt += lightAmtXStep;
            }
        }
    }
}
