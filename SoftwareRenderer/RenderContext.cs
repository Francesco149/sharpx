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

        public void ClipPolygonComponent(LinkedList<Vertex> vertices,
                                         int componentIndex,
                                         float componentFactor,
                                         LinkedList<Vertex> result)
        {
            Vertex previousVertex = vertices.Last.Value;
            float previousComponent =
                previousVertex[componentIndex] * componentFactor;
            bool previousInside =
                previousComponent <= previousVertex.Position.W;

            LinkedListNode<Vertex> it = vertices.First;
            while (it.Next != null)
            {
                Vertex currentVertex = it.Next.Value;
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

                    result.AddLast(previousVertex.Lerp(currentVertex, lerpAmt));
                }

                if (currentInside)
                {
                    result.AddLast(currentVertex);
                }

                previousVertex = currentVertex;
                previousComponent = currentComponent;
                previousInside = currentInside;
            }
        }

        public void DrawMesh(Mesh mesh, Matrix4 transform, Bitmap texture)
        {
            for (int i = 0; i < mesh.Indices.Count; i += 3)
            {
                FillTriangle(
                    mesh.Vertices[mesh.Indices[i]].Transform(transform),
                    mesh.Vertices[mesh.Indices[i + 1]].Transform(transform),
                    mesh.Vertices[mesh.Indices[i + 2]].Transform(transform),
                    texture);
            }
        }

        public void FillTriangle(Vertex v1, Vertex v2, Vertex v3,
                                 Bitmap texture)
        {
            Matrix4 screenSpaceTransform =
                Matrix4Utils.InitScreenSpaceTransform(Width / 2, Height / 2);

            Vertex minYVert = v1.Transform(screenSpaceTransform)
                                .PerspectiveDivide();
            Vertex midYVert = v2.Transform(screenSpaceTransform)
                                .PerspectiveDivide();
            Vertex maxYVert = v3.Transform(screenSpaceTransform)
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

            ScanEdges(topToBottom, topToMiddle, handedness, texture);
            ScanEdges(topToBottom, middleToBottom, handedness, texture);
        }

        protected void ScanEdges(Edge a, Edge b, bool handedness,
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

            int yStart = b.YStart;
            int yEnd = b.YEnd;

            for (int j = yStart; j < yEnd; ++j)
            {
                DrawScanLine(left, right, j, texture);
                left.Step();
                right.Step();
            }
        }

        protected void DrawScanLine(Edge left, Edge right, int j,
                                    Bitmap texture)
        {
            int xMin = (int)Math.Ceiling(left.X);
            int xMax = (int)Math.Ceiling(right.X);
            float xPrestep = xMin - left.X;

            float xDist = right.X - left.X;
            float texCoordXXStep = (right.TexCoordX - left.TexCoordX) / xDist;
            float texCoordYXStep = (right.TexCoordY - left.TexCoordY) / xDist;
            float oneOverZXStep = (right.OneOverZ - left.OneOverZ) / xDist;
            float depthXStep = (right.Depth - left.Depth) / xDist;

            float texCoordX = left.TexCoordX + texCoordXXStep * xPrestep;
            float texCoordY = left.TexCoordY + texCoordYXStep * xPrestep;
            float oneOverZ = left.OneOverZ + oneOverZXStep * xPrestep;
            float depth = left.Depth + depthXStep * xPrestep;

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

                    CopyPixel(i, j, srcX, srcY, texture);
                }

                oneOverZ += oneOverZXStep;
                texCoordX += texCoordXXStep;
                texCoordY += texCoordYXStep;
                depth += depthXStep;
            }
        }
    }
}
