using olc;
using System.Collections.Generic;

namespace Test
{
    public class ObjectTest : PixelGameEngine
    {


        public class Bil
        {
            public int Power = 10;
            public int X = 0;
            public int Speed = 1;

            public virtual void BrukPower()
            {
                Power = Power - 1;
            }

            public virtual void Kjor()
            {
                if (Power > 0)
                {
                    BrukPower();
                    X += Speed;
                }
            }

        }


        public class ElektriskBil : Bil
        {
            public override void BrukPower()
            {
                Power -= 2;
            }
        }

        public class BensinBil : Bil
        {
            public BensinBil()
            {
                Speed = 5;
            }

            public override void BrukPower()
            {
                Power -= 4;
            }
        }


        public class Motorsykkel : Bil
        {
            public int Power = 10;
            public int X = 0;
            public int Speed = 10;

            public virtual void BrukPower()
            {
                Power = Power - 1;
            }

            public virtual void Kjor()
            {
                if (Power > 0)
                {
                    BrukPower();
                    X += Speed;
                }
            }

        }


        List<Bil> biler = new List<Bil>();


        public override bool OnUserCreate()
        {
            var Tesla = new ElektriskBil();
            var Toyota = new BensinBil();
            var Honda = new Motorsykkel();

            biler.Add(Tesla);
            biler.Add(Toyota);
            biler.Add(Honda);


            return true;
        }


        public override bool OnUserUpdate(float fElapsedTime)
        {
            Clear(Pixel.BLACK);
            int y = 0;
            foreach (var bil in biler)
            {
                DrawRect(bil.X, y, 30, 10, Pixel.WHITE);
                DrawString(bil.X + 1, y + 1, bil.Power.ToString(), Pixel.RED);
                y = y + 10;
            }
            if (GetKey(Key.SPACE).bPressed)
            {
                foreach(var bil in biler)
                {
                    if (bil is Motorsykkel)
                    {
                        ((Motorsykkel)bil).Kjor();
                    }
                    else
                    {
                        bil.Kjor();
                    }
                }
            }
            return true;
            
        }
    }
}
