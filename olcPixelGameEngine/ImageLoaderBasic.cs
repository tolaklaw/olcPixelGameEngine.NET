using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace olc
{
    public class ImageLoaderBasic : ImageLoader
    {
        public override Const.rcode LoadImageResource(Sprite spr, string sImageFile, ResourcePack pack)
        {

            var img = Image.FromFile(sImageFile);
            Bitmap bmp = new Bitmap(img);

            ImageConverter imageConverter = new ImageConverter();
            byte[] xByte = (byte[])imageConverter.ConvertTo(img, typeof(byte[]));
            //Sprite spr = new Sprite();
            spr.GetData().Clear();
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var imgCol = bmp.GetPixel(x, y);
                    spr.GetData().Add(new Pixel(imgCol.R, imgCol.G, imgCol.B, imgCol.A));
                }
            }
            spr.height = bmp.Height;
            spr.width = bmp.Width;
            return Const.rcode.OK;

        }

        public override Const.rcode SaveImageResource(Sprite spr, string sImageFile)
        {
            throw new NotImplementedException();
        }
    }
}
