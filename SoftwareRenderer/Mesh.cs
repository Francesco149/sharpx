// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System.Collections.Generic;

namespace SoftwareRenderer
{
    public class Mesh
    {
        public readonly List<Vertex> Vertices;
        public readonly List<int> Indices;

        public Mesh(string fileName)
        {
            IndexedModel model = new OBJModel(fileName).ToIndexedModel();

            Vertices = new List<Vertex>();
            for (int i = 0; i < model.Positions.Count; ++i)
            {
                Vertices.Add(new Vertex(model.Positions[i],
                                        model.TexCoords[i]));
            }

            Indices = model.Indices;
        }
    }
}
