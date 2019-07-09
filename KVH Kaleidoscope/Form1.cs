using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Kvh.Kaleidoscope
{
    public partial class Form1 : Form
    {
        private readonly int MIN_PATTERN_SIZE = 100;
        private readonly int MAX_PATTERN_SIZE = 450;
        private readonly int MIN_IMG_SIZE = 100;
        private readonly int MAX_IMG_SIZE = 4096;

        private Bitmap img;
        private Bitmap pattern;
        private float patternWidth = 250;

        private int clipPathOffsetX;
        private int clipPathOffsetY;
        private float clipPathRotation;

        private Point clickedLocation;
        private int previousRdrSize = 250;
        private int previousClipPathOffsetX;
        private int previousClipPathOffsetY;
        private float previousClipPathRotation;

        private RenderWindow renderWindow = new RenderWindow();

        private string imgFileName;

        public Form1()
        {
            InitializeComponent();
            //GeneratePattern();
            renderWindow.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GeneratePattern();
            RenderAll();
            GC.Collect();
        }

        private void RenderAll()
        {
            toolStripStatusLabel1.Text = "Rendering...";
            toolStripStatusLabel1.Invalidate();
            toolStripStatusLabel3.Text = renderWindow.PictureBox.Width + "";
            toolStripStatusLabel5.Text = renderWindow.PictureBox.Height + "";
            toolStripStatusLabel7.Text = patternWidth + "";
            Application.DoEvents();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            renderWindow.PictureBox.Image = Kvh.Kaleidoscope.Kaleidoscope.Render(
                renderWindow.PictureBox.Width, renderWindow.PictureBox.Height, pattern);

            stopwatch.Stop();
            toolStripStatusLabel1.Text = "Rendered in " + stopwatch.ElapsedMilliseconds + " ms.";
        }

        private void LoadImage(string imgPath)
        {
            try
            {
                var tmp = new Bitmap(imgPath);
                img = tmp.Clone() as Bitmap;
                tmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading image: " + ex.Message, 
                    "Error Loading Image", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void GeneratePattern()
        {
            LoadImage(imgFileName);
            if (img == null)
                return;

            textBox1.Text = img.Width.ToString();
            textBox2.Text = img.Height.ToString();
            var scaledWidth = int.Parse(textBox3.Text);
            var scaledHeight = (int)((float) scaledWidth / img.Width * img.Height);

            pictureBox1.Width = scaledWidth + 2;
            pictureBox1.Height = scaledHeight + 2;

            this.patternWidth = int.Parse(textBox5.Text);
            textBox4.Text = scaledHeight + "";

            var clipXOffset = int.Parse(textBox6.Text);
            var clipYOffset = int.Parse(textBox7.Text);
            var angle = float.Parse(textBox8.Text);

            var imgRect = new Rectangle(0, 0, img.Width, img.Height);
            var scaledImgRect = new RectangleF(0, 0, scaledWidth, scaledHeight);

            var patternHeight = patternWidth * (float)Math.Sqrt(3) / 2;

            var clippingPath = new GraphicsPath();
            clippingPath.AddPolygon(new[] {
                new PointF(0, 0),
                new PointF(patternWidth, 0),
                new PointF(patternWidth/2, patternHeight)});

            pattern = new Bitmap((int)(Math.Round(patternWidth, 0)), (int)(Math.Round(patternHeight)));
            var gPattern = Graphics.FromImage(pattern);
            gPattern.SmoothingMode = SmoothingMode.HighQuality;
            gPattern.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gPattern.Clip = new Region(clippingPath);
            gPattern.RotateTransform(-angle);
            gPattern.TranslateTransform(-clipXOffset, -clipYOffset);
            gPattern.DrawImage(img, scaledImgRect, imgRect, GraphicsUnit.Pixel);
            gPattern.Dispose();

            var previewBmp = new Bitmap(scaledWidth, scaledHeight);
            var gPreviewBmp = Graphics.FromImage(previewBmp);
            gPreviewBmp.SmoothingMode = SmoothingMode.HighQuality;
            gPreviewBmp.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gPreviewBmp.DrawImage(img, scaledImgRect, imgRect, GraphicsUnit.Pixel);
            gPreviewBmp.TranslateTransform(clipXOffset, clipYOffset);
            gPreviewBmp.RotateTransform(angle);
            gPreviewBmp.DrawPath(new Pen(Brushes.Red), clippingPath);
            gPreviewBmp.Dispose();
            pictureBox1.Image = previewBmp;
            pictureBox2.Image = pattern;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imgFileName = openFileDialog1.FileName;
            toolStripStatusLabel8.Text = openFileDialog1.FileName;
            GeneratePattern();
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
                previousRdrSize = (int)patternWidth;
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
                patternWidth = previousRdrSize + dX;
                if (patternWidth > MAX_PATTERN_SIZE)
                    patternWidth = MAX_PATTERN_SIZE;
                else if (patternWidth < MIN_PATTERN_SIZE)
                    patternWidth = MIN_PATTERN_SIZE;

                textBox5.Text = patternWidth.ToString();
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
            GeneratePattern();
            previousUpdateLocation = e.Location;
            GC.Collect();
            isUpdating = false;
        }
    }
}
