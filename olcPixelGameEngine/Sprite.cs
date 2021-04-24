using System;
using System.Collections.Generic;
using System.Text;

namespace olc
{
    public class Sprite //1252 / 696
    {
        public Sprite() => (width, height) = (0, 0);
        //public Sprite(string sImageFile, ResourcePack pack = null) => LoadFromFile(sImageFile, pack);
        public Sprite(int w, int h)
        {
            width = w; height = h;
            pColData = new List<Pixel>((int)(width * height));
            for (int i=0; i < width * height; i++)
            {
                pColData.Add(new Pixel(Const.nDefaultPixel));
            }

            //pColData.ForEach(o => o.n = Const.nDefaultPixel);
        }


        //public Sprite(int w, int h)
        //{
        //    width = w; height = h;
        //    pColData = new List<Pixel>((int)(width * height));
        //    pColData.ForEach(o => o.n = Const.nDefaultPixel);
        //}

        public Const.rcode LoadFromFile(string sImageFile, ResourcePack pack = null)
        {
         //   UNUSED(pack);
            return loader.LoadImageResource(this, sImageFile, pack);
        }
        //public Const.rcode LoadFromPGESprFile(string sImageFile, ResourcePack pack = null);
        //public Const.rcode SaveToPGESprFile(string sImageFile);

        public int width = 0;
        public int height = 0;
        public enum SpriteMode { NORMAL, PERIODIC }
        public enum Flip { NONE = 0, HORIZ = 1, VERT = 2 };
        public ImageLoader loader;

        //public Mode Mode;
        //public Flip Flip;

        public void SetSampleMode(SpriteMode mode = SpriteMode.NORMAL) => modeSample = mode;
        public Pixel GetPixel(int x, int y)
        {
            if (modeSample == SpriteMode.NORMAL)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    return pColData[y * width + x];
                }
                else return new Pixel(0, 0, 0);
            }
            else
            {
                return pColData[Math.Abs(y % height) * width + Math.Abs(x % width)];

            }
        }
        public bool SetPixel(Int32 x, Int32 y, Pixel p)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                pColData[(int)(y * width + x)] = p;
                return true;
            }
            else
                return false;
        }

        public Pixel GetPixel(vi2d a) => GetPixel(a.x, a.y);
        public bool SetPixel(vi2d a, Pixel p) => SetPixel(a.x, a.y, p);

        public Pixel Sample(float x, float y)
        {
            Int32 sx = (Int32)Math.Min((Int32)(x * (float)width), width - 1);
            Int32 sy = (Int32)Math.Min((Int32)(y * (float)height), height - 1);
            return GetPixel(sx, sy);
        }

        public Pixel SampleBL(float u, float v)
        {
            u = u * width - 0.5f;
            v = v * height - 0.5f;
            int x = (int)Math.Floor(u); // cast to int rounds toward zero, not downward
            int y = (int)Math.Floor(v); // thanks @joshinils
            float u_ratio = u - x;
            float v_ratio = v - y;
            float u_opposite = 1 - u_ratio;
            float v_opposite = 1 - v_ratio;
            Pixel p1 = GetPixel(Math.Max(x, 0), Math.Max(y, 0));
            Pixel p2 = GetPixel(Math.Min(x + 1, (int)width - 1), Math.Max(y, 0));
            Pixel p3 = GetPixel(Math.Max(x, 0), Math.Min(y + 1, (int)height - 1));
            Pixel p4 = GetPixel(Math.Min(x + 1, (int)width - 1), Math.Min(y + 1, (int)height - 1));

            return new Pixel(
                (byte)((p1.r * u_opposite + p2.r * u_ratio) * v_opposite + (p3.r * u_opposite + p4.r * u_ratio) * v_ratio),
                (byte)((p1.g * u_opposite + p2.g * u_ratio) * v_opposite + (p3.g * u_opposite + p4.g * u_ratio) * v_ratio),
                (byte)((p1.b * u_opposite + p2.b * u_ratio) * v_opposite + (p3.b * u_opposite + p4.b * u_ratio) * v_ratio)
                );
        }

        public List<Pixel> GetData() { return this.pColData; }
        public Sprite Duplicate()
        {
            Sprite spr = new Sprite(width, height);
            spr.modeSample = modeSample;
            spr.pColData = new List<Pixel>(pColData);
            return spr;
        }

        public Sprite Duplicate(vi2d vPos, vi2d vSize) {
            Sprite spr = new Sprite(vSize.x, vSize.y);
            for (int y = 0; y < vSize.y; y++)
                for (int x = 0; x < vSize.x; x++)
                    spr.SetPixel(x, y, GetPixel(vPos.x + x, vPos.y + y));
            return spr;
        }


        protected List<Pixel> pColData;
        protected SpriteMode modeSample = SpriteMode.NORMAL;

//        unique_ptr<ImageLoader> loader;
    }
}
