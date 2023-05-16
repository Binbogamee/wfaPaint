using System;
using System.Drawing;
using System.Windows.Forms;

namespace wfaPaint
{
    public partial class fm : Form
    {
        private enum MyDrawMode
        {
            Pencil,
            Line,
            Ellipse,
            Rectangle,
            RightTriangle,
            Polygon
        }

        private Bitmap b;
        private Graphics g;
        private Point startMouseDown;
        private Bitmap bb;
        private Pen myPenLeft;
        private Pen myPenRight;

        private MyDrawMode myDrawMode = MyDrawMode.Pencil;
        private bool pipette = false;
        private bool cut = false;

        public fm()
        {
            InitializeComponent();

            // Screen.PrimaryScreen - размер окна
            b = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            myPenLeft = new Pen(pxColor1.BackColor, 10);
            myPenLeft.StartCap = myPenLeft.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            myPenRight = new Pen(pxColorR1.BackColor, 10);
            myPenRight.StartCap = myPenRight.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            pxImage.MouseDown += PxImage_MouseDown;
            pxImage.MouseMove += PxImage_MouseMove;
            pxImage.MouseUp += PxImage_MouseUp;
            pxImage.Paint += (s, e) => e.Graphics.DrawImage(b, 0, 0); // отображение в picturebox

            pxColor1.Click += (s, e) => myPenLeft.Color = pxColor1.BackColor;
            pxColor2.Click += (s, e) => myPenLeft.Color = pxColor2.BackColor;
            pxColor3.Click += (s, e) => myPenLeft.Color = pxColor3.BackColor;
            pxColor4.Click += (s, e) => myPenLeft.Color = pxColor4.BackColor;
            pxColor5.Click += (s, e) => myPenLeft.Color = pxColor5.BackColor;

            pxColorR1.Click += (s, e) => myPenRight.Color = pxColorR1.BackColor;
            pxColorR2.Click += (s, e) => myPenRight.Color = pxColorR2.BackColor;
            pxColorR3.Click += (s, e) => myPenRight.Color = pxColorR3.BackColor;
            pxColorR4.Click += (s, e) => myPenRight.Color = pxColorR4.BackColor;
            pxColorR5.Click += (s, e) => myPenRight.Color = pxColorR5.BackColor;

            trPenWidth.Value = Convert.ToInt32(myPenLeft.Width);
            trPenWidth.ValueChanged += (s, e) => myPenLeft.Width = trPenWidth.Value;

            trPenWidth.Value = Convert.ToInt32(myPenRight.Width);
            trPenWidth.ValueChanged += (s, e) => myPenRight.Width = trPenWidth.Value;

            buImageClear.Click += BuImageClear_Click;
            buImageLoadFromFile.Click += BuImageLoadFromFile_Click;
            buImageSaveToFile.Click += BuImageSaveToFile_Click;
            buImageSaveToClipBoard.Click += (s, e) => { Clipboard.SetImage(b); cut = false; };
            buGetFromClipBoard.Click += BuGetFromClipBoard_Click;

            buDrawPencil.Click += (s, e) => { myDrawMode = MyDrawMode.Pencil; cut = false; };
            buDrawLine.Click += (s, e) => { myDrawMode = MyDrawMode.Line; cut = false; };
            buDrawEllipse.Click += (s, e) => { myDrawMode = MyDrawMode.Ellipse; cut = false; };
            buDrawRectangle.Click += (s, e) => { myDrawMode = MyDrawMode.Rectangle; cut = false; };
            buDrawPolygon.Click += (s, e) => { myDrawMode = MyDrawMode.Polygon; cut = false; };
            buDrawRightTriangle.Click += (s, e) => { myDrawMode = MyDrawMode.RightTriangle; cut = false; };
            bupipette.Click += (s, e) => { pipette = true; cut = false; };
            buCut.Click += (s, e) => cut = true;
        }

        private void BuGetFromClipBoard_Click(object? sender, EventArgs e)
        {
            cut = false;
            IDataObject iData = Clipboard.GetDataObject();
            Bitmap b = (Bitmap)iData.GetData(DataFormats.Bitmap);
            if (b != null)
            {
                g.DrawImage(b, 0, 0);
            }
            pxImage.Invalidate();
        }

        private void BuImageSaveToFile_Click(object? sender, EventArgs e)
        {
            cut = false;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                b.Save(dialog.FileName);
            }
        }

        private void BuImageLoadFromFile_Click(object? sender, EventArgs e)
        {
            cut = false;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                g.Clear(DefaultBackColor);
                g.DrawImage(Bitmap.FromFile(dialog.FileName), 0, 0);
                pxImage.Invalidate();
            }
        }

        private void BuImageClear_Click(object? sender, EventArgs e)
        {
            cut = false;
            g.Clear(DefaultBackColor);
            pxImage.Invalidate();
        }

        private void PxImage_MouseUp(object? sender, MouseEventArgs e)
        {
            if (cut && e.Button == MouseButtons.Left)
            {
                for (int i = startMouseDown.X; i <= e.X; i++)
                {
                    for (int j = startMouseDown.Y; j <= e.Y; j++)
                    {
                        b.SetPixel(i, j, DefaultBackColor);
                    }
                }
                pxImage.Invalidate();

            }
        }

        private void PxImage_MouseMove(object? sender, MouseEventArgs e)
        {
            Pen myPen = myPenLeft;
            if (e.Button == MouseButtons.Left)
            {
                myPen = myPenLeft;
            }
            if (e.Button == MouseButtons.Right)
            {
                myPen = myPenRight;
            }
            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) && !cut)
            {
                switch (myDrawMode)
                {
                    case MyDrawMode.Pencil:
                        g.DrawLine(myPen, startMouseDown, e.Location);
                        startMouseDown = e.Location;
                        break;
                    case MyDrawMode.Line:
                        RestoreBitmap();
                        g.DrawLine(myPen, startMouseDown, e.Location);
                        break;
                    case MyDrawMode.Ellipse:
                        RestoreBitmap();
                        if (ckFillMode.Checked)
                        {
                            g.FillEllipse(new SolidBrush(myPen.Color),
                                new Rectangle(startMouseDown.X, startMouseDown.Y,
                            e.X - startMouseDown.X, e.Y - startMouseDown.Y));
                        }
                        else
                        {
                            g.DrawEllipse(myPen,
                            new Rectangle(startMouseDown.X, startMouseDown.Y,
                            e.X - startMouseDown.X, e.Y - startMouseDown.Y));
                        }
                        break;
                    case MyDrawMode.Rectangle:
                        RestoreBitmap();
                        if (ckFillMode.Checked)
                        {
                            g.FillRectangle(new SolidBrush(myPen.Color),
                                startMouseDown.X, startMouseDown.Y,
                            e.X - startMouseDown.X, e.Y - startMouseDown.Y);
                        }
                        else
                        {
                            g.DrawRectangle(myPen,
                                startMouseDown.X, startMouseDown.Y,
                            e.X - startMouseDown.X, e.Y - startMouseDown.Y);
                        }
                        break;
                    case MyDrawMode.Polygon:
                        RestoreBitmap();
                        var d = Math.Sqrt(Math.Pow(e.X - startMouseDown.X, 2) - Math.Pow((e.Y + startMouseDown.Y) / 2, 2));
                        PointF point1 = new PointF(startMouseDown.X, startMouseDown.Y);
                        PointF point2 = new PointF(e.X, startMouseDown.Y);
                        PointF point3 = new PointF(e.X + (e.X - startMouseDown.X) / 4, (e.Y + startMouseDown.Y) / 2);
                        PointF point4 = new PointF(e.X, e.Y);
                        PointF point5 = new PointF(startMouseDown.X, e.Y);
                        PointF point6 = new PointF(startMouseDown.X - (e.X - startMouseDown.X) / 4, (e.Y + startMouseDown.Y) / 2);

                        PointF[] points = { point1, point2, point3, point4, point5, point6 };
                        if (ckFillMode.Checked)
                        {
                            g.FillPolygon(new SolidBrush(myPen.Color), points);
                        }
                        else
                        {
                            g.DrawPolygon(myPen, points);
                        }
                        break;
                    case MyDrawMode.RightTriangle:
                        RestoreBitmap();
                        Point rightTriangle1 = new Point(startMouseDown.X, startMouseDown.Y);
                        Point rightTriangle2 = new Point(e.X, e.Y);
                        Point rightTriangle3 = new Point(startMouseDown.X, e.Y);

                        Point[] rightTrianglePoints = { rightTriangle1, rightTriangle2, rightTriangle3 };
                        if (ckFillMode.Checked)
                        {
                            g.FillPolygon(new SolidBrush(myPen.Color), rightTrianglePoints);
                        }
                        else
                        {
                            g.DrawPolygon(myPen, rightTrianglePoints);
                        }
                        break;
                    default:
                        break;
                }

                pxImage.Invalidate();
            }

            if (cut)
            {
                if (e.Button == MouseButtons.Left)
                {
                    RestoreBitmap();
                    g.DrawRectangle(new Pen(Color.Gray, 1), startMouseDown.X,
                        startMouseDown.Y, e.X - startMouseDown.X, e.Y - startMouseDown.Y);
                    pxImage.Invalidate();
                }
            }
        }

        private void RestoreBitmap()
        {
            g.Dispose();
            b.Dispose();
            b = (Bitmap)bb.Clone();
            g = Graphics.FromImage(b);
        }

        private void PxImage_MouseDown(object? sender, MouseEventArgs e)
        {
            if (pipette)
            {
                myPenLeft.Color = b.GetPixel(e.X, e.Y);
                pipette = false;
            }
            startMouseDown = e.Location;
            bb = (Bitmap)b.Clone();
        }
    }
}
