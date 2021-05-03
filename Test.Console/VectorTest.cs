using olc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{

    public class GameObject
    {
        public vf2d Pos { get; set; }
        public vf2d Vel { get; set; }
        public vf2d Acc { get; set; }
        public int Ang { get; set; }
        private PixelGameEngine pge;
        public GameObject(PixelGameEngine pge, vf2d pos)
        {
            this.Pos = pos;
            this.pge = pge;
            Vel = new vf2d(0.01f, 0.02f);
            Acc = new vf2d(0, 0);
            Ang = 0;
        }


        public void Update()
        {
            Vel += Acc;
            Pos += Vel;

        }

        public void Show()
        {
            pge.DrawCircle(Pos, 10, Pixel.WHITE);
        }
    }


    public class VectorTest : PixelGameEngine
    {
        GameObject go;
        public override bool OnUserCreate()
        {
            go = new GameObject(this, new vf2d(ScreenWidth() / 2, ScreenHeight() / 2));
            return true;
        }

        public override bool OnUserUpdate(float fElapsedTime)
        {
            Clear(Pixel.BLACK);

            go.Update();
            go.Show();

            return true;
        }

    }
}
