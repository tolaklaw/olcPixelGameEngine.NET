using System;
using System.Runtime.InteropServices;

namespace olc
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel
    {
        //[StructLayout(LayoutKind.Sequential,Pack=1)]
        //class union {  // This needs work

        //}
        public enum PixelMode { NORMAL, MASK, ALPHA, CUSTOM }
        public static Pixel WHITE => new Pixel(255, 255, 255);
        public static Pixel BLACK => new Pixel(0, 0, 0);
        public static Pixel MAGENTA => new Pixel(255, 0, 255);
        public static Pixel RED => new Pixel(255, 0, 0);
        public static Pixel BLUE => new Pixel(0, 0, 255);
        public static Pixel YELLOW => new Pixel(255, 255, 0);
        public static Pixel GREEN => new Pixel(0, 255, 0);
        //public static Pixel RED = new {1,2,3};
        /*
         * 
         * GREY(192, 192, 192), DARK_GREY(128, 128, 128), VERY_DARK_GREY(64, 64, 64),
                RED(255, 0, 0), DARK_RED(128, 0, 0), VERY_DARK_RED(64, 0, 0),
                YELLOW(255, 255, 0), DARK_YELLOW(128, 128, 0), VERY_DARK_YELLOW(64, 64, 0),
                GREEN(0, 255, 0), DARK_GREEN(0, 128, 0), VERY_DARK_GREEN(0, 64, 0),
                CYAN(0, 255, 255), DARK_CYAN(0, 128, 128), VERY_DARK_CYAN(0, 64, 64),
                BLUE(0, 0, 255), DARK_BLUE(0, 0, 128), VERY_DARK_BLUE(0, 0, 64),
                MAGENTA(255, 0, 255), DARK_MAGENTA(128, 0, 128), VERY_DARK_MAGENTA(64, 0, 64),
                WHITE(255, 255, 255), BLACK(0, 0, 0), BLANK(0, 0, 0, 0);
         * 
         * */
        rgb rgb;

        //private int v1;
        //private int v2;
        //private int v3;

        public byte r { get { return rgb.r; } set { rgb.r = value; } }
        public byte g { get { return rgb.g; } set { rgb.g = value; } }
        public byte b { get { return rgb.b; } set { rgb.b = value; } }
        public byte a { get { return rgb.a; } set { rgb.a = value; } }

        

        // public uint n { get { return rgb.n; } set { rgb.n = value; } }


        //public Pixel() { r = 0; g = 0; b = 0; a = Const.nDefaultAlpha; }

        public Pixel(byte red, byte green, byte blue, byte alpha = Const.nDefaultAlpha)
        {
            rgb.r = red;
            rgb.g = green;
            rgb.b = blue;
            rgb.a = alpha;
        }

        public Pixel(float red, float green, float blue, float alpha)
        {
            rgb.r = (byte)(red * 255f);
            rgb.g = (byte)(green * 255f);
            rgb.b = (byte)(blue * 255f);
            rgb.a = (byte)(alpha * 255f);
        }

        public Pixel PixelLerp(Pixel p1, Pixel p2, float t)
        {
            return (p2 * t) + p1 * (1.0f - t);
        }


        public Pixel(UInt32 p)
        {
            rgb = new rgb();
            rgb.n = p;
        }

        public static bool operator ==(Pixel a, Pixel b) => a.rgb.n == b.rgb.n;
        public static bool operator !=(Pixel a, Pixel b) => !(a.rgb.n == b.rgb.n);
        public static Pixel operator *(Pixel a, float i)
        {
            float fR = Math.Min(255.0f, Math.Max(0.0f, (float)a.rgb.r * i));
            float fG = Math.Min(255.0f, Math.Max(0.0f, (float)a.rgb.g * i));
            float fB = Math.Min(255.0f, Math.Max(0.0f, (float)a.rgb.b * i));
            return new Pixel((byte)fR, (byte)fG, (byte)(fB), a.a);
        }
        public static Pixel operator /(Pixel a, float i)
        {
            float fR = Math.Min(255.0f, Math.Max(0.0f, (float)a.rgb.r / i));
            float fG = Math.Min(255.0f, Math.Max(0.0f, (float)a.rgb.g / i));
            float fB = Math.Min(255.0f, Math.Max(0.0f, (float)a.rgb.b / i));
            return new Pixel((byte)fR, (byte)fG, (byte)(fB), a.a);
        }
        public static Pixel operator +(Pixel a, byte i)
        {
            byte nR = (byte)Math.Min(255, Math.Max(0, a.rgb.r + i));
            byte nG = (byte)Math.Min(255, Math.Max(0, a.rgb.g + i));
            byte nB = (byte)Math.Min(255, Math.Max(0, a.rgb.b + i));
            return new Pixel(nR, nG, nB, a.a);
        }
        public static Pixel operator +(Pixel a, Pixel b)
        {
            byte nR = (byte)Math.Min(255, Math.Max(0, a.rgb.r + b.rgb.r));
            byte nG = (byte)Math.Min(255, Math.Max(0, a.rgb.g + b.rgb.g));
            byte nB = (byte)Math.Min(255, Math.Max(0, a.rgb.b + b.rgb.b));
            return new Pixel(nR, nG, nB, a.a);
        }

        public static Pixel operator -(Pixel a, byte i)
        {
            byte nR = (byte)Math.Min(255, Math.Max(0, a.rgb.r - i));
            byte nG = (byte)Math.Min(255, Math.Max(0, a.rgb.g - i));
            byte nB = (byte)Math.Min(255, Math.Max(0, a.rgb.b - i));
            return new Pixel(nR, nG, nB, a.a);
        }
        public static Pixel operator -(Pixel a, Pixel b)
        {
            byte nR = (byte)Math.Min(255, Math.Max(0, a.rgb.r - b.rgb.r));
            byte nG = (byte)Math.Min(255, Math.Max(0, a.rgb.g - b.rgb.g));
            byte nB = (byte)Math.Min(255, Math.Max(0, a.rgb.b - b.rgb.b));
            return new Pixel(nR, nG, nB, a.a);
        }

        Pixel Inv()
        {
            byte nR = (byte)Math.Min(255, Math.Max(0, 255 - rgb.r));
            byte nG = (byte)Math.Min(255, Math.Max(0, 255 - rgb.g));
            byte nB = (byte)Math.Min(255, Math.Max(0, 255 - rgb.b));
            return new Pixel(nR, nG, nB, a);
        }
    }




}

