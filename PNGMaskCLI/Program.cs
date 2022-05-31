using PNGMask.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace PNGMask
{

    public class Provider
    {
        public string Name;
        public Type ProviderType;

        public Provider(string Name, Type ProviderType)
        {
            this.Name = Name;
            this.ProviderType = ProviderType;
        }
    }

    public class Wrapper
    {
        public PNG pngOriginal = null;

        public Provider provid= new Provider("Colorcrash", typeof(ColorCrash));


        public PNG LoadImage(string path)
        {
            pngOriginal = new PNG(path);
            //equal at this point
            using (MemoryStream stream = new MemoryStream())
            {
                //if (imgOriginal.Image != null) imgOriginal.Image.Dispose();
                pngOriginal.WriteToStream(stream, true, true);
                stream.Seek(0, SeekOrigin.Begin);
                Image img = Image.FromStream(stream);
                //imgOriginal.Image = img;

                //imghandler(imgOriginal, null);
            }
            return pngOriginal;

            
        }

        public byte[] Extract(PNG pngOriginal, string password)
        {
            try
            {

                ColorCrash provider = new ColorCrash(pngOriginal, password, true);

                object data;

                //hidden = data;
                //hiddent = t;

                //if (t != DataType.None)
                //    menuActionDumpHidden.Enabled = true;
            }
            catch (InvalidPasswordException)
            {
                //provider = null; SetHidden(DataType.None, null); tabs.SelectedIndex = 0;

                //MessageBox.Show(this, "The password you entered was incorrect.", "Incorrect Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("The password you entered was incorrect.");
            }
            pngOriginal.RemoveNonCritical();
            return null;
        }
        
        public void ExtractFromImage(string imgPath, string password)
        {
            PNG image = LoadImage(imgPath);
            var retn = Extract(image, password);

        }

        public void Imprint(DataType type, string imgPath, byte[] data, string password)
        {
            PNG image = LoadImage(imgPath);
            string testfilename = "Z:\\xfer-local\\testout.png";
            //good at this point
            SteganographyProvider provider = new ColorCrash(image, password, true);
            try
            {

                provider.Imprint(type, data, password);

                using (FileStream fs = File.Open(testfilename, FileMode.Create, FileAccess.Write, FileShare.Read))
                    provider.WriteToStream(fs);
                
                //DisposeHidden();

                //hidden = switchdata;
                //hiddent = switchtype;

                //SetHidden(hiddent, hidden);

                //menuFileSave.Enabled = true;
                //menuActionDumpHidden.Enabled = true;
            }
            catch (NotEnoughSpaceException ex) {
                Console.WriteLine("Out of Space");
            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            //encode call
            string baseImgPath = "Z:\\xfer-local\\windows-10-microsoft-windows-colorful-black-background-2560x1920-1553.png";
            string password = "1234";
            string binPath = "Z:\\xfer-local\\beacon.exe.bin";

            Wrapper w = new Wrapper();

            byte[] payload = File.ReadAllBytes(binPath);
            w.Imprint(DataType.Binary, baseImgPath, payload, password);

            string outpath = "Z:\\xfer-local\\testout.png";
            w.Extract(outpath, password);



            //decode call
        }
    }
}
