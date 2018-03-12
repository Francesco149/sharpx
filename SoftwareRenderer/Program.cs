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

            Vertex minYVert = new Vertex(-1, -1, 0);
            Vertex midYVert = new Vertex(0, 1, 0);
            Vertex maxYVert = new Vertex(1, -1, 0);

            Matrix4 projection =
               Matrix4.CreatePerspectiveFieldOfView(
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
                Matrix4 rotation = Matrix4.CreateRotationY(rotCounter);
                Matrix4 transform = rotation * translation * projection;

                target.Clear(0x00);
                target.FillTriangle(maxYVert.Transform(transform),
                                    midYVert.Transform(transform),
                                    minYVert.Transform(transform));

                display.SwapBuffers();
            };

            display.Run();
        }
    }
}
