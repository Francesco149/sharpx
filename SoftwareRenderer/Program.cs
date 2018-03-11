// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

namespace SoftwareRenderer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Display display = new Display();
            display.UpdateFrame += delegate (object s, FrameEventArgs e) {
                display.SwapBuffers();
            };
            display.Run();
        }
    }
}
