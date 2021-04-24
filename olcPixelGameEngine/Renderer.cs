using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using static olc.Const;

namespace olc
{
    public abstract unsafe class Renderer
    {

        public abstract void SetWindowObject(Window* window);
        public abstract void PrepareDevice();
        public abstract rcode CreateDevice(List<IntPtr> parameters, bool bFullScreen, bool bVSYNC);
        public abstract rcode DestroyDevice();
        public abstract void DisplayFrame();
        public abstract void PrepareDrawing();
        public abstract void SetDecalMode(DecalMode mode);
        public abstract void DrawLayerQuad(vf2d offset, vf2d scale, Pixel tint);
        public abstract void DrawDecal(DecalInstance decal);
        public abstract UInt32 CreateTexture(int width, int height, bool filtered = false, bool clamp = true);
        public abstract void UpdateTexture(UInt32 id, Sprite spr);
        
        public abstract void ReadTexture(UInt32 id, Sprite spr);
        
        public abstract UInt32 DeleteTexture(UInt32 id);
        public abstract void ApplyTexture(UInt32 id);
        public abstract void UpdateViewport(vi2d pos, vi2d size);
        public abstract void ClearBuffer(Pixel p, bool bDepth);
        public static PixelGameEngine PGE;
    }




}

