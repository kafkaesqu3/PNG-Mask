﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PNGMask
{
    public abstract class Graphical : SteganographyProvider
    {
        public Graphical() { }
        public Graphical(Stream svector, string password, bool find = true) : base(svector, password, find) { }
        public Graphical(string fvector, string password, bool find = true) : base(fvector, password, find) { }
        public Graphical(byte[] bvector, string password, bool find = true) : base(bvector, password, find) { }
        public Graphical(PNG pngvector, string password, bool find = true) : base(pngvector, password, find) { }

        protected byte[] BitmapData;
        protected int bpp = 0;
        protected Size BitmapDimensions;
        protected int LineWidth = 0, padding = 0;
        public override void ProcessData(byte[] s, string password, bool find = true)
        {
            using (MemoryStream stream = new MemoryStream(s))
            using (Bitmap bmp = new Bitmap(stream, false))
            {
                BitmapDimensions = bmp.Size;

                if (bmp.PixelFormat == PixelFormat.Format24bppRgb) bpp = 3;
                else if (bmp.PixelFormat == PixelFormat.Format32bppArgb) bpp = 4;
                else throw new FormatException("Image must be 24bppRGB or 32bppARGB");

                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

                try
                {
                    IntPtr ptr = data.Scan0;
                    int bytes = Math.Abs(data.Stride) * bmp.Height;
                    BitmapData = new byte[bytes];

                    Marshal.Copy(ptr, BitmapData, 0, bytes);
                }
                finally { bmp.UnlockBits(data); }

                if (bpp == 3)
                {
                    LineWidth = bmp.Width * 3;
                    padding = 4 - LineWidth % 4;
                }
            }
        }

        public override void WriteToStream(Stream s)
        {
            using (Bitmap bmp = new Bitmap(BitmapDimensions.Width, BitmapDimensions.Height, (bpp == 3 ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb)))
            {
                Rectangle rect = new Rectangle(0, 0, BitmapDimensions.Width, BitmapDimensions.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

                try
                {
                    IntPtr ptr = data.Scan0;

                    Marshal.Copy(BitmapData, 0, ptr, BitmapData.Length);
                }
                finally { bmp.UnlockBits(data); }

                bmp.Save(s, ImageFormat.Png);
            }
        }
    }
}
