﻿using PNGMask.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PNGMask.GUI
{
    public partial class Main : Form
    {
        EventHandler imghandler = delegate(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            if (p.Image == null) return;

            if (p.Width > p.Image.Width &&
                p.Height > p.Image.Height)
                p.SizeMode = PictureBoxSizeMode.CenterImage;
            else
                p.SizeMode = PictureBoxSizeMode.Zoom;
        };

        public Main()
        {
            InitializeComponent();

            this.Icon = Properties.Resources.icon;
            this.ShowIcon = true;

            this.AllowDrop = true;
            this.DragOver += delegate(object sender, DragEventArgs e)
            {
                string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (data == null || data.Length < 1)
                    e.Effect = DragDropEffects.None;
                else if (Path.GetExtension(data[0]) == ".png")
                    e.Effect = DragDropEffects.All;
            };
            this.DragDrop += delegate(object sender, DragEventArgs e)
            {
                string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (data != null && data.Length > 0 && Path.GetExtension(data[0]) == ".png")
                    LoadImage(data[0]);
            };

            imgHidden.Dock = DockStyle.Fill;
            hexHidden.Dock = DockStyle.Fill;
            txtHidden.Dock = DockStyle.Fill;
            listHidden.Dock = DockStyle.Fill;

            imgOriginal.Resize += imghandler;
            imgHidden.Resize += imghandler;

            this.ResizeBegin += delegate { listHidden.Scrollable = false; };
            this.Resize += delegate { columnHeader1.Width = listHidden.Width - 4; };
            this.ResizeEnd += delegate { listHidden.Scrollable = true; };


            ContextMenuStrip cms = new ContextMenuStrip();

            ToolStripMenuItem mi1 = new ToolStripMenuItem("Open link");
            EventHandler openlink = delegate
            {
                if (listHidden.SelectedIndices.Count < 1) return;

                Process.Start(listHidden.SelectedItems[0].SubItems[1].Text);
            };
            mi1.Click += openlink;
            cms.Items.Add(mi1);

            ToolStripMenuItem mi2 = new ToolStripMenuItem("Copy link location");
            mi2.Click += delegate
            {
                if (listHidden.SelectedIndices.Count < 1) return;

                Clipboard.SetText(listHidden.SelectedItems[0].SubItems[1].Text);
            };
            cms.Items.Add(mi2);

            listHidden.ContextMenuStrip = cms;

            listHidden.DoubleClick += openlink;
            listHidden.KeyDown += delegate(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) openlink(null, null); };
            listHidden.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                //Point local = listHidden.PointToClient(e.Location);
                ListViewItem item = listHidden.GetItemAt(e.X, e.Y);

                if (item == null) lblLink.Text = "None";
                else lblLink.Text = item.SubItems[1].Text;
            };
            listHidden.MouseLeave += delegate { lblLink.Text = "None"; };
        }

        string OpenFileDialog(string filter = null)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if(filter != null)
                    ofd.Filter = filter;

                if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return null;
                return ofd.FileName;
            }
        }

        void SetHidden(DataType t, object data)
        {
            imgHidden.Visible = false;
            hexHidden.Visible = false;
            txtHidden.Visible = false;
            listHidden.Visible = false;
            lblLink.Visible = false;

            listHidden.Items.Clear();
            if (listHidden.SmallImageList != null) listHidden.SmallImageList.Dispose();
            if (listHidden.LargeImageList != null) listHidden.LargeImageList.Dispose();
            listHidden.SmallImageList = null;
            listHidden.LargeImageList = null;

            imgHidden.Image = null;

            switch (t)
            {
                case DataType.Image:
                    imgHidden.Image = (Image)data;
                    imgHidden.Visible = true;

                    imghandler(imgHidden, null);

                    tabs.SelectedIndex = 1;
                    break;
                case DataType.Binary:
                    hexHidden.ReadBytes((byte[])data);
                    hexHidden.Visible = true;

                    tabs.SelectedIndex = 1;
                    break;
                case DataType.Text:
                    txtHidden.Text = (string)data;
                    txtHidden.Visible = true;

                    tabs.SelectedIndex = 1;
                    break;
                case DataType.Index:
                    LinkIndex index = (LinkIndex)data;
                    ImageList imglist = new ImageList() { ImageSize = new Size(32, 32), ColorDepth = ColorDepth.Depth32Bit };

                    imglist.Images.AddRange(index.Images.ToArray());

                    listHidden.SmallImageList = imglist;
                    listHidden.LargeImageList = imglist;

                    foreach (LinkIndexRow row in index.Rows)
                        listHidden.Items.Add(new ListViewItem(new string[] { row.Title, row.URL }, row.ImageIndex));

                    listHidden.Visible = true;
                    lblLink.Visible = true;
                    tabs.SelectedIndex = 1;
                    columnHeader1.Width = listHidden.Width - 4;
                    break;
            }
        }

        void DisposeHidden()
        {
            imgHidden.Image = null; //Necessary for imgHidden
            if (hidden != null && hidden is IDisposable)
                ((IDisposable)hidden).Dispose();
        }

        #region menuFile
        PNG pngOriginal = null;
        SteganographyProvider provider = null;
        object hidden = null;
        DataType hiddent = DataType.None;
        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            string path = OpenFileDialog("PNG File (*.png)|*.png");
            if (path == null) return;

            menuFileSave.Enabled = false;
            menuActionDumpHidden.Enabled = false;

            LoadImage(path);
        }

        void LoadImage(string path)
        {
            pngOriginal = new PNG(path);

            using (MemoryStream stream = new MemoryStream())
            {
                if (imgOriginal.Image != null) imgOriginal.Image.Dispose();
                pngOriginal.WriteToStream(stream, true, true);
                stream.Seek(0, SeekOrigin.Begin);
                Image img = Image.FromStream(stream);
                imgOriginal.Image = img;

                imghandler(imgOriginal, null);
            }

            lblNoFile.Visible = false;

            List<Provider> providers = new List<Provider>(Program.Providers);
            bool hasEOF = false;
            bool hasTXT = false;
            int IDATs = 0;
            foreach (PNGChunk chunk in pngOriginal.Chunks)
            {
                if (chunk.Name == "_EOF") hasEOF = true;
                if (chunk.Name == "tEXt") hasTXT = true;
                if (chunk.Name == "IDAT") IDATs++;
            }

            if (hasEOF) providers.Add(Program.XOREOF);
            if (hasTXT) providers.Add(Program.XORTXT);
            if (IDATs > 1) providers.Add(Program.XORIDAT);

            Provider pr = null;
            if (providers.Count > 0)
            {
                using (SelectProvider prov = new SelectProvider(providers.ToArray()))
                {
                    prov.ShowDialog();
                    pr = prov.SelectedProvider;
                }
            }
            providers = null;

            if (pr == null) { provider = null; SetHidden(DataType.None, null); tabs.SelectedIndex = 0; }
            else
            {
                try
                {
                    provider = (SteganographyProvider)Activator.CreateInstance(pr.ProviderType, pngOriginal, true);

                    DisposeHidden();

                    object data;
                    DataType t = provider.Extract(out data);
                    hidden = data;
                    SetHidden(t, data);
                    hiddent = t;

                    if (t != DataType.None)
                        menuActionDumpHidden.Enabled = true;
                }
                catch (InvalidPasswordException)
                {
                    provider = null; SetHidden(DataType.None, null); tabs.SelectedIndex = 0;

                    MessageBox.Show(this, "The password you entered was incorrect.", "Incorrect Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            tabs.Enabled = true;

            menuActionInject.Enabled = true;
            menuActionDumpOriginal.Enabled = true;

            pngOriginal.RemoveNonCritical();
        }

        private void menuFileSave_Click(object sender, EventArgs e)
        {
            if (provider == null) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG File (*.png)|*.png";

            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                sfd.Dispose();
                return;
            }

            string file = sfd.FileName;
            sfd.Dispose();

            using (FileStream fs = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.Read))
                provider.WriteToStream(fs);

            MessageBox.Show(this, "Injected image was saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region menuAction
        #region menuActionInject
        Provider GetProvider()
        {
            using (SelectProvider sp = new SelectProvider(Program.AllProviders))
            {
                sp.ShowDialog();
                return sp.SelectedProvider;
            }
        }

        void Imprint(DataType type, object data, string password, DataType switchtype, object switchdata)
        {
            try
            {
                provider.Imprint(type, data, password);

                DisposeHidden();

                hidden = switchdata;
                hiddent = switchtype;

                SetHidden(hiddent, hidden);

                menuFileSave.Enabled = true;
                menuActionDumpHidden.Enabled = true;
            }
            catch (NotEnoughSpaceException ex) { MessageBox.Show(this, ex.Message, "Out of Space", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void menuActionInjectImage_Click(object sender, EventArgs e)
        {
            string path = OpenFileDialog("Image Files (*.png, *.jpg, *.jpeg, *.gif)|*.png;*.jpg;*.jpeg;*.gif");
            if (path == null) return;

            Provider prov = GetProvider();
            if (prov == null) return;

            provider = (SteganographyProvider)Activator.CreateInstance(prov.ProviderType, pngOriginal, false);
            byte[] img = File.ReadAllBytes(path);
            Imprint(DataType.ImageBytes, img, DataType.Image, Image.FromFile(path));
        }

        private void menuActionInjectText_Click(object sender, EventArgs e)
        {
            string data;
            using (Notepad np = new Notepad())
            {
                np.ShowDialog();
                if (np.Canceled) return;
                data = np.TextData;
            }

            Provider prov = GetProvider();
            if (prov == null) return;

            provider = (SteganographyProvider)Activator.CreateInstance(prov.ProviderType, pngOriginal, false);
            Imprint(DataType.Text, data, DataType.Text, data);
        }

        private void menuActionInjectBinary_Click(object sender, EventArgs e)
        {
            string path = OpenFileDialog("All Files (*.*)|*.*");
            if (path == null) return;

            Provider prov = GetProvider();
            if (prov == null) return;

            provider = (SteganographyProvider)Activator.CreateInstance(prov.ProviderType, pngOriginal, false);
            byte[] data = File.ReadAllBytes(path);


            Imprint(DataType.Binary, data, DataType.Binary, data);
        }

        private void menuActionInjectIndex_Click(object sender, EventArgs e)
        {
            ImageList imgs;
            ListViewItem[] rows;
            using (LinkIndexBuilder lib = new LinkIndexBuilder())
            {
                lib.ShowDialog();
                if (lib.Canceled) return;

                imgs = lib.imglist;
                rows = lib.rows;
            }

            if (rows.Length < 1) return;

            List<string> keys = new List<string>();
            LinkIndex index = new LinkIndex();
            foreach (ListViewItem lvi in rows)
            {
                int i = -1;
                if (!keys.Contains(lvi.ImageKey) && imgs.Images.ContainsKey(lvi.ImageKey))
                {
                    keys.Add(lvi.ImageKey);
                    index.Images.Add(imgs.Images[lvi.ImageKey]);
                    i = index.Images.Count - 1;
                }
                if (i < 0)
                    i = keys.IndexOf(lvi.ImageKey);

                index.Rows.Add(new LinkIndexRow(i, lvi.SubItems[0].Text, lvi.SubItems[1].Text));
            }

            Provider prov = GetProvider();
            if (prov == null) return;

            provider = (SteganographyProvider)Activator.CreateInstance(prov.ProviderType, pngOriginal, false);
            Imprint(DataType.Index, index, DataType.Index, index);
        }
        #endregion

        private void menuActionDumpOriginal_Click(object sender, EventArgs e)
        {
            if (pngOriginal == null) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG File (*.png)|*.png";

            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                sfd.Dispose();
                return;
            }

            string file = sfd.FileName;
            sfd.Dispose();

            using (FileStream fs = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.Read))
                pngOriginal.WriteToStream(fs, true, true);

            MessageBox.Show(this, "Original image was successfully extracted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void menuActionDumpHidden_Click(object sender, EventArgs e)
        {
            if (hidden == null || hiddent == DataType.None) return;

            SaveFileDialog sfd = new SaveFileDialog();

            ImageFormat imgformat = null;
            Image img = null;
            switch (hiddent)
            {
                case DataType.Image:
                    img = ((Image)hidden);
                    if (img.RawFormat.Equals(ImageFormat.Png))
                    {
                        sfd.Filter = "PNG File (*.png)|*.png";
                        imgformat = ImageFormat.Png;
                    }
                    else if (img.RawFormat.Equals(ImageFormat.Jpeg))
                    {
                        sfd.Filter = "JPEG File (*.jpg)|*.jpg";
                        imgformat = ImageFormat.Jpeg;
                    }
                    else if (img.RawFormat.Equals(ImageFormat.Gif))
                    {
                        sfd.Filter = "GIF File (*.gif)|*.gif";
                        imgformat = ImageFormat.Gif;
                    }
                    break;
                case DataType.Index:
                case DataType.Text:
                    sfd.Filter = "Text File (*.txt)|*.txt";
                    break;
                case DataType.Binary:
                    sfd.Filter = "All Files (*.*)|*.*";
                    break;
            }

            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                sfd.Dispose();
                return;
            }

            string file = sfd.FileName;
            sfd.Dispose();

            switch (hiddent)
            {
                case DataType.Image:
                    img.Save(file, imgformat);
                    break;
                case DataType.Index:

                    break;
                case DataType.Text:
                    File.WriteAllText(file, (string)hidden);
                    break;
                case DataType.Binary:
                    File.WriteAllBytes(file, (byte[])hidden);
                    break;
            }

            MessageBox.Show(this, "Hidden data was successfully extracted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            using (About abt = new About())
                abt.ShowDialog();
        }
    }
}
