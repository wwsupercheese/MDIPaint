using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace MDIPaint
{
    public partial class DocumentForm : Form
    {
        private Point lastPoint;
        public Bitmap bitmap;
        public string filePath = null;
        private string CurrentBrush = null;
        private Pen CurrentPen = null;
        private bool isDrawing = false;
        private bool fill = false;
        private double scale = 1;
        public bool changes = false;

        private delegate void DrawAction(Graphics g, MouseEventArgs e);
        private Dictionary<string, DrawAction> actions;
        private Dictionary<string, DrawAction> actionsWithoutPreview;
        private Dictionary<string, Cursor> myCursors;

        public List<string> GetBrushs()
        {
            var brushs = actions.Keys.ToList();
            brushs.AddRange(actionsWithoutPreview.Keys.ToList());
            return brushs;
        }

        public DocumentForm()
        {
            InitializeComponent();
            Size = new Size(300, 200);
            Text = "New";
            bitmap = new Bitmap(300, 200);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, Color.White);
                }
            }
            // Инициализация словарей в конструкторе
            actions = new Dictionary<string, DrawAction>
            {
                { "Линия", (g, e) => DrawLine(g, e) },
                { "Элипс", (g, e) => DrawEllipse(g, e) },
                { "Прямоугольник", (g, e) => DrawRectangle(g, e) },
                { "Сердце", (g, e) => DrawHeart(g, e) }
            };

            actionsWithoutPreview = new Dictionary<string, DrawAction>
            {
                { "Ластик", (g, e) => DrawEraser(g, e) },
                { "Перо", (g, e) => DrawFreehand(g, e) },
                { "Ведро краски", (g, e) => DrawBucket(g, e) },
                { "Текст", (g, e) => DrawText(g, e) }
            };

            myCursors = new Dictionary<string, Cursor>
            {
                { "Перо", Cursors.Cross },
                { "Ластик", Cursors.No },
                { "Ведро краски", Cursors.Hand },
                //{ "Текст", Cursors. }

            };
            changes = false;
        }

        private void DocumentForm_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;
            fill = ((MainForm)MdiParent).GetFill();
            lastPoint = e.Location;
            CurrentPen = new Pen(MainForm.Color, MainForm.Width);
            CurrentBrush = ((MainForm)MdiParent).GetBrush();

            if (!isDrawing || CurrentBrush == null) return;
            changes = true;
            if (actionsWithoutPreview.ContainsKey(CurrentBrush))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    actionsWithoutPreview[CurrentBrush](g, e);
                }
                Invalidate();
            }
        }

        public void UpdateBrush()
        {
            fill = ((MainForm)MdiParent).GetFill();
            CurrentPen = new Pen(MainForm.Color, MainForm.Width);
            CurrentBrush = ((MainForm)MdiParent).GetBrush();
            UpdateCursor();
        }

        private void DocumentForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing || CurrentBrush == null) return;

            if (actionsWithoutPreview.ContainsKey(CurrentBrush))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    actionsWithoutPreview[CurrentBrush](g, e);
                }
                Invalidate();
            }
            else
            {
                Refresh();
                using (Graphics g = CreateGraphics())
                {
                    actions[CurrentBrush](g, e);
                }
            }
        }

        private void DocumentForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isDrawing || CurrentBrush == null) return;
            if (actions.ContainsKey(CurrentBrush))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    actions[CurrentBrush](g, e);
                }
                Invalidate();
            }
            isDrawing = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        private void DrawBucket(Graphics g, MouseEventArgs e)
        {
            if (e.X < 0 || e.X >= bitmap.Width || e.Y < 0 || e.Y >= bitmap.Height)
                return;

            Color targetColor = bitmap.GetPixel(e.X, e.Y);
            Color fillColor = CurrentPen.Color;

            if (targetColor.ToArgb() == fillColor.ToArgb())
                return;

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(e.Location);
            HashSet<Point> visited = new HashSet<Point>();

            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            unsafe
            {
                int bytesPerPixel = 4;
                byte* scan0 = (byte*)bmpData.Scan0.ToPointer();

                while (queue.Count > 0)
                {
                    Point pt = queue.Dequeue();

                    if (pt.X < 0 || pt.X >= bitmap.Width ||
                        pt.Y < 0 || pt.Y >= bitmap.Height)
                        continue;

                    byte* pixel = scan0 + pt.Y * bmpData.Stride + pt.X * bytesPerPixel;
                    Color currentColor = Color.FromArgb(
                        pixel[3],
                        pixel[2],
                        pixel[1],
                        pixel[0]);

                    if (currentColor.ToArgb() != targetColor.ToArgb() ||
                        visited.Contains(pt))
                        continue;

                    pixel[3] = fillColor.A;
                    pixel[2] = fillColor.R;
                    pixel[1] = fillColor.G;
                    pixel[0] = fillColor.B;

                    visited.Add(pt);

                    queue.Enqueue(new Point(pt.X + 1, pt.Y));
                    queue.Enqueue(new Point(pt.X - 1, pt.Y));
                    queue.Enqueue(new Point(pt.X, pt.Y + 1));
                    queue.Enqueue(new Point(pt.X, pt.Y - 1));
                }
            }

            bitmap.UnlockBits(bmpData);
            Invalidate();
        }

        private void DrawFreehand(Graphics g, MouseEventArgs e)
        {
            var w = MainForm.Width;
            var rect = new Rectangle(e.X - w / 2, e.Y - w / 2, w, w);
            g.FillEllipse(new SolidBrush(CurrentPen.Color), rect);
        }

        private void DrawEraser(Graphics g, MouseEventArgs e)
        {
            var w = MainForm.Width;
            var rect = new Rectangle(e.X - w / 2, e.Y - w / 2, w, w);
            g.FillEllipse(new SolidBrush(Color.White), rect);
        }

        private void DrawLine(Graphics g, MouseEventArgs e)
        {
            g.DrawLine(CurrentPen, lastPoint, e.Location);
        }
        
        private void DrawHeart(Graphics g, MouseEventArgs e)
        {
            // Определяем границы области
            var ax = Math.Min(e.X, lastPoint.X);
            var bx = Math.Max(e.X, lastPoint.X);
            var ay = Math.Min(e.Y, lastPoint.Y);
            var by = Math.Max(e.Y, lastPoint.Y);

            int width = bx - ax;
            int height = by - ay;

            // Верхняя начальная точка сердца
            Point startTop = new Point(ax + width / 2, ay + height / 3);

            // Контрольные точки для кривых
            Point leftControl1 = new Point(ax, ay);
            Point leftControl2 = new Point(ax, ay + height / 2);
            Point rightControl1 = new Point(bx, ay);
            Point rightControl2 = new Point(bx, ay + height / 2);
            Point endBottom = new Point(ax + width / 2, by);

            // Создаем путь для заливки
            if (fill)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    // Левая половина сердца
                    path.AddBezier(startTop, leftControl1, leftControl2, endBottom);
                    // Правая половина (обратный путь)
                    path.AddBezier(endBottom, rightControl2, rightControl1, startTop);
                    path.CloseFigure();

                    // Заливаем цветом пера
                    using (SolidBrush brush = new SolidBrush(CurrentPen.Color))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }

            // Рисуем контур (всегда)
            g.DrawBezier(CurrentPen, startTop, leftControl1, leftControl2, endBottom);
            g.DrawBezier(CurrentPen, startTop, rightControl1, rightControl2, endBottom);
        }

        private void DrawEllipse(Graphics g, MouseEventArgs e)
        {
            var rect = GetRectangle(lastPoint, e.Location);
            if (fill)
                g.FillEllipse(new SolidBrush(CurrentPen.Color), rect);
            else
                g.DrawEllipse(CurrentPen, rect);
        }

        private void DrawText(Graphics g, MouseEventArgs e)
        {
            isDrawing = false;
            var tf = new TextForm();
            if (tf.ShowDialog() == DialogResult.OK)
            {
                g.DrawString(tf.textbox.Text, tf.textbox.Font, new SolidBrush(CurrentPen.Color), e.Location);
            }
        }
        private void DrawRectangle(Graphics g, MouseEventArgs e)
        {
            var rect = GetRectangle(lastPoint, e.Location);
            if (fill)
                g.FillRectangle(new SolidBrush(CurrentPen.Color), rect);
            else
                g.DrawRectangle(CurrentPen, rect);
        }

        private Rectangle GetRectangle(Point start, Point end)
        {
            return new Rectangle(
                Math.Min(start.X, end.X),
                Math.Min(start.Y, end.Y),
                Math.Abs(start.X - end.X),
                Math.Abs(start.Y - end.Y));
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateCursor();
        }

        public void UpdateCursor()
        {
            if (CurrentBrush == null) return;
            if (myCursors.ContainsKey(CurrentBrush))
            {
                Cursor = myCursors[CurrentBrush];
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        private void DocumentForm_Load(object sender, EventArgs e)
        {
            UpdateBrush();
        }
        private void DocumentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!changes) return;
            var CDF = new CloseDialogForm(Text);
            switch (CDF.ShowDialog())
            {
                case DialogResult.Yes:
                    if (filePath == null)
                    {
                        if (!((MainForm)MdiParent).сохранитьКак()) 
                        {
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        ((MainForm)MdiParent).сохранитьToolStripMenuItem_Click(sender, e);
                    }
                break;

                case DialogResult.No:
                break;

                case DialogResult.Cancel:
                    e.Cancel = true;
                break;
            }
        }

        public void ChangeScale(double newScale)
        {
            if (newScale <= 0 || newScale > 10) return; // Ограничиваем масштаб

            scale = newScale;
            int newWidth = (int)(bitmap.Width * scale);
            int newHeight = (int)(bitmap.Height * scale);

            // Создаем новое масштабированное изображение
            var scaledBitmap = new Bitmap(newWidth, newHeight);

            using (Graphics g = Graphics.FromImage(scaledBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(bitmap, 0, 0, newWidth, newHeight);
            }

            // Обновляем bitmap и размеры формы
            bitmap?.Dispose();
            bitmap = scaledBitmap;
            //Size = new Size(newWidth, newHeight);
            Invalidate();
            changes = true;
        }
    }
}