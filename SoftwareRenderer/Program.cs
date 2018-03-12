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
            long previousTime = DateTime.UtcNow.Ticks;
            Stars3D stars = new Stars3D(4096, 64, 20);

            Vertex minYVert = new Vertex(100, 100);
            Vertex midYVert = new Vertex(150, 200);
            Vertex maxYVert = new Vertex(80, 300);

            display.UpdateFrame += delegate (object s, FrameEventArgs e)
            {
                long currentTime = DateTime.UtcNow.Ticks;
                float delta = (float)((currentTime - previousTime)
                                      / 10000000.0);
                previousTime = currentTime;

                //stars.UpdateAndRender(target, delta);
                target.Clear(0x00);

                //for (int j = 100; j < 200; ++j)
                //{
                //    target.DrawScanBuffer(j, 300 - j, 300 + j);
                //}

                target.ScanConvertTriangle(minYVert, midYVert, maxYVert, 0);
                target.FillShape(100, 300);

                display.SwapBuffers();
            };

            display.Run();
        }
    }
}
