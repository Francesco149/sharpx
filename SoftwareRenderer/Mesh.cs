// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

public class Mesh
{
    public List<Vertex> Vertices { get; } = new List<Vertex>();
    public List<int> Indices { get; } = new List<int>();

    public static Mesh FromFile(string fileName)
    {
        string ext = Path.GetExtension(fileName);

        switch (ext)
        {
        case ".obj": return new OBJModel(fileName).ToMesh();
        }

        throw new ArgumentException($"Unknown Mesh format {ext}");
    }

    public void Draw(RenderContext context, Matrix4 viewProjection,
        Matrix4 transform, Bitmap texture)
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
