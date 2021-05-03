using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace olc
{
    struct rgb
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public uint n
        {
            get
            {
                return (uint)(r | g << 8 | b << 16 | a << 24);
            }
            set
            { // Is this working?

                r = (byte)(value << 0x0000);
                g = (byte)(value << 0x000F);
                b = (byte)(value << 0x00F0);
                a = (byte)(value << 0x0F00);
            }
        }
    }


    public struct HWButton
    {
        public bool bPressed;
        public bool bReleased;
        public bool bHeld;
    }


    public enum Key
    {
        NONE,
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        K0, K1, K2, K3, K4, K5, K6, K7, K8, K9,
        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        UP, DOWN, LEFT, RIGHT,
        SPACE, TAB, SHIFT, CTRL, INS, DEL, HOME, END, PGUP, PGDN,
        BACK, ESCAPE, RETURN, ENTER, PAUSE, SCROLL,
        NP0, NP1, NP2, NP3, NP4, NP5, NP6, NP7, NP8, NP9,
        NP_MUL, NP_DIV, NP_ADD, NP_SUB, NP_DECIMAL, PERIOD,
        EQUALS, COMMA, MINUS,
        OEM_1, OEM_2, OEM_3, OEM_4, OEM_5, OEM_6, OEM_7, OEM_8,
        CAPS_LOCK, ENUM_END
    };


    public struct ResourceBuffer
    {
        public ResourceBuffer(BinaryReader ifstream, int offset, int size)
        {
            vMemory = new List<char>(capacity: (int)size);
        }
        List<char> vMemory;
    }

    public class sResourceFile
    {
        public int nSize;
        public int nOffset;
    }

    public enum DecalMode
    {
        NORMAL,
        ADDITIVE,
        MULTIPLICATIVE,
        STENCIL,
        ILLUMINATE,
        WIREFRAME,
    };

    public struct DecalInstance
    {
        public Decal decal;
        public List<vf2d> pos;
        public List<vf2d> uv;
        public List<float> w;
        public List<Pixel> tint;
        public DecalMode mode;
        public uint points;
    };
    public delegate void CustomRenderFunction();
    public class LayerDesc
    {
        public vf2d vOffset = new vf2d(0, 0);
        public vf2d vScale = new vf2d(1, 1);
        public bool bShow;
        public bool bUpdate;
        public Sprite pDrawTarget;
        public uint nResID;
        public List<DecalInstance> vecDecalInstance = new List<DecalInstance>();
        public Pixel tint;
        public CustomRenderFunction funcHook;
        //function<void()> funcHook = nullptr;
    };


}
