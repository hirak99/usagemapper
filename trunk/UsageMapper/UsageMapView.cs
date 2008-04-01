using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace UsageMapper
{
    public partial class UsageMapView : UserControl
    {
        public FileCollection fc = new FileCollection();
        private FileCollection.FileStructure CurDirStructure;
        public delegate void CurrentFileChangeHandler(object Sender);
        public event CurrentFileChangeHandler CurrentFileChanged;
        private void FireCurrentFileChanged() {
            if (CurrentFileChanged != null) CurrentFileChanged(this);
        }
        private string LastScanned;
        private void ScanningUpdate(object Sender, string Dir) {
            LastScanned = Dir;
            Invalidate();
        }
        private string CurrentFile_;
        private void SetCurrentFile() {
            string tmpFileName = CurrentFile_;
            fc.CancelProcessing();
            lock (fc) {
                CurrentFile_ = tmpFileName;
                FileCollection.FileStructure result = fc.GetFileByName(CurrentFile_);
                if (result != null) {
                    CurDirStructure = result;
                    CreateAllRects(); Invalidate();
                }
                else if (CurDirStructure != null) {
                    CurrentFile_ = CurDirStructure.Name;
                    try {
                        Invoke(new MethodInvoker(FireCurrentFileChanged));
                    }
                    catch (Exception) { }
                }
            }
        }
        public string CurrentFile {
            set {
                CurrentFile_ = value;
                new Thread(SetCurrentFile).Start();
                FireCurrentFileChanged();
                Invalidate();
            }
            get {
                return CurrentFile_;
            }
        }
        public UsageMapView() {
            InitializeComponent();
            fc.ScanUpdate += ScanningUpdate;
        }

        private int Depth = 4;
        private float GapProportion = 0;

        private float GraphWidth, GraphHeight;
        private float GraphLeft, GraphTop;
        private float BarWidth, GapWidth;
        private class Shape
        {
            public RectangleF r;
            public FileCollection.FileStructure fs;
            public Shape(FileCollection.FileStructure fs_, RectangleF r_) {
                fs = fs_;
                r = r_;
            }
            public Brush brush;
            public Pen pen;
            public void Draw(Graphics g) {
                if (brush != null) g.FillRectangle(brush, r);
                if (pen != null) g.DrawRectangle(pen, r.Left,r.Top,r.Width,r.Height);
                if (fs != null)
                    g.DrawString(fs.LastName, new Font("Arial", 8, FontStyle.Regular), Brushes.Black, r);
            }
        }
        private List<Shape> ShapeCollection = new List<Shape>();
        /// <summary>
        /// Adapted from http://www.easyrgb.com/math.php?MATH=M19.
        /// I use it for returning random colors based on the property - see indexToColor
        /// </summary>
        /// <param name="H"></param>
        /// <param name="S"></param>
        /// <param name="L"></param>
        /// <returns></returns>
        private Color HSL_2_RGB(float H, float S, float V) {
            float R, G, B;
            if (S == 0) {
                R = V * 255;
                G = V * 255;
                B = V * 255;
            }
            else {
                float var_h, var_i, var_1, var_2, var_3, var_r, var_g, var_b;
                var_h = H * 6;
                if (var_h == 6) var_h = 0;
                var_i = (float)Math.Floor(var_h);
                var_1 = V * (1 - S);
                var_2 = V * (1 - S * (var_h - var_i));
                var_3 = V * (1 - S * (1 - (var_h - var_i)));

                if (var_i == 0) { var_r = V; var_g = var_3; var_b = var_1; }
                else if (var_i == 1) { var_r = var_2; var_g = V; var_b = var_1; }
                else if (var_i == 2) { var_r = var_1; var_g = V; var_b = var_3; }
                else if (var_i == 3) { var_r = var_1; var_g = var_2; var_b = V; }
                else if (var_i == 4) { var_r = var_3; var_g = var_1; var_b = V; }
                else { var_r = V; var_g = var_1; var_b = var_2; }

                R = var_r * 255;
                G = var_g * 255;
                B = var_b * 255;
            }
            return Color.FromArgb((int)R, (int)G, (int)B);
        }
        // When called from outside, CurDepth must be 0
        private void CreateRects(FileCollection.FileStructure directory, int CurDepth, float top, float bottom) {
            if (CurDepth >= Depth) return;
            float x=GraphLeft+GapWidth+(GapWidth+BarWidth)*CurDepth;
            float height = bottom - top;
            float cumHeight = 0;
            if (directory.SubFiles!=null)
                foreach (FileCollection.FileStructure subDir in directory.SubFiles) {
                    float thisHeight = (float)subDir.Size / directory.Size;
                    if (thisHeight * height > 2) {
                        Shape shape = new Shape(subDir,
                            new RectangleF(
                                x, top + height * (1 - (cumHeight + thisHeight)),
                                BarWidth, thisHeight * height));
                        shape.pen = Pens.Black;
                        float hue = (shape.r.Top + shape.r.Height / 2) / GraphHeight;
                        hue = ((1-hue) + 1) / 3;
                        shape.brush = new SolidBrush(HSL_2_RGB(hue, 0.5f, 1));
                        ShapeCollection.Add(shape);
                        if (subDir.IsDirectory) {
                            CreateRects(subDir, CurDepth + 1, shape.r.Top, shape.r.Bottom);
                        }
                    }
                    cumHeight += thisHeight;
                }
        }
        private void CreateAllRects() {
            if (fc.IsWorking) return;
            lock (ShapeCollection) {
                ShapeCollection.Clear();
                if (CurDirStructure == null) return;
                GraphWidth = ClientSize.Width - 51;
                GraphHeight = ClientSize.Height - 3;
                GraphLeft = 50;
                GraphTop = (ClientSize.Height - GraphHeight) / 2;
                /*
                Shape thisDir;
                ShapeCollection.Add(thisDir=new Shape(CurDirStructure,
                    new RectangleF(0, GraphTop, 50, GraphHeight)));
                thisDir.pen = Pens.Black;
                thisDir.brush = new SolidBrush(HSL_2_RGB(2f / 3, 0.5f, 1));*/
                float BarGapWidth = (GraphWidth / (Depth * (1 + GapProportion) + GapProportion)) * (1 + GapProportion);
                BarWidth = BarGapWidth / (GapProportion + 1);
                GapWidth = BarGapWidth * GapProportion / (GapProportion + 1);
                CreateRects(CurDirStructure, 0, GraphTop, GraphTop + GraphHeight);
            }
        }

        private void PaintAllRects(Graphics g) {
            lock (ShapeCollection) {
                foreach (Shape s in ShapeCollection)
                    s.Draw(g);
            }
        }
        private Shape GetShapeFromCoords(PointF pt) {
            if (fc.IsWorking) return null;
            foreach (Shape s in ShapeCollection)
                if (s.r.Contains(pt)) return s;
            return null;
        }
        private void PaintScale(Graphics g) {
            float GridSize = CurDirStructure.Size;
            while (GridSize > 20) GridSize /= 10;
            GridSize *= 10;
        }
        private void UsageMapView_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.White, ClientRectangle);
            if (fc.IsWorking) {
                g.DrawString("Loading \"" + LastScanned + "\"...",
                    new Font("Arial", 10), Brushes.Black, new PointF(0, 0));
            }
            else if (CurDirStructure != null) {
                PaintAllRects(g);
                g.DrawRectangle(Pens.Gray, GraphLeft, GraphTop, GraphWidth, GraphHeight);
                if (ShapeUnderMouse != null && ShapeUnderMouse.fs != null) {
                    g.DrawRectangle(new Pen(Color.FromArgb(128,0,0,128), 3),
                        ShapeUnderMouse.r.X, ShapeUnderMouse.r.Y, ShapeUnderMouse.r.Width, ShapeUnderMouse.r.Height);
                }
            }
        }

        private void UsageMapView_Resize(object sender, EventArgs e) {
            CreateAllRects();
            Invalidate();
        }
        private String SizeToText(float Size) {
            string[] SizeNames = new string[] { "bytes", "kb", "mb", "gb" };
            int i;
            for (i = 0; i < 3 && Size > 1024; ++i) {
                Size /= 1024;
            }
            return string.Format("{0} {1}",Size.ToString("#,##0.00"),SizeNames[i]);
        }
        private Shape ShapeUnderMouse;
        private void UsageMapView_MouseMove(object sender, MouseEventArgs e) {
            Shape s = GetShapeFromCoords(new Point(e.X, e.Y));
            if (s==null || ShapeUnderMouse == null || s.fs.Name != ShapeUnderMouse.fs.Name) {
                ShapeUnderMouse = GetShapeFromCoords(new Point(e.X, e.Y));
                if (ShapeUnderMouse != null) {
                    string msg = ShapeUnderMouse.fs.Name;
                    if (ShapeUnderMouse.fs.IsDirectory)
                        msg += "\r\nContains: " + ShapeUnderMouse.fs.SubDirCount.ToString("#,##0") + " Dir" + (ShapeUnderMouse.fs.SubDirCount > 1 ? "s" : "")
                        + ", " + ShapeUnderMouse.fs.SubFileCount.ToString("#,##0") + " File" + (ShapeUnderMouse.fs.SubFileCount > 1 ? "s" : "");
                    msg += "\r\nSize: " + SizeToText(ShapeUnderMouse.fs.Size);
                    toolTip1.Show(msg, this, e.X, e.Y);
                }
                else toolTip1.Hide(this);
                Invalidate();
            }
        }

        private void UsageMapView_MouseDown(object sender, MouseEventArgs e) {
            Shape s = GetShapeFromCoords(new PointF(e.X, e.Y));
            if (s != null && s.fs != null && s.fs.IsDirectory) {
                CurDirStructure = s.fs;
                CurrentFile = s.fs.Name;
            }
        }
    }
}
