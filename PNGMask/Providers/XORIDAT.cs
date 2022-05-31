﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//namespace PNGMask.Providers
//{
//    public sealed class XORIDAT : XOR
//    {
//        public XORIDAT(Stream svector, bool find = true) : base(svector, find) { }
//        public XORIDAT(string fvector, bool find = true) : base(fvector, find) { }
//        public XORIDAT(byte[] bvector, bool find = true) : base(bvector, find) { }

//        public XORIDAT(PNG png, bool find = true)
//        {
//            image = png;

//            ProcessPNG(find);
//        }

//        public override void ProcessData(byte[] s, bool find = true)
//        {
//            base.ProcessData(s);

//            ProcessPNG(find);
//        }

//        void ProcessPNG(bool find = true)
//        {
//            foreach (PNGChunk chunk in image.Chunks)
//                if (chunk.Name == "IDAT")
//                {
//                    key = (byte[])chunk.Data.Clone();
//                    break;
//                }
//            if (key == null) throw new PNGMaskException("PNG has no IDAT chunk for the SteganographyProvider to process.");

//            if (find)
//            {
//                PNGChunk pdata = default(PNGChunk);
//                bool skip = true, pdatafound = false;
//                foreach (PNGChunk chunk in image.Chunks)
//                    if (chunk.Name == "IDAT")
//                    {
//                        if (skip)
//                        {
//                            skip = false;
//                            continue;
//                        }

//                        pdata = chunk;
//                        pdatafound = true;
//                    }

//                if (pdatafound)
//                {
//                    vector = (byte[])pdata.Data.Clone();

//                    string pass = SteganographyProvider.AskPassword();
//                    if (pass != null && pass.Length > 0)
//                        PrepareKey(Encoding.UTF8.GetBytes(pass));
//                }
//            }
//        }

//        static byte[] PNG_IDAT_HEADER = { 0x49, 0x44, 0x41, 0x54 };
//        protected override void ImprintPNG(byte[] data)
//        {
//            uint[] crcTable = null;
//            uint crc = PNG.CRC32(PNG_IDAT_HEADER, 0, PNG_IDAT_HEADER.Length, 0, ref crcTable);
//            crc = PNG.CRC32(data, 0, data.Length, crc, ref crcTable);
//            byte[] crcb = BitConverter.GetBytes(crc);

//            int IEND = 0;
//            for (int i = 0; i < image.Chunks.Count; i++)
//                if (image.Chunks[i].Name == "IEND")
//                {
//                    IEND = i;
//                    break;
//                }

//            image.Chunks.Insert(IEND, new PNGChunk() { Name = "IDAT", Standard = true, Critical = true, CRC = crc, CRCBytes = new byte[4] { crcb[3], crcb[2], crcb[1], crcb[0] }, ValidCRC = true, Data = data });
//        }
//    }
//}
