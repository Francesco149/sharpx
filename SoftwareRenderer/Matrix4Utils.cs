// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using OpenTK;

public static class Matrix4Utils
{
    public static Matrix4 InitScreenSpaceTransform(float halfWidth,
                                                   float halfHeight)
    {
        return new Matrix4(
            halfWidth, 0, 0, 0,
            0, -halfHeight, 0, 0,
            0, 0, 1, 0,
            halfWidth - 0.5f, halfHeight - 0.5f, 0, 1
        );
    }

    public static Matrix4 InitPerspective(float fov, float aspectRatio,
                                          float zNear, float zFar)
    {
        // Matrix4.CreatePerspectiveFieldOfView uses the OpenGL convention
        // (forward is negative Z) so I made this which mimick benny's
        // convention

        float tanHalfFOV = (float)Math.Tan(fov / 2);
        float zRange = zNear - zFar;

        return new Matrix4(
            1 / (tanHalfFOV * aspectRatio), 0, 0, 0,
            0, 1 / tanHalfFOV, 0, 0,
            0, 0, (-zNear - zFar) / zRange, 1,
            0, 0, 2 * zFar * zNear / zRange, 0
        );
    }
}
