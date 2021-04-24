using olc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class OLCTestClass : PixelGameEngine
    {
        //protected override string sAppName => "APPNAME";
        float i = 0;

        public override bool OnUserCreate()
        {
            return true;
        }
        bool flag = true;

        float mindist = float.MaxValue;
        float maxdist = float.MinValue;


        private float normalize(float max, float min, float value)
        {
            return (value - min) / (max - min);
        }

        private float getTriangleDistance(vf2d a1, vf2d a2, vf2d a3)
        {
            var distance = getDistance(a1, a2);
            distance += getDistance(a2, a3);
            distance += getDistance(a3, a1);
            mindist = Math.Min(distance, mindist);
            maxdist = Math.Max(distance, maxdist);
            return distance;

        }


        private float getDistance(vf2d start, vf2d end)
        {
            var distance = (float)Math.Sqrt(Math.Pow((end.y - start.y), 2) + Math.Pow((end.x - start.x), 2));
            return distance;
        }

        int wavefunction = 0;
        static int flag_scale = 1;
        int spacing_scale = 5;
        int fw = 22 * flag_scale;
        int fh = 16 * flag_scale;
        bool showTriangle = true;
        bool showPixel = false;

        public override bool OnUserUpdate(float fElapsedTime)
        {
            vf2d v = new vf2d(1, 1);
            //VectorF2d f = v.norm

            int offsetx = (ScreenWidth() / 2) - ((fw / 2) * spacing_scale);
            int offsety = (ScreenHeight() / 2) - ((fh / 2) * spacing_scale);

            if (GetKey(Key.SPACE).bPressed) flag = !flag;
            if (GetKey(Key.T).bPressed) showTriangle = !showTriangle;
            if (GetKey(Key.P).bPressed) showPixel = !showPixel;
            if (GetKey(Key.LEFT).bPressed)
            {
                wavefunction++;
                wavefunction = wavefunction % 4;
            }
            if (GetKey(Key.RIGHT).bPressed)
            {
                wavefunction--;
                wavefunction = Math.Abs(wavefunction % 4);
            }

            Clear(Pixel.BLACK);
            int spacingX = spacing_scale;
            int spacingY = spacing_scale;
            vf2d[,] flagMap = new vf2d[fw, fh];


            for (int y = 0; y < fh; y++)
            {
                for (int x = 0; x < fw; x++)
                {
                    Pixel col = Pixel.MAGENTA;
                    col = flagColors(x, y);

                    int xo = offsetx + (x * spacingX);
                    int yo = offsety + (y * spacingY);
                    float index = i - (x / 1f) - (y / 1.25f);
                    float cx = 0;
                    float cy = 0;


                    switch (wavefunction)
                    {
                        case 0:
                            cx = (float)Math.Cos(index + x) * (y * 0.25f) * 1.5f + xo;
                            cy = (float)Math.Sin(index + y) * (x * 0.25f) * 1.5f + yo;
                            break;

                        case 1:
                            cx = (float)Math.Cos(index + y) * (y * 0.25f) * 1.5f + xo;
                            cy = (float)Math.Sin(index + x) * (x * 0.25f) * 1.5f + yo;
                            break;
                        case 2:
                            cx = (float)Math.Cos(index + y) * (x * 0.25f) * 1.5f + xo;
                            cy = (float)Math.Cos(index + x) * (y * 0.25f) * 1.5f + yo;
                            break;
                        case 3:
                            cx = (float)Math.Cos(index) * 1.5f + xo;
                            cy = (float)Math.Sin(index) * 1.5f + yo;
                            break;
                    }
                    flagMap[x, y] = new vf2d(cx, cy);
                }
            }
            for (int y = 0; y < fh - 1; y++)
            {
                for (int x = 0; x < fw - 1; x++)
                {
                    if (showTriangle)
                    {
                        var distance = getTriangleDistance(flagMap[x, y], flagMap[x + 1, y], flagMap[x, y + 1]);
                        FillTriangle(flagMap[x, y], flagMap[x + 1, y], flagMap[x, y + 1], getFancyColor(flagColors(x, y), distance));
                        distance = getTriangleDistance(flagMap[x, y + 1], flagMap[x + 1, y + 1], flagMap[x + 1, y]);
                        FillTriangle(flagMap[x, y + 1], flagMap[x + 1, y + 1], flagMap[x + 1, y], getFancyColor(flagColors(x, y), distance));
                    }
                    if (showPixel)
                    {
                        Draw(flagMap[x, y], flagColors(x, y));
                    }
                }
            }
            //i++;
            i += fElapsedTime * 10;

            return true;
        }
        Pixel getFancyColor(Pixel pixel, float distance)
        {
            var scaledDistance = normalize(maxdist, mindist, distance);
            return new Pixel(
                    (byte)(pixel.r * scaledDistance),
                    (byte)(pixel.g * scaledDistance),
                    (byte)(pixel.b * scaledDistance)
                );
        }

        Pixel flagColors(int x, int y)
        {
            Pixel col = Pixel.MAGENTA;
            var fx1 = 6 / 22f * fw;
            var fx2 = 1 / 22f * fw + fx1;
            var fx3 = 2 / 22f * fw + fx2;
            var fx4 = 1 / 22f * fw + fx3;
            var fx5 = 12 / 22f * fw + fx4;

            var fy1 = 6 / 16f * fh;
            var fy2 = 1 / 16f * fh + fy1;
            var fy3 = 2 / 16f * fh + fy2;
            var fy4 = 1 / 16f * fh + fy3;
            var fy5 = 6 / 16f * fh + fy4;
            if (flag)
            {
                if (x < fx1 || x >= fx4)
                {
                    if (y < fy1) col = Pixel.RED;
                    else if (y < fy2) col = Pixel.WHITE;
                    else if (y < fy3) col = Pixel.BLUE;
                    else if (y < fy4) col = Pixel.WHITE;
                    else if (y < fy5) col = Pixel.RED;
                }
                else if (x < fx2 || x >= fx3)
                {
                    if (y < fy2) col = Pixel.WHITE;
                    else if (y < fy3) col = Pixel.BLUE;
                    else col = Pixel.WHITE;
                }
                else if (x < fx3) col = Pixel.BLUE;

            }
            else
            {

                if (x < fx1 || x >= fx4)
                {
                    if (y < fy1) col = Pixel.BLUE;
                    else if (y < fy2) col = Pixel.YELLOW;
                    else if (y < fy3) col = Pixel.YELLOW;
                    else if (y < fy4) col = Pixel.YELLOW;
                    else if (y < fy5) col = Pixel.BLUE;
                }
                else if (x < fx2 || x >= fx3)
                {
                    if (y < fy2) col = Pixel.YELLOW;
                    else if (y < fy3) col = Pixel.YELLOW;
                    else col = Pixel.YELLOW;
                }
                else if (x < fx3) col = Pixel.YELLOW;
            }

            return col;
        }



    }

    public class TestClass : PixelGameEngine
    {
        float i = 0;

        Random rng;
        public override bool OnUserCreate()
        {
            rng = new Random();
            return true;
        }

        public override bool OnUserUpdate(float fElapsedTime)
        {
            //float i = 100;
            Clear(new Pixel(0, 100, 100));
            for (int j = 0; j < ScreenWidth(); j++)
            {

                //DrawLine(100, 100, 200,200,10, Pixel.RED);
                i = i + 0.01f;
                for (int i = 0; i < ScreenHeight(); i++)
                    if (rng.Next(100) > 50)
                        Draw(j, i, new Pixel(127, 127, 127, 255));
                    else
                        Draw(j, i, new Pixel(40, 40, 40, 255));


                //Draw(j, (int)i, Pixel.BLUE);
                //Draw(j, (int)i + 100, Pixel.BLUE);

                //Draw((int)i + 100, (int)j, Pixel.RED);
                //Draw((int)i, (int)j, Pixel.BLUE);

            }
            i = i + 0.5f;
            i = i % ScreenHeight();


            return true;
        }
    }
}
