using System;
using System.Collections.Generic;
using System.Text;
using static olc.Const;

namespace olc
{
    public abstract class ImageLoader
    {
        public abstract rcode LoadImageResource(Sprite spr, string sImageFile, ResourcePack pack);
        public abstract rcode SaveImageResource(Sprite spr, string sImageFile);
    }
}
