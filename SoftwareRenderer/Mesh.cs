// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System.Collections.Generic;
using OpenTK;

public class Mesh
{
    public List<Vertex> Vertices { get; }
    public List<int> Indices { get; }

    public Mesh(string fileName)
    {
        IndexedModel model = new OBJModel(fileName).ToIndexedModel();
        Vertices = new List<Vertex>();

        for (int i = 0; i < model.Positions.Count; ++i)
        {
            var v = new Vertex(
                model.Positions[i],
                model.TexCoords[i],
                model.Normals[i]
            );

            Vertices.Add(v);
        }

        Indices = model.Indices;
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
