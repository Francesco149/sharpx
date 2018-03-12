// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using OpenTK;

namespace SoftwareRenderer
{
    public static class Matrix4Utils
    {
        public static Matrix4 InitScreenSpaceTransform(float halfWidth,
                                                       float halfHeight)
        {
            return new Matrix4(
                halfWidth, 0, 0, 0,
                0, -halfHeight, 0, 0,
                0, 0, 1, 0,
                halfWidth, halfHeight, 0, 1
            );
        }
    }
}
