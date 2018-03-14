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

        const float SENS = 2.66f;
        const float SPEED = 5.0f;

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
                Matrix4 rotation =
                    Matrix4.CreateFromQuaternion(
                        Quaternion.Conjugate(transform.TransformedRot)
                    );

                Vector4 pos = transform.TransformedPos * -1;
                Matrix4 translation = Matrix4.CreateTranslation(pos.Xyz);

                return translation * rotation * projection;
            }
        }

        public void Update(KeyboardState input, float deltaTime)
        {
            float dt = deltaTime;
            Transform t = transform;

            if (input.IsKeyDown(Key.W)) Move(t.Rot * Z_AXIS, SPEED * dt);
            if (input.IsKeyDown(Key.S)) Move(t.Rot * Z_AXIS, -SPEED * dt);
            if (input.IsKeyDown(Key.D)) Move(t.Rot * X_AXIS, SPEED * dt);
            if (input.IsKeyDown(Key.A)) Move(t.Rot * X_AXIS, -SPEED * dt);

            if (input.IsKeyDown(Key.Right)) Rotate(Y_AXIS, SENS * dt);
            if (input.IsKeyDown(Key.Left)) Rotate(Y_AXIS, -SENS * dt);
            if (input.IsKeyDown(Key.Down)) Rotate(t.Rot * X_AXIS, SENS * dt);
            if (input.IsKeyDown(Key.Up)) Rotate(t.Rot * X_AXIS, -SENS * dt);
        }

        protected void Move(Vector4 dir, float amt)
        {
            transform = transform.SetPos(transform.Pos + dir * amt);
        }

        protected void Rotate(Vector4 axis, float angle)
        {
            transform = transform.Rotate(
                Quaternion.FromAxisAngle(new Vector3(axis), angle)
            );
        }
    }
}
