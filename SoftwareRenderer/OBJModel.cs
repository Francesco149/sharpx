// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace SoftwareRenderer
{
    public class OBJModel
    {
        protected class OBJIndex
        {
            public int VertexIndex;
            public int TexCoordIndex;
            public int NormalIndex;

            public override bool Equals(object obj)
            {
                var index = (OBJIndex)obj;

                return VertexIndex == index.VertexIndex
                       && TexCoordIndex == index.TexCoordIndex
                       && NormalIndex == index.NormalIndex;
            }

            public override int GetHashCode()
            {
                const int BASE = 17;
                const int MULTIPLIER = 31;

                int result = BASE;

#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'
                result = MULTIPLIER * result + VertexIndex;
                result = MULTIPLIER * result + TexCoordIndex;
                result = MULTIPLIER * result + NormalIndex;
#pragma warning restore RECS0025 // Non-readonly field referenced in 'GetHashCode()'

                return result;
            }
        }

        protected List<Vector4> positions;
        protected List<Vector4> texCoords;
        protected List<Vector4> normals;
        protected List<OBJIndex> indices;
        protected bool hasTexCoords;
        protected bool hasNormals;

        public OBJModel(string fileName)
        {
            positions = new List<Vector4>();
            texCoords = new List<Vector4>();
            normals = new List<Vector4>();
            indices = new List<OBJIndex>();
            hasTexCoords = false;
            hasNormals = false;

            using (var fs = new FileStream(fileName, FileMode.Open,
                                           FileAccess.Read))
            using (var bs = new BufferedStream(fs))
            using (var meshReader = new StreamReader(bs))
            {
                string line;

                while ((line = meshReader.ReadLine()) != null)
                {
                    string[] tokens = line.Split(
                        new char[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries
                    );

                    if (tokens.Length == 0 || tokens[0] == "#")
                        continue;
                    switch (tokens[0])
                    {
                        case "v":
                            positions.Add(new Vector4(float.Parse(tokens[1]),
                                float.Parse(tokens[2]), float.Parse(tokens[3]),
                                1));
                            break;

                        case "vt":
                            texCoords.Add(new Vector4(float.Parse(tokens[1]),
                               float.Parse(tokens[2]), 0, 0));
                            break;

                        case "vn":
                            normals.Add(new Vector4(float.Parse(tokens[1]),
                                float.Parse(tokens[2]), float.Parse(tokens[3]),
                                0));
                            break;

                        case "f":
                            for (int i = 0; i < tokens.Length - 3; ++i)
                            {
                                indices.Add(ParseOBJIndex(tokens[1]));
                                indices.Add(ParseOBJIndex(tokens[2 + i]));
                                indices.Add(ParseOBJIndex(tokens[3 + i]));
                            }
                            break;
                    }
                }
            }
        }

        public IndexedModel ToIndexedModel()
        {
            var result = new IndexedModel();
            var normalModel = new IndexedModel();
            var resultIndexMap = new Dictionary<OBJIndex, int>();
            var normalIndexMap = new Dictionary<int, int>();
            var indexMap = new Dictionary<int, int>();

            for (int i = 0; i < indices.Count; ++i)
            {
                OBJIndex currentIndex = indices[i];

                Vector4 currentPosition = positions[currentIndex.VertexIndex];
                Vector4 currentTexCoord;
                Vector4 currentNormal;

                if (hasTexCoords)
                    currentTexCoord = texCoords[currentIndex.TexCoordIndex];
                else
                    currentTexCoord = new Vector4(0, 0, 0, 0);

                if (hasNormals)
                    currentNormal = normals[currentIndex.NormalIndex];
                else
                    currentNormal = new Vector4(0, 0, 0, 0);

                int modelVertexIndex;

                if (!resultIndexMap.TryGetValue(currentIndex,
                                                out modelVertexIndex))
                {
                    modelVertexIndex = result.Positions.Count;
                    resultIndexMap[currentIndex] = modelVertexIndex;

                    result.Positions.Add(currentPosition);
                    result.TexCoords.Add(currentTexCoord);
                    if (hasNormals)
                        result.Normals.Add(currentNormal);
                }

                int normalModelIndex;

                if (!normalIndexMap.TryGetValue(currentIndex.VertexIndex,
                                                out normalModelIndex))
                {
                    normalModelIndex = normalModel.Positions.Count;
                    normalIndexMap[currentIndex.VertexIndex] = normalModelIndex;

                    normalModel.Positions.Add(currentPosition);
                    normalModel.TexCoords.Add(currentTexCoord);
                    normalModel.Normals.Add(currentNormal);
                    normalModel.Tangents.Add(new Vector4(0, 0, 0, 0));
                }

                result.Indices.Add(modelVertexIndex);
                normalModel.Indices.Add(normalModelIndex);
                indexMap[modelVertexIndex] = normalModelIndex;
            }

            if (!hasNormals)
            {
                normalModel.CalcNormals();

                for (int i = 0; i < result.Positions.Count; ++i)
                    result.Tangents.Add(normalModel.Tangents[indexMap[i]]);
            }

            return result;
        }

        protected OBJIndex ParseOBJIndex(string token)
        {
            string[] values = token.Split(new char[] { '/' });

            var result = new OBJIndex();
            result.VertexIndex = int.Parse(values[0]) - 1;

            if (values.Length > 1)
            {
                if (values[1] != "")
                {
                    hasTexCoords = true;
                    result.TexCoordIndex = int.Parse(values[1]) - 1;
                }

                if (values[2] != "")
                {
                    hasNormals = true;
                    result.NormalIndex = int.Parse(values[2]) - 1;
                }
            }

            return result;
        }
    }
}