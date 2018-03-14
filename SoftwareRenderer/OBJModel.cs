// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using OpenTK;

public class OBJModel
{
    protected class OBJIndex
    {
        public int VertexIndex { get; }
        public int TexCoordIndex { get; }
        public int NormalIndex { get; }

        public OBJIndex(int vertexIndex, int texCoordIndex, int normalIndex)
        {
            VertexIndex = vertexIndex;
            TexCoordIndex = texCoordIndex;
            NormalIndex = normalIndex;
        }

        public override bool Equals(object obj)
        {
            var index = (OBJIndex)obj;

            return VertexIndex == index.VertexIndex &&
                   TexCoordIndex == index.TexCoordIndex &&
                   NormalIndex == index.NormalIndex;
        }

        public override int GetHashCode()
        {
            const int BASE = 17;
            const int MULTIPLIER = 31;

            int result = BASE;
            result = MULTIPLIER * result + VertexIndex;
            result = MULTIPLIER * result + TexCoordIndex;
            result = MULTIPLIER * result + NormalIndex;
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

        CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        using (var f = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        using (var bs = new BufferedStream(f))
        using (var meshReader = new StreamReader(bs))
        {
            for (string line; (line = meshReader.ReadLine()) != null;)
                ParseLine(line);
        }

        Thread.CurrentThread.CurrentCulture = oldCulture;
    }

    protected string[] SplitTrimEmpty(string str)
    {
        char[] cset = { ' ' };
        return str.Split(cset, StringSplitOptions.RemoveEmptyEntries);
    }

    protected void ParseLine(string line)
    {
        string[] tokens = SplitTrimEmpty(line);

        if (tokens.Length == 0 || tokens[0] == "#")
            return;

        switch (tokens[0])
        {
        case "v":
            positions.Add(new Vector4(float.Parse(tokens[1]),
                float.Parse(tokens[2]), float.Parse(tokens[3]), 1));
            break;

        case "vt":
            texCoords.Add(new Vector4(float.Parse(tokens[1]),
               float.Parse(tokens[2]), 0, 0));
            break;

        case "vn":
            normals.Add(new Vector4(float.Parse(tokens[1]),
                float.Parse(tokens[2]), float.Parse(tokens[3]), 0));
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

    protected OBJIndex ParseOBJIndex(string token)
    {
        string[] values = token.Split(new char[] { '/' });

        int vertexIndex = int.Parse(values[0]) - 1;
        int texCoordIndex = 0, normalIndex = 0;

        if (values.Length > 1)
        {
            if (values[1] != "")
            {
                hasTexCoords = true;
                texCoordIndex = int.Parse(values[1]) - 1;
            }

            if (values[2] != "")
            {
                hasNormals = true;
                normalIndex = int.Parse(values[2]) - 1;
            }
        }

        return new OBJIndex(vertexIndex, texCoordIndex, normalIndex);
    }

    public Mesh ToMesh()
    {
        var result = new Mesh();
        var indexMap = new Dictionary<OBJIndex, int>();

        for (int i = 0; i < indices.Count; ++i)
        {
            OBJIndex index = indices[i];

            Vector4 position = positions[index.VertexIndex];

            Vector4 texCoord = hasTexCoords ?
                texCoords[index.TexCoordIndex]
                : new Vector4(0, 0, 0, 0);

            Vector4 normal = hasNormals ?
                normals[index.NormalIndex]
                : new Vector4(0, 0, 0, 0);

            bool found = indexMap.TryGetValue(index, out int vertexIndex);

            if (!found)
            {
                vertexIndex = result.Vertices.Count;
                indexMap[index] = vertexIndex;
                result.Vertices.Add(new Vertex(position, texCoord, normal));
            }

            result.Indices.Add(vertexIndex);
        }

        return result;
    }
}