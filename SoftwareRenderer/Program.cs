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
            Bitmap target = display.FrameBuffer;
            long previousTime = DateTime.UtcNow.Ticks;
            Stars3D stars = new Stars3D(4096, 64, 20);

            display.UpdateFrame += delegate (object s, FrameEventArgs e) {
                long currentTime = DateTime.UtcNow.Ticks;
                float delta = (float)((currentTime - previousTime)
                                      / 10000000.0);
                previousTime = currentTime;

                stars.UpdateAndRender(target, delta);
                display.SwapBuffers();
            };

            display.Run();
        }
    }
}
