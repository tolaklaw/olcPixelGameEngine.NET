using olc;
using System;

namespace Test
{
    public class CuboidClass : PixelGameEngine
    {
        // ScreenWidth() <----------------> 
        // ScreenHeight() ^
        // Draw  Draw a pixel 
        // Clear(Pixel color)

        int radius = 350;
        int total = 500;
        float delta;
        vi2d senter;
        float factor = 0f;

        public override bool OnUserCreate()
        {
            // Dette skjer EN gang
            senter = new vi2d(ScreenWidth() / 2, ScreenHeight() / 2);
            delta = (MathF.PI * 2) / total;
            return true;
        }

        public override bool OnUserUpdate(float fElapsedTime)
        {
            // Dette skjer en gang pr frame
            Clear(Pixel.BLACK);
            int x = ScreenWidth();
            int y = ScreenHeight();

            x = x / 2;
            y = y / 2;

            Draw(x, y, Pixel.WHITE);

            for(int i = 0; i < total; i++)
            {

                var a = getposs(i);
                var b = getposs(i * factor);
                //Draw(a + senter, Pixel.RED);
                DrawLine(a + senter, b + senter, Pixel.WHITE);
            }

            factor = factor + 0.15f * fElapsedTime;

            return true;
        }


        vi2d getposs (float i)
        {
            int nx = (int)(radius * MathF.Cos(delta * i));
            int ny = (int)(radius * MathF.Sin(delta * i));

            return new vi2d(nx, ny);
        }
    }
}
