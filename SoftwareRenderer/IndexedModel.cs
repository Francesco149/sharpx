// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using System.Collections.Generic;
using OpenTK;

namespace SoftwareRenderer
{
    public class IndexedModel
    {
        public List<Vector4> Positions { get; }
        public List<Vector4> TexCoords { get; }
        public List<Vector4> Normals { get; }
        public List<int> Indices { get; }

        public IndexedModel()
        {
            Positions = new List<Vector4>();
            TexCoords = new List<Vector4>();
            Normals = new List<Vector4>();
            Indices = new List<int>();
        }

        public void CalcNormals()
        {
            for (int i = 0; i < Indices.Count; i += 3)
            {
                int i0 = Indices[i];
                int i1 = Indices[i + 1];
                int i2 = Indices[i + 2];

                Vector3 v1 = new Vector3(Positions[i1] - Positions[i0]);
                Vector3 v2 = new Vector3(Positions[i2] - Positions[i0]);

                Vector4 normal = new Vector4(Vector3.Cross(v1, v2), 0);

                Normals[i0] += normal;
                Normals[i1] += normal;
                Normals[i2] += normal;
            }

            for (int i = 0; i < Normals.Count; ++i)
                Normals[i].Normalize();
        }
    }
}
