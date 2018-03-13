// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using System.Collections.Generic;
using OpenTK;

namespace SoftwareRenderer
{
    public class IndexedModel
    {
        public readonly List<Vector4> Positions;
        public readonly List<Vector4> TexCoords;
        public readonly List<Vector4> Normals;
        public readonly List<Vector4> Tangents;
        public readonly List<int> Indices;

        public IndexedModel()
        {
            Positions = new List<Vector4>();
            TexCoords = new List<Vector4>();
            Normals = new List<Vector4>();
            Tangents = new List<Vector4>();
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

        public void CalcTangents()
        {
            for (int i = 0; i < Indices.Count; i += 3)
            {
                int i0 = Indices[i];
                int i1 = Indices[i + 1];
                int i2 = Indices[i + 2];

                Vector4 edge1 = Positions[i1] - Positions[i0];
                Vector4 edge2 = Positions[i2] - Positions[i0];

                float deltaU1 = TexCoords[i1].X - TexCoords[i0].X;
                float deltaV1 = TexCoords[i1].Y - TexCoords[i0].Y;
                float deltaU2 = TexCoords[i2].X - TexCoords[i0].X;
                float deltaV2 = TexCoords[i2].Y - TexCoords[i0].Y;

                float dividend = (deltaU1 * deltaV2 - deltaU2 * deltaV1);
                float f = Math.Abs(dividend) < float.Epsilon ? 0 : 1 / dividend;

                Vector4 tangent = new Vector4(
                    f * (deltaV2 * edge1.X - deltaV1 * edge2.X),
                    f * (deltaV2 * edge1.Y - deltaV1 * edge2.Y),
                    f * (deltaV2 * edge1.Z - deltaV1 * edge2.Z),
                    0
                );

                Tangents[i0] += tangent;
                Tangents[i1] += tangent;
                Tangents[i2] += tangent;
            }

            for (int i = 0; i < Tangents.Count; ++i)
                Tangents[i].Normalize();
        }
    }
}
