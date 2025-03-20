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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MDIPaint
{
    public partial class MainForm : Form
    {
        public static Color Color { get; set; }
        public static new int Width { get; set; }

        private MdiLayout CurrectMdiLayout = MdiLayout.TileVertical;
        public MainForm()
        {
            InitializeComponent();
            Color = Color.Black;
            UpdateColorIcon(Color);
            DocumentForm temp = new DocumentForm();
            SetBrushs(temp.GetBrushs());
            temp.Dispose();
            Width = 3;
            toolStripTextBoxBrushSize.Text = Width.ToString();
            toolStripComboBoxBrushs.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        public string GetBrush()
        {
            return (string)toolStripComboBoxBrushs.SelectedItem;
        }
        private void SetBrushs(List<string> brs)
        {
            foreach (var br in brs)
            {
                toolStripComboBoxBrushs.Items.Add(br);
            }
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
                frm.filePath = filepath;
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
                UpdateColorIcon(cd.Color);
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
                foreach (var doc in MdiChildren)
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
            if (new_name != name)
            {
                mydoc.filePath = null;
            }
            mydoc.Text = new_name;
        }
        public bool сохранитьКак()
        {
            if (ActiveMdiChild == null) return false;
            var act = (DocumentForm)ActiveMdiChild;

            var bmp = act.bitmap;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = act.filePath;
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

                act.filePath = dlg.FileName;
                var directoryes = dlg.FileName.Split('\\');
                SetDocumentName(act, directoryes[directoryes.Length - 1]);
                act.changes = false;
                return true;
            }
            return false;

        }
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            сохранитьКак();
        }

        private void файлToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            сохранитьКакToolStripMenuItem.Enabled = !(ActiveMdiChild == null);

            сохранитьToolStripMenuItem.Enabled = !(ActiveMdiChild == null ||
                ((DocumentForm)ActiveMdiChild).filePath == null);
        }

        public void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null || ((DocumentForm)ActiveMdiChild).filePath == null) return;
            var act = (DocumentForm)ActiveMdiChild;

            var bmp = act.bitmap;

            bmp.Save(act.filePath);

            act.changes = false;
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

        private void toolStripComboBoxBrushs_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var child in MdiChildren)
            {
                if (child is DocumentForm doc)
                {
                    doc.UpdateBrush();
                    doc.UpdateCursor();
                }
            }
            //this.Focus();
        }

        private void toolStripTextBoxBrushSize_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Width = Convert.ToInt32(toolStripTextBoxBrushSize.Text);
            }
            catch
            {
                toolStripTextBoxBrushSize.Text = "3";
                MessageBox.Show("Введите кооректный размер");
            }
        }

        private void toolStripButtonFill_Click(object sender, EventArgs e)
        {
            bool currentState = toolStripButtonFill.Tag is bool flag && flag;

            toolStripButtonFill.Tag = !currentState;

            toolStripButtonFill.Text = (bool)toolStripButtonFill.Tag ? "Да" : "Нет";
        }

        public bool GetFill()
        {
            return toolStripButtonFill.Tag is bool flag && flag;
        }

        private void toolStripButtonScaleUp_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null) return;

            var act = (DocumentForm)ActiveMdiChild;

            act.ChangeScale(2);
        }

        private void toolStripButtonScaleDown_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null) return;

            var act = (DocumentForm)ActiveMdiChild;

            act.ChangeScale(0.5);

        }
    }
}
