// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

namespace SoftwareRenderer
{
    public class Transform
    {
        protected Vector4 pos;
        protected Quaternion rot;
        protected Vector4 scale;

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
            this.pos = pos;
            this.rot = rot;
            this.scale = scale;
        }

        public Transform SetPos(Vector4 newPos)
        {
            return new Transform(newPos, rot, scale);
        }

        public Transform Rotate(Quaternion newRot)
        {
            return new Transform(pos, (newRot * rot).Normalized(), scale);
        }

        public Transform LookAt(Vector4 point, Vector4 up)
        {
            return Rotate(GetLookAtRotation(point, up));
        }

        public Quaternion GetLookAtRotation(Vector4 point, Vector4 up)
        {
            return Quaternion.FromMatrix(
                new Matrix3(
                    Matrix4.LookAt(new Vector3(pos), new Vector3(point),
                        new Vector3(up))
                )
            );
        }

        public Matrix4 Transformation
        {
            get
            {
                Matrix4 translationMatrix =
                    Matrix4.CreateTranslation(pos.X, pos.Y, pos.Z);
                Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rot);
                Matrix4 scaleMatrix =
                    Matrix4.CreateScale(scale.X, scale.Y, scale.Z);

                return scaleMatrix * rotationMatrix * translationMatrix;
            }
        }

        public Vector4 TransformedPos { get { return pos; } }
        public Quaternion TransformedRot { get { return rot; } }
        public Vector4 Pos { get { return pos; } }
        public Quaternion Rot { get { return rot; } }
        public Vector4 Scale { get { return scale; } }
    }
}
