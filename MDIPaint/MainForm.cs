using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MDIPaint
{
    public partial class MainForm: Form
    {
        public static Color Color { get; set; }
        public static int Width { get; set; }

        private MdiLayout CurrectMdiLayout = MdiLayout.TileVertical;
        public MainForm()
        {
            InitializeComponent();
            Color = Color.Black;
            UpdateColorIcon(Color);
            Width = 3;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frmAbout = new AboutForm();
            frmAbout.ShowDialog();
        }

        private void AddDocumentForm(string filepath = null)
        {
            var frm = new DocumentForm();

            if (filepath != null)
            {
                // Загружаем изображение без блокировки файла
                using (var temp = new Bitmap(filepath)) // Освобождаем ресурсы
                {
                    frm.bitmap = new Bitmap(temp); // Создаем копию в памяти
                }
                frm.fileName = filepath;
                SetDocumentName(frm, Path.GetFileName(filepath)); // Используем Path
            }
            else if (MdiChildren.Length > 0)
            {
                SetDocumentName(frm, "New");
            }
            frm.MdiParent = this;
            frm.Show();
            SetMdiLayout(CurrectMdiLayout);
        }
        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddDocumentForm();
        }

        private void размерХолстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var CSF = new CanvasSizeForm();
            if (CSF.ShowDialog() == DialogResult.OK)
            {
                int width = CSF.CanvasWidth;
                int height = CSF.CanvasHeight;

                if (ActiveMdiChild == null) return;
                
                var act = (DocumentForm)ActiveMdiChild;
                var temp = new Bitmap(act.bitmap);
                
                act.bitmap = new Bitmap(width, height);
                
                for (int i = 0; i < temp.Width && i < act.bitmap.Width; i++)
                {
                    for (int j = 0; j < temp.Height && j < act.bitmap.Height; j++)
                    {
                        act.bitmap.SetPixel(i, j, temp.GetPixel(i, j));
                    }
                }
                for (int i = temp.Width; i < act.bitmap.Width; i++)
                {
                    for (int j = 0; j < act.bitmap.Height; j++)
                    {
                        act.bitmap.SetPixel(i, j, Color.White);
                    }
                }
                for (int i = 0; i < act.bitmap.Width; i++)
                {
                    for (int j = temp.Height; j < act.bitmap.Height; j++)
                    {
                        act.bitmap.SetPixel(i, j, Color.White);
                    }
                }
                act.Refresh();
            }
        }

        private void рисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void красныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color = Color.Red;
            UpdateColorIcon(Color.Red);
        }

        private void синийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color = Color.Blue;
            UpdateColorIcon(Color.Blue);
        }

        private void зеленыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color = Color.Green;
            UpdateColorIcon(Color.Green);
        }

        private void другойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                Color = cd.Color;
                UpdateColorIcon(Color);
            }
        }

        private void рисунокToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            размерХолстаToolStripMenuItem.Enabled = !(ActiveMdiChild == null);
        }
        private void SetDocumentName(DocumentForm mydoc, string name)
        {
            int count = 0;
            string new_name = name;
            bool stop = false;
            while (!stop)
            {
                stop = true;
                foreach(var doc in MdiChildren)
                {
                    if (doc.Text == new_name)
                    {
                        count++;
                        new_name = name.Split('.').Length > 1 ? 
                            name.Split('.')[0] + "(" + count + ")." + name.Split('.')[1] : 
                            name + "(" + count + ")";
                        stop = false;
                        continue;
                    }
                }
            }

            mydoc.Text = new_name;
        }
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null) return;
            var act = (DocumentForm)ActiveMdiChild;

            var bmp = act.bitmap;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = act.fileName;
            dlg.AddExtension = true;
            dlg.Filter = "Windows Bitmap (*.bmp)|*.bmp| Файлы JPEG (*.jpg)|*.jpg";
            ImageFormat[] ff = { ImageFormat.Bmp, ImageFormat.Jpeg };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.FileName.Contains("."))
                {
                    bmp.Save(dlg.FileName);
                }
                else
                {
                    bmp.Save(dlg.FileName, ff[dlg.FilterIndex - 1]);
                }
                
                act.fileName = dlg.FileName;
                var directoryes = dlg.FileName.Split('\\');
                SetDocumentName(act, directoryes[directoryes.Length - 1]);
            }

        }

        private void файлToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            сохранитьКакToolStripMenuItem.Enabled = !(ActiveMdiChild == null);

            сохранитьToolStripMenuItem.Enabled = !(ActiveMdiChild == null || ((DocumentForm)ActiveMdiChild).fileName == null);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null || ((DocumentForm)ActiveMdiChild).fileName == null) return;
            var act = (DocumentForm)ActiveMdiChild;

            var bmp = act.bitmap;

            bmp.Save(act.fileName);

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Windows Bitmap (*.bmp)|*.bmp| Файлы JPEG (*.jpeg, *.jpg)|*.jpeg;*.jpg|Все файлы ()*.*|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                AddDocumentForm(dlg.FileName);
            }
        }
        private void SetMdiLayout(MdiLayout layout)
        {
            CurrectMdiLayout = layout;
            LayoutMdi(layout);
        }

        private void каскадомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMdiLayout(MdiLayout.Cascade);
        }

        private void слеваНаправоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMdiLayout(MdiLayout.TileVertical);
        }

        private void сверхуВнизToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMdiLayout(MdiLayout.TileHorizontal);
        }

        private void упорядочитьЗначкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMdiLayout(MdiLayout.ArrangeIcons);
        }
        private void UpdateColorIcon(Color color)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Brush brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, 0, 0, 15, 15);
                }
            }
            colorToolStripMenuItem.Image = bmp;
        }
    }
}
