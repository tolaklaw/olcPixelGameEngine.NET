using System;
using System.Collections.Generic;
using System.Text;

namespace olc
{
    // O------------------------------------------------------------------------------O
    // | olc::Decal - A GPU resident storage of an olc::Sprite                        |
    // O------------------------------------------------------------------------------O
    public class Decal
    {
        public Decal(Sprite spr, bool filter = false, bool clamp = true)
        {
            id = int.MaxValue;
            if (spr == null) return;
            sprite = spr;
            id = Const.renderer.CreateTexture(sprite.width, sprite.height, filter, clamp);
            Update();
        }

        public Decal(UInt32 nExistingTextureResource, Sprite spr)
        {
            if (spr == null) return;
            id = nExistingTextureResource;

        }
        public void Update()
        {
            if (sprite == null) return;
            vUVScale = new vf2d ( 1.0f / (float)sprite.width, 1.0f / (float)sprite.height );
            Const.renderer.ApplyTexture(id);
            Const.renderer.UpdateTexture(id, sprite);
        }
        public void UpdateSprite()
        {
            if (sprite == null) return;
            Const.renderer.ApplyTexture(id);
            Const.renderer.ReadTexture(id, sprite);
        }


        public UInt32 id = int.MaxValue;
        public Sprite sprite = null;
        public vf2d vUVScale = new vf2d(1.0f, 1.0f);
    }
}
