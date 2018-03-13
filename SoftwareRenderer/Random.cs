// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

namespace SoftwareRenderer
{
    public static class Random
    {
        static readonly System.Random rng = new System.Random();
        public static double NextDouble() { return rng.NextDouble(); }
    }
}
