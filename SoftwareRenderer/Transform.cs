// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

public class Transform
{
    public Vector4 Pos { get; protected set; }
    public Quaternion Rot { get; protected set; }
    public Vector4 Scale { get; protected set; }

    public Transform()
        : this(new Vector4(0, 0, 0, 0))
    {
    }

    public Transform(Vector4 pos)
        : this(pos, new Quaternion(0, 0, 0, 1), new Vector4(1, 1, 1, 1))
    {
    }

    public Transform(Vector4 pos, Quaternion rot, Vector4 scale)
    {
        Pos = pos;
        Rot = rot;
        Scale = scale;
    }

    public Transform SetPos(Vector4 newPos)
    {
        return new Transform(newPos, Rot, Scale);
    }

    public Transform Rotate(Quaternion newRot)
    {
        return new Transform(Pos, (newRot * Rot).Normalized(), Scale);
    }

    public Transform LookAt(Vector4 point, Vector4 up)
    {
        return Rotate(GetLookAtRotation(point, up));
    }

    public Quaternion GetLookAtRotation(Vector4 point, Vector4 up)
    {
        var m = Matrix4.LookAt(Pos.Xyz, point.Xyz, up.Xyz);
        var m3 = new Matrix3(m);
        return Quaternion.FromMatrix(m3);
    }

    public Matrix4 Matrix
    {
        get
        {
            return
                Matrix4.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                Matrix4.CreateFromQuaternion(Rot) *
                Matrix4.CreateTranslation(Pos.X, Pos.Y, Pos.Z);
        }
    }
}
