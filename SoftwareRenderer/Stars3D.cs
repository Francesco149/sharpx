// This is free and unencumbered software released into the public domain.
// See the attached UNLICENSE or http://unlicense.org/

using System;
using OpenTK;

namespace SoftwareRenderer
{
    public class Stars3D
    {
        static readonly float tanHalfFOV =
            MathHelper.DegreesToRadians(70.0f / 2);

        static readonly Random rng = new Random();

        protected readonly float spread;
        protected readonly float speed;

        protected readonly float[] starX;
        protected readonly float[] starY;
        protected readonly float[] starZ;

        public Stars3D(int numStars, float spread, float speed)
        {
            this.spread = spread;
            this.speed = speed;

            starX = new float[numStars];
            starY = new float[numStars];
            starZ = new float[numStars];

            for (int i = 0; i < starX.Length; ++i)
            {
                InitStar(i);
            }
        }

        protected void InitStar(int i)
        {
            starX[i] = 2 * ((float)rng.NextDouble() - 0.5f) * spread;
            starY[i] = 2 * ((float)rng.NextDouble() - 0.5f) * spread;
            starZ[i] = ((float)rng.NextDouble() + 0.00001f) * spread;
        }

        public void UpdateAndRender(Bitmap target, float delta)
        {
            target.Clear(0x00);

            float halfWidth = target.Width / 2.0f;
            float halfHeight = target.Height / 2.0f;

            for (int i = 0; i < starX.Length; ++i)
            {
                starZ[i] -= delta * speed;

                if (starZ[i] <= 0)
                {
                    InitStar(i);
                }

                int x = (int)((starX[i]/(starZ[i]*tanHalfFOV))
                              * halfWidth + halfWidth);
                int y = (int)((starY[i]/(starZ[i]*tanHalfFOV))
                              * halfHeight + halfHeight);

                if (x < 0 || x >= target.Width ||
                    y < 0 || y >= target.Height)
                {
                    InitStar(i);
                    continue;
                }

                target.DrawPixel(x, y, 0xFF, 0xFF, 0xFF, 0xFF);
            }
        }
    }
}
