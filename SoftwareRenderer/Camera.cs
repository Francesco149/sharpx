// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;
using OpenTK.Input;

namespace SoftwareRenderer
{
    public class Camera
    {
        static readonly Vector4 X_AXIS = new Vector4(1, 0, 0, 0);
        static readonly Vector4 Y_AXIS = new Vector4(0, 1, 0, 0);
        static readonly Vector4 Z_AXIS = new Vector4(0, 0, 1, 0);

        protected Transform transform;
        protected Matrix4 projection;

        public Camera(Matrix4 projection)
        {
            this.projection = projection;
            transform = new Transform();
        }

        public Matrix4 ViewProjection
        {
            get
            {
                Matrix4 cameraRotation =
                    Matrix4.CreateFromQuaternion(
                        Quaternion.Conjugate(transform.TransformedRot)
                    );

                Vector4 cameraPos = transform.TransformedPos * -1;
                Matrix4 cameraTranslation =
                    Matrix4.CreateTranslation(
                        cameraPos.X, cameraPos.Y, cameraPos.Z
                    );

                return cameraTranslation * cameraRotation * projection;
            }
        }

        public void Update(KeyboardState input, float delta)
        {
            float sensitivityX = 2.66f * delta;
            float sensitivityY = 2.0f * delta;
            float movAmt = 5.0f * delta;

            if (input.IsKeyDown(Key.W))
                Move(transform.Rot * Z_AXIS, movAmt);
            if (input.IsKeyDown(Key.S))
                Move(transform.Rot * Z_AXIS, -movAmt);
            if (input.IsKeyDown(Key.D))
                Move(transform.Rot * X_AXIS, movAmt);
            if (input.IsKeyDown(Key.A))
                Move(transform.Rot * X_AXIS, -movAmt);

            if (input.IsKeyUp(Key.Right))
                Rotate(Y_AXIS, sensitivityX);
            if (input.IsKeyUp(Key.Left))
                Rotate(Y_AXIS, -sensitivityX);
            if (input.IsKeyUp(Key.Down))
                Rotate(transform.Rot * X_AXIS, sensitivityY);
            if (input.IsKeyUp(Key.Up))
                Rotate(transform.Rot * X_AXIS, -sensitivityY);
        }

        protected void Move(Vector4 dir, float amt)
        {
            transform = transform.SetPos(transform.Pos + dir * amt);
        }

        protected void Rotate(Vector4 axis, float angle)
        {
            transform = transform.Rotate(
                Quaternion.FromAxisAngle(new Vector3(axis), -angle)
            );
        }
    }
}
