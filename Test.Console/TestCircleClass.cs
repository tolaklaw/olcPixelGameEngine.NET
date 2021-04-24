using olc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class TestCircleClass :PixelGameEngine
    {

        float total = 200;
        float radius = 300;
        float delta = 0;
        float factor = 0;

        Pixel p;
        public override bool OnUserCreate()
        {
            delta = (MathF.PI * 2) / total;
            radius = Math.Min(ScreenWidth(), ScreenHeight()) / 2;
            p = new Pixel(127, 127, 127);
            return true;

        }

        public override bool OnUserUpdate(float fElapsedTime)
        {
            Clear(Pixel.BLACK);            
            DCircle(new vi2d(ScreenWidth() / 2, ScreenHeight() / 2), radius);
            DrawString(10, 50, factor.ToString(), Pixel.WHITE);
            factor += 0.05f * fElapsedTime; 
            return true;
        }


        public void DCircle(vi2d pos, float radius)
        {
            for (int i = 0; i < total; i++)
            {
                var res = i * factor;
                DrawLine(getpos(i) + pos, getpos(res) + pos, p);
            }
        }


        public vi2d getpos(float index)
        {
            return new vi2d((int)(radius * MathF.Cos(((index) * delta)+MathF.PI)), 
                            (int)(radius * MathF.Sin(((index) * delta)+MathF.PI)));
        }
    }
}
