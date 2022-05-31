using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//namespace PNGMask.Providers
//{
//    public sealed class XOREOF : XOR
//    {
//        public XOREOF(Stream svector, string password, bool find = true) : base(svector, passow find) { }
//        public XOREOF(string fvector, string password, bool find = true) : base(fvector, find) { }
//        public XOREOF(byte[] bvector, string password, bool find = true) : base(bvector, find) { }

//        public XOREOF(PNG png, string password, bool find = true)
//        {
//            image = png;

//            ProcessPNG(find, password);
//        }

//        public override void ProcessData(byte[] s, string password, bool find = true)
//        {
//            base.ProcessData(s);

//            ProcessPNG(find, password);
//        }

//        void ProcessPNG(bool find = true, string pass)
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
//                PNGChunk eof = image.Chunks[image.Chunks.Count - 1];
//                if (eof.Name == "_EOF")
//                {
//                    vector = (byte[])eof.Data.Clone();

//                    if (pass != null && pass.Length > 0)
//                        PrepareKey(Encoding.UTF8.GetBytes(pass));
//                }
//            }
//        }

//        protected override void ImprintPNG(byte[] data)
//        {
//            image.Chunks.Add(new PNGChunk() { Name = "_EOF", Standard = false, Critical = false, CRC = 0, CRCBytes = new byte[4] { 0x00, 0x00, 0x00, 0x00 }, ValidCRC = false, Data = data });
//        }
//    }
//}
