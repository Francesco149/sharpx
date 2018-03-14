// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;
using OpenTK.Input;

public class Camera
{
    static readonly Vector4 X_AXIS = new Vector4(1, 0, 0, 0);
    static readonly Vector4 Y_AXIS = new Vector4(0, 1, 0, 0);
    static readonly Vector4 Z_AXIS = new Vector4(0, 0, 1, 0);

    protected Transform transform = new Transform();
    protected Matrix4 projection;

    public Camera(Matrix4 projection)
    {
        this.projection = projection;
    }

    public Matrix4 ViewProjection
    {
        get
        {
            var inverseRot = Quaternion.Conjugate(transform.Rot);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(inverseRot);

            Vector4 inversePos = transform.Pos * -1;
            Matrix4 translation = Matrix4.CreateTranslation(inversePos.Xyz);

            return translation * rotation * projection;
        }
    }

    public void Update(KeyboardState input, float deltaTime)
    {
        float dt = deltaTime;
        Transform t = transform;

        const float SPEED = 5.0f;
        if (input.IsKeyDown(Key.W)) Move(t.Rot * Z_AXIS, +SPEED * dt);
        if (input.IsKeyDown(Key.S)) Move(t.Rot * Z_AXIS, -SPEED * dt);
        if (input.IsKeyDown(Key.D)) Move(t.Rot * X_AXIS, +SPEED * dt);
        if (input.IsKeyDown(Key.A)) Move(t.Rot * X_AXIS, -SPEED * dt);

        const float SENS = 2.66f;
        const Key R = Key.Right, L = Key.Left, D = Key.Down, U = Key.Up;
        if (input.IsKeyDown(R)) Rotate(Y_AXIS, +SENS * dt);
        if (input.IsKeyDown(L)) Rotate(Y_AXIS, -SENS * dt);
        if (input.IsKeyDown(D)) Rotate(t.Rot * X_AXIS, +SENS * dt);
        if (input.IsKeyDown(U)) Rotate(t.Rot * X_AXIS, -SENS * dt);
    }

    protected void Move(Vector4 dir, float amt)
    {
        transform = transform.SetPos(transform.Pos + dir * amt);
    }

    protected void Rotate(Vector4 axis, float angle)
    {
        transform = transform.Rotate(Quaternion.FromAxisAngle(axis.Xyz, angle));
    }
}
