using olc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class MouseTest : PixelGameEngine
    {

        public override bool OnUserCreate()
        {
            return true;
        }

        public override bool OnUserUpdate(float fElapsedTime)
        {

            if (GetMouse(0).bHeld)
            {
                var pos = GetMousePos();
                FillCircle(pos, 50, Pixel.WHITE);
            }
            if (GetMouse(1).bHeld)
            {
                var pos = GetMousePos();
                FillCircle(pos, 50, Pixel.BLACK);
            }

            if (GetMouseWheel() != 0)
            {
                Clear(Pixel.BLACK);
            }
            return true;
        }
    }
}
