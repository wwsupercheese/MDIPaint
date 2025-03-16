using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDIPaint
{
    public partial class DocumentForm: Form
    {
        private int x, y;
        public Bitmap bitmap;
        public string fileName = null;
        public DocumentForm()
        {
            InitializeComponent();
            Size = new Size(300, 200);
            Text = "New";
            bitmap = new Bitmap(300, 200);
            for(int i = 0; i < bitmap.Width; i++)
            {
                for(int j = 0; j < bitmap.Height; j++)
                {
                   bitmap.SetPixel(i, j, Color.White);
                }
            }
        }
        private void Draw(Graphics g, MouseEventArgs e)
        {
            //g.DrawLine(new Pen(MainForm.Color, MainForm.Width), x, y, e.X, e.Y);
            g.DrawEllipse(new Pen(MainForm.Color, MainForm.Width), 
                new RectangleF(Math.Min(x, e.X), Math.Min(y, e.Y), Math.Abs(e.X - x), Math.Abs(e.Y - y)));
        }
        private void DocumentForm_MouseDown(object sender, MouseEventArgs e)
        {
            x = e.X;
            y = e.Y;
        }

        private void DocumentForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Refresh();
                Graphics g = CreateGraphics();
                Draw(g, e);
            }
        }

        private void DocumentForm_MouseUp(object sender, MouseEventArgs e)
        {
            Graphics g = Graphics.FromImage(bitmap);
            Draw(g, e);
            x = e.X;
            y = e.Y;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImage(bitmap, 0, 0);
        }

    }
}
