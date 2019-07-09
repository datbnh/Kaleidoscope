using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KaDoscope_Beta0
{
    public partial class Form1 : Form
    {
        private readonly int MIN_PATTERN_SIZE = 100;
        private readonly int MAX_PATTERN_SIZE = 450;
        private readonly int MIN_IMG_SIZE = 100;
        private readonly int MAX_IMG_SIZE = 4096;

        private readonly float MIN_PATTERN_ROTATION = 0;
        private readonly float MAX_PATTERN_ROTATION = 360f;

        private Bitmap img;

        private Bitmap patternOriginal;
        private Bitmap patternFlippedLR;

        private float rdrSize = 250;
        private float rdrWidth;
        private float rdrHeight;

        private float xOffset;
        private float yOffset;

        private int clipPathOffsetX;
        private int clipPathOffsetY;
        private float clipPathRotation;

        private Point clickedLocation;
        private int previousRdrSize = 250;
        private int previousClipPathOffsetX;
        private int previousClipPathOffsetY;
        private float previousClipPathRotation;

        private RenderWindow renderWindow = new RenderWindow();

        private string imgFileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\P4484_710-03456_01.jpg";

        public Form1()
        {
            InitializeComponent();
            refreshSeed();
            renderWindow.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            refreshSeed();
            RenderAll();
            System.GC.Collect();
        }

        private void RenderAll()
        {
            toolStripStatusLabel1.Text = "Rendering...";
            toolStripStatusLabel1.Invalidate();
            toolStripStatusLabel3.Text = renderWindow.PictureBox.Width + "";
            toolStripStatusLabel5.Text = renderWindow.PictureBox.Height + "";
            toolStripStatusLabel7.Text = rdrSize + "";
            Application.DoEvents();

            var nTotalRows = (int)Math.Round(renderWindow.PictureBox.Height / yOffset, 0) + 2;
            var nTotalCols = (int)Math.Round(renderWindow.PictureBox.Width / xOffset / 3, 0) + 2;
            var nTop = (int)Math.Round(nTotalRows / 2f, 0);
            var nLeft = (int)Math.Round(nTotalCols / 2f, 0);;

            var g = renderWindow.PictureBox.CreateGraphics();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Gray);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            g.TranslateTransform(renderWindow.PictureBox.Width / 2, renderWindow.PictureBox.Height / 2);
            //g.RotateTransform(clipPathRotation);
            if (nTop > 0)
            {
                g.TranslateTransform(0, -nTop * yOffset);
            }
            for (var i = 0; i < nTotalRows; i++)
            {
                if (i % 2 != 0)
                {
                    g.TranslateTransform(-1.5f * xOffset, 0);
                    DrawRow(g, nLeft, nTotalCols + 1, xOffset, yOffset);
                    g.TranslateTransform(1.5f * xOffset, 0);
                }
                else
                {
                    DrawRow(g, nLeft, nTotalCols, xOffset, yOffset);
                }
                g.TranslateTransform(0, yOffset);
            }
            g.Dispose();

            stopwatch.Stop();
            toolStripStatusLabel1.Text = "Rendered in " + stopwatch.ElapsedMilliseconds + " ms.";
        }

        private void DrawImageAtPointAndAngle(Graphics g, Bitmap bmp, float x, float y, float degree)
        {
            g.TranslateTransform(x, y);
            g.RotateTransform(degree);
            g.DrawImage(bmp, 0, 0);
            g.RotateTransform(-degree);
            g.TranslateTransform(-x, -y);
        }

        private void DrawSet(Graphics g, float xOffset, float yOffset)
        {
            DrawImageAtPointAndAngle(g, patternOriginal, 0, 0, 0);
            DrawImageAtPointAndAngle(g, patternFlippedLR, 1 * xOffset, 0, 60);
            DrawImageAtPointAndAngle(g, patternOriginal, 1.5f * xOffset, yOffset, 240);
            DrawImageAtPointAndAngle(g, patternFlippedLR, 2.5f * xOffset, yOffset, 180);
            DrawImageAtPointAndAngle(g, patternOriginal, 3 * xOffset, 0, 120);
            DrawImageAtPointAndAngle(g, patternFlippedLR, 2.5f * xOffset, yOffset, 300);
        }

        private void DrawRow(Graphics g, int nLeft, int nTotalCols, float xOffset, float yOffset)
        {
            var setXOffset = xOffset * 3;
            if (nLeft > 0)
            {
                g.TranslateTransform(-nLeft * setXOffset, 0);
            }
            for (var i=0; i<nTotalCols; i++)
            {
                DrawSet(g, xOffset, yOffset);
                g.TranslateTransform(setXOffset, 0);
            }
            g.TranslateTransform(-(nTotalCols - nLeft) * setXOffset, 0);
        }

        private int floor(float number)
        {
            return (int)Math.Floor(number);
        }

        private void loadImage(string imgPath)
        {
            var tmp = new Bitmap(imgPath);
            img = tmp.Clone() as Bitmap;
            tmp.Dispose();
        }

        private void refreshSeed()
        {
            loadImage(imgFileName);

            textBox1.Text = img.Width.ToString();
            textBox2.Text = img.Height.ToString();
            var scaledWidth = int.Parse(textBox3.Text);
            var scaledHeight = (int)((float) scaledWidth / img.Width * img.Height);

            pictureBox1.Width = scaledWidth + 2;
            pictureBox1.Height = scaledHeight + 2;

            rdrSize = int.Parse(textBox5.Text);
            textBox4.Text = scaledHeight + "";

            var clipXOffset = int.Parse(textBox6.Text);
            var clipYOffset = int.Parse(textBox7.Text);
            var angle = float.Parse(textBox8.Text);

            rdrWidth = rdrSize;
            rdrHeight = rdrSize * (float)Math.Sqrt(3) / 2;
            //pictureBox2.Width = (int)rdrWidth + 2;
            //pictureBox2.Height = (int)rdrHeight + 2;
            //var rdrRect = new RectangleF(0, 0, rdrWidth, rdrHeight);

            var imgRect = new Rectangle(0, 0, img.Width, img.Height);
            var scaledImgRect = new RectangleF(0, 0, scaledWidth, scaledHeight);
            
            var clippingPath = new GraphicsPath();
            clippingPath.AddPolygon(new[] {
                new PointF(0, 0),
                new PointF(rdrWidth, 0),
                new PointF(rdrWidth/2, rdrHeight)});

            patternOriginal = new Bitmap(floor(rdrWidth), floor(rdrHeight));
            var gPattern = Graphics.FromImage(patternOriginal);
            gPattern.SmoothingMode = SmoothingMode.HighQuality;
            gPattern.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gPattern.Clip = new Region(clippingPath);
            gPattern.RotateTransform(-angle);
            gPattern.TranslateTransform(-clipXOffset, -clipYOffset);
            gPattern.DrawImage(img, scaledImgRect, imgRect, GraphicsUnit.Pixel);
            gPattern.Dispose();

            patternFlippedLR = patternOriginal.Clone() as Bitmap;
            patternFlippedLR.RotateFlip(RotateFlipType.RotateNoneFlipX);

            var overlappingFactor = 0.999f;

            xOffset = rdrWidth * overlappingFactor;// * 0.99f;
            yOffset = rdrHeight * overlappingFactor;// * 0.99f;

            var previewBmp = new Bitmap(scaledWidth, scaledHeight);
            var gPreviewBmp = Graphics.FromImage(previewBmp);
            gPreviewBmp.SmoothingMode = SmoothingMode.HighQuality;
            gPreviewBmp.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //gPreviewBmp.TranslateTransform(-clipXOffset, -clipYOffset);
            gPreviewBmp.DrawImage(img, scaledImgRect, imgRect, GraphicsUnit.Pixel);
            gPreviewBmp.TranslateTransform(clipXOffset, clipYOffset);
            gPreviewBmp.RotateTransform(angle);
            gPreviewBmp.DrawPath(new Pen(Brushes.Red), clippingPath);

            //gPreviewBmp.RotateTransform(-angle);
            //gPreviewBmp.TranslateTransform(-clipXOffset, -clipYOffset);
            //gPreviewBmp.DrawImage(patternOriginal, 0, scaledHeight + 10);
            gPreviewBmp.Dispose();
            pictureBox1.Image = previewBmp;
            pictureBox2.Image = patternOriginal;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imgFileName = openFileDialog1.FileName;
            toolStripStatusLabel8.Text = openFileDialog1.FileName;
            refreshSeed();
        }

        private bool isPictureBoxLMouseDown = false;
        private bool isPictureBoxRMouseDown = false;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel1.Text = e.Button.ToString();

            if (e.Button == MouseButtons.Left)
                isPictureBoxLMouseDown = true;
            else if (e.Button == MouseButtons.Right)
                isPictureBoxRMouseDown = true;

            if (isPictureBoxLMouseDown && isPictureBoxRMouseDown)
            {
                previousRdrSize = (int)rdrSize;
            }

            clickedLocation = e.Location;
            previousClipPathOffsetX = clipPathOffsetX;
            previousClipPathOffsetY = clipPathOffsetY;
            previousClipPathRotation = clipPathRotation;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                isPictureBoxLMouseDown = false;
            if (e.Button == MouseButtons.Right)
                isPictureBoxRMouseDown = false;
        }


        private bool isUpdating = false;
        private Point previousUpdateLocation;
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            if (!isPictureBoxLMouseDown && !isPictureBoxRMouseDown)
                return;

            if (previousUpdateLocation != null)
                if (e.Location == previousUpdateLocation)
                    return;

            if (isUpdating)
            {
                Console.WriteLine("Skipped");
                return;
            }

            var dX = e.X - clickedLocation.X;
            var dY = e.Y - clickedLocation.Y;

            if (isPictureBoxLMouseDown && isPictureBoxRMouseDown)
            {
                rdrSize = previousRdrSize + dX;
                if (rdrSize > MAX_PATTERN_SIZE)
                    rdrSize = MAX_PATTERN_SIZE;
                else if (rdrSize < MIN_PATTERN_SIZE)
                    rdrSize = MIN_PATTERN_SIZE;

                textBox5.Text = rdrSize.ToString();
            }
            else if (isPictureBoxLMouseDown)
            {
                clipPathOffsetX = previousClipPathOffsetX + dX;
                clipPathOffsetY = previousClipPathOffsetY + dY;
                textBox6.Text = clipPathOffsetX.ToString();
                textBox7.Text = clipPathOffsetY.ToString();
            }
            else
            {
                clipPathRotation = previousClipPathRotation - dX/10f;
                if (clipPathRotation > 360)
                    clipPathRotation = clipPathRotation - 360;
                else if (clipPathRotation < 0)
                    clipPathRotation = clipPathRotation + 360;

                textBox8.Text = clipPathRotation.ToString("0.00");
            }

            isUpdating = true;
            refreshSeed();
            previousUpdateLocation = e.Location;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff"));
            GC.Collect();
            isUpdating = false;
        }
    }
}
