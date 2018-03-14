// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using OpenTK;

class MainClass
{
    public static void Main(string[] args)
    {
        Display display = new Display();
        RenderContext target = display.FrameBuffer;

        Bitmap texture = new Bitmap("./res/bricks.jpg");
        Bitmap texture2 = new Bitmap("./res/bricks2.jpg");
        Mesh monkeyMesh = new Mesh("./res/smoothMonkey0.obj");
        Transform monkeyTransform = new Transform(new Vector4(0, 0, 3, 1));

        Mesh terrainMesh = new Mesh("./res/terrain2.obj");
        Transform terrainTransform =
            new Transform(new Vector4(0, -1, 0, 1));

        Camera camera = new Camera(
            Matrix4Utils.InitPerspective(
                MathHelper.DegreesToRadians(70),
                (float)target.Width / target.Height,
                0.1f, 1000.0f
            )
        );

        long previousTime = DateTime.UtcNow.Ticks;

        display.UpdateFrame += delegate (object s, FrameEventArgs e)
        {
            long currentTime = DateTime.UtcNow.Ticks;
            float delta = (float)((currentTime - previousTime)
                                  / 10000000.0);
            previousTime = currentTime;

            camera.Update(display.Input, delta);
            Matrix4 vp = camera.ViewProjection;

            monkeyTransform = monkeyTransform.Rotate(
                Quaternion.FromAxisAngle(new Vector3(0, 1, 0), delta)
            );

            target.Clear(0x00);
            target.ClearDepthBuffer();

            monkeyMesh.Draw(
                target, vp, monkeyTransform.Transformation, texture2
            );

            terrainMesh.Draw(
                target, vp, terrainTransform.Transformation, texture
            );

            display.SwapBuffers();
        };

        display.Run();
    }
}
