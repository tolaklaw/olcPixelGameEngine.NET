using System.Runtime.InteropServices;

namespace olc
{


    public abstract class PGEX
    {
        public PGEX(bool bHook = false) {
            if (bHook) pge.pgex_Register(this);
        }
        public abstract void OnBeforeUserCreate();
        public abstract void OnAfterUserCreate();
        public abstract void OnBeforeUserUpdate(float fElapsedTime);
        public abstract void OnAfterUserUpdate(float fElapsedTime);
        public static PixelGameEngine pge;
    }




}

