using System;
using System.Collections.Generic;
using System.Text;
using static olc.Const;

namespace olc
{
    public class Renderable
    {

        public rcode Load(string sFile, ResourcePack pack = null, bool filter = false, bool clamp = true)
        {
            pSprite = new Sprite();
            if (pSprite.LoadFromFile(sFile, pack) == rcode.OK)
            {
                pDecal = new Decal(pSprite, filter, clamp);
                return rcode.OK;
            }
            else
            {
                pSprite = null;
                return rcode.NO_FILE;
            }
        }
        public void Create(int width, int height, bool filter = false, bool clamp = true)
        {
            pSprite = new Sprite(width, height);
            pDecal = new Decal(pSprite, filter, clamp);
        }

        public Decal Decal() => pDecal;
        public Sprite Sprite() => pSprite;
        Sprite pSprite = null;
        Decal pDecal = null;
    }
}
