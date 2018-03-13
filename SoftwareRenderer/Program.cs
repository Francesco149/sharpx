// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using OpenTK;

namespace SoftwareRenderer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Display display = new Display();
            RenderContext target = display.FrameBuffer;
            Stars3D stars = new Stars3D(3, 64, 4);

            Bitmap texture = new Bitmap("./res/bricks.jpg");
            Mesh mesh = new Mesh("./res/monkey0.obj");

            Vertex minYVert = new Vertex(new Vector4(-1, -1, 0, 1),
                                         new Vector4(0, 0, 0, 0));
            Vertex midYVert = new Vertex(new Vector4(0, 1, 0, 1),
                                         new Vector4(0.5f, 1, 0, 0));
            Vertex maxYVert = new Vertex(new Vector4(1, -1, 0, 1),
                                         new Vector4(1, 0, 0, 0));

            Matrix4 projection =
               Matrix4Utils.InitPerspective(
                    MathHelper.DegreesToRadians(70.0f),
                    (float)target.Width / target.Height,
                    0.1f, 1000.0f);

            float rotCounter = 0;
            long previousTime = DateTime.UtcNow.Ticks;

            display.UpdateFrame += delegate (object s, FrameEventArgs e)
            {
                long currentTime = DateTime.UtcNow.Ticks;
                float delta = (float)((currentTime - previousTime)
                                      / 10000000.0);
                previousTime = currentTime;

                //stars.UpdateAndRender(target, delta);

                rotCounter += delta;
                Matrix4 translation = Matrix4.CreateTranslation(0, 0, 3);
                Matrix4 rotation = Matrix4.CreateRotationX(rotCounter) *
                                   Matrix4.CreateRotationZ(rotCounter);
                Matrix4 transform = rotation * translation * projection;

                target.Clear(0x00);
                target.ClearDepthBuffer();
                target.DrawMesh(mesh, transform, texture);
                //target.FillTriangle(maxYVert.Transform(transform),
                //midYVert.Transform(transform),
                //minYVert.Transform(transform),
                //texture);

                display.SwapBuffers();
            };

            display.Run();
        }
    }
}
