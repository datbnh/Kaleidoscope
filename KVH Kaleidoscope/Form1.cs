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

        private bool IsARLocked { get => toolStripButton3.Checked;
            set => toolStripButton3.Checked = value; }
        public int ImageScaledWidth {
            get => imageScaledWidth;
            set { imageScaledWidth = value; toolStripTextBox3.Text = value.ToString(); }
        }
        public int ImageScaledHeight {
            get => imageScaledHeight;
            set { imageScaledHeight = value; toolStripTextBox4.Text = value.ToString(); }
        }
        public int PatternSize {
            get => patternSize;
            set { patternSize = value; toolStripTextBox1.Text = value.ToString(); }
        }
        public int PatternXOffset {
            get => patternXOffset;
            set { patternXOffset = value; toolStripTextBox2.Text = value.ToString(); }
        }
        public int PatternYOffset {
            get => patternYOffset;
            set { patternYOffset = value; toolStripTextBox5.Text = value.ToString(); }
        }
        public float PatternRotation {
            get => patternRotation;
            set { patternRotation = value; toolStripTextBox6.Text = value.ToString(); }
        }

        private int imageScaledWidth;
        private int imageScaledHeight;
        private int patternSize;
        private int patternXOffset;
        private int patternYOffset;
        private float patternRotation;


        private Bitmap img;
        private Bitmap pattern;
        //private float patternWidth = 250;

        //private int clipPathOffsetX;
        //private int clipPathOffsetY;
        //private float clipPathRotation;

        private Point clickedLocation;
        private int previousRdrSize = 250;
        private int previousClipPathOffsetX;
        private int previousClipPathOffsetY;
        private float previousClipPathRotation;

        private RenderWindow renderWindow = new RenderWindow();
        private RenderWindow previewWindow = new RenderWindow()
        {
            Text = "Preview",
            ControlBox = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };

        private string imgFileName;

        public Form1()
        {
            InitializeComponent();
            //GeneratePattern();
            renderWindow.Show();
            previewWindow.Show();
            previewWindow.PictureBox.SizeMode = PictureBoxSizeMode.AutoSize;

            PatternSize = 250;
            PatternXOffset = 0;
            PatternYOffset = 0;
            PatternRotation = 0f;
            ImageScaledWidth = 400;
            ImageScaledHeight = 300;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (img == null)
                return;
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
            //toolStripStatusLabel7.Text = patternWidth + "";
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
                toolStripLabel2.Text = img.Width + " × " + img.Height;
                tmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading image: " + ex.Message,
                    "Error Loading Image", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                toolStripLabel2.Text = "Invalid Image.";
            }
        }

        private void GeneratePattern()
        {
            LoadImage(imgFileName);
            if (img == null)
                return;

            //textBox1.Text = img.Width.ToString();
            //textBox2.Text = img.Height.ToString();
            //var scaledWidth = int.Parse(textBox3.Text);
            //var scaledHeight = (int)((float)scaledWidth / img.Width * img.Height);

            //pictureBox1.Width = scaledWidth + 2;
            //pictureBox1.Height = scaledHeight + 2;

            pictureBox1.Width = ImageScaledWidth + 2;
            pictureBox1.Height = ImageScaledHeight + 2;

            //patternWidth = int.Parse(textBox5.Text);
            //textBox4.Text = scaledHeight + "";

            //var clipXOffset = int.Parse(textBox6.Text);
            //var clipYOffset = int.Parse(textBox7.Text);
            //var angle = float.Parse(textBox8.Text);
            var angle = float.Parse(toolStripTextBox6.Text);

            var imgRect = new Rectangle(0, 0, img.Width, img.Height);
            var scaledImgRect = new RectangleF(0, 0, ImageScaledWidth, ImageScaledHeight);

            var patternWidth = PatternSize;
            var patternHeight = patternWidth * (float)Math.Sqrt(3) / 2;

            var clippingPath = new GraphicsPath();
            clippingPath.AddPolygon(new[] {
                new PointF(0, 0),
                new PointF(patternWidth, 0),
                new PointF(patternWidth/2, patternHeight)});

            pattern = new Bitmap(patternWidth, (int)(Math.Round(patternHeight)));
            var gPattern = Graphics.FromImage(pattern);
            gPattern.SmoothingMode = SmoothingMode.HighQuality;
            gPattern.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gPattern.Clip = new Region(clippingPath);
            gPattern.RotateTransform(-angle);
            gPattern.TranslateTransform(-PatternXOffset, -PatternYOffset);
            gPattern.DrawImage(img, scaledImgRect, imgRect, GraphicsUnit.Pixel);
            gPattern.Dispose();

            var previewBmp = new Bitmap(ImageScaledWidth, ImageScaledHeight);
            var gPreviewBmp = Graphics.FromImage(previewBmp);
            gPreviewBmp.SmoothingMode = SmoothingMode.HighQuality;
            gPreviewBmp.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gPreviewBmp.DrawImage(img, scaledImgRect, imgRect, GraphicsUnit.Pixel);
            gPreviewBmp.TranslateTransform(PatternXOffset, PatternYOffset);
            gPreviewBmp.RotateTransform(angle);
            gPreviewBmp.DrawPath(new Pen(Brushes.Red), clippingPath);
            gPreviewBmp.Dispose();

            pictureBox1.Image = previewBmp;
            //pictureBox2.Image = pattern;

            previewWindow.PictureBox.Image = pattern;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imgFileName = openFileDialog1.FileName;
            //toolStripStatusLabel8.Text = openFileDialog1.FileName;
            var MAX_LEN = 32;
            var fileFullPath = openFileDialog1.FileName;
            if (fileFullPath.Length > MAX_LEN)
            {
                if (openFileDialog1.SafeFileName.Length > MAX_LEN)
                    toolStripLabel1.Text = "..." + openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - MAX_LEN);
                else
                {
                    var len = MAX_LEN - openFileDialog1.SafeFileName.Length;
                    toolStripLabel1.Text = fileFullPath.Substring(0, len) + "...\\" + openFileDialog1.SafeFileName;
                }
            }
            else
            {
                toolStripLabel1.Text = fileFullPath;
            }
            toolStripLabel1.ToolTipText = fileFullPath;

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
                previousRdrSize = PatternSize;
            }

            clickedLocation = e.Location;
            previousClipPathOffsetX = PatternXOffset;
            previousClipPathOffsetY = PatternYOffset;
            previousClipPathRotation = PatternRotation;
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
                if (previousRdrSize + dX > MAX_PATTERN_SIZE)
                    PatternSize = MAX_PATTERN_SIZE;
                else if (previousRdrSize + dX < MIN_PATTERN_SIZE)
                    PatternSize = MIN_PATTERN_SIZE;
                else
                    PatternSize = previousRdrSize + dX;

                //textBox5.Text = patternWidth.ToString();
                //toolStripTextBox1.Text = patternWidth.ToString();
            }
            else if (isPictureBoxLMouseDown)
            {
                PatternXOffset = previousClipPathOffsetX + dX;
                PatternYOffset = previousClipPathOffsetY + dY;
                //textBox6.Text = clipPathOffsetX.ToString();
                //textBox7.Text = clipPathOffsetY.ToString();
                //toolStripTextBox2.Text = clipPathOffsetX.ToString();
                //toolStripTextBox5.Text = clipPathOffsetY.ToString();
            }
            else
            {
                var rotation = previousClipPathRotation - dX / 10f;
                if (rotation > 360)
                    PatternRotation = rotation - 360;
                else if (rotation < 0)
                    PatternRotation = rotation + 360;
                else
                    PatternRotation = rotation;

                //textBox8.Text = clipPathRotation.ToString("0.00");
                //toolStripTextBox6.Text = rotation.ToString("0.00");
            }

            isUpdating = true;
            GeneratePattern();
            previousUpdateLocation = e.Location;
            GC.Collect();
            isUpdating = false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (img == null)
                return;
            GeneratePattern();
            RenderAll();
            GC.Collect();
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            isPictureBoxLMouseDown = false;
            isPictureBoxRMouseDown = false;
        }

        private void toolStripLabel10_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        #region text box validating

        private void toolStripTextBox3_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                 MIN_IMG_SIZE, MAX_IMG_SIZE,
                 "Width", ref imageScaledWidth, toolStrip2))
                e.Cancel = true;
            else if (IsARLocked && img != null)
            {
                ImageScaledHeight = (int)(Math.Round((float)ImageScaledWidth / img.Width * img.Height, 0));
                //toolStripTextBox4.Text = ImageScaledHeight.ToString();
            }
        }

        private void toolStripTextBox4_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                MIN_IMG_SIZE, MAX_IMG_SIZE,
                "Height", ref imageScaledHeight, toolStrip2))
                e.Cancel = true;
            else if (IsARLocked && img != null)
            {
                ImageScaledWidth = (int)(Math.Round((float)ImageScaledHeight / img.Height * img.Width, 0));
                //toolStripTextBox3.Text = ImageScaledWidth.ToString();
            }
        }

        private void toolStripTextBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                MIN_PATTERN_SIZE, MAX_PATTERN_SIZE,
                "Size", ref patternSize, toolStrip4))
                e.Cancel = true;
        }

        
        private void toolStripTextBox2_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                int.MinValue, int.MaxValue,
                "X Offset", ref patternXOffset, toolStrip4))
                e.Cancel = true;
        }

        private void toolStripTextBox5_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                int.MinValue, int.MaxValue,
                "Y Offset", ref patternYOffset, toolStrip4))
                e.Cancel = true;
        }

        private void toolStripTextBox6_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private bool ValidateInt(string text, int min, int max, string valueLabel, ref int valueToUpdate, Control control)
        {
            if (!int.TryParse(text, out int tmp))
            {
                errorProvider1.SetError(control, "Invalid " + valueLabel + ".");
            }
            else if (tmp < min)
            {
                errorProvider1.SetError(control, valueLabel + " must not smaller than " + min + ".");
            }
            else if (tmp > max)
            {
                errorProvider1.SetError(control, valueLabel + " must not larger than " + MAX_IMG_SIZE + ".");
            }
            else
            {
                errorProvider1.SetError(control, "");
                valueToUpdate = tmp;
                return true;
            }
            return false;
        }



        #endregion

        private void toolStripTextBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            PatternSize = random.Next(MIN_PATTERN_SIZE, Math.Min(ImageScaledWidth, imageScaledHeight));
            PatternXOffset = random.Next(0, ImageScaledWidth - PatternSize);
            PatternYOffset = random.Next(0, ImageScaledHeight - PatternSize);
            PatternRotation = random.Next(0, 300)/10f;
            GeneratePattern();
        }
    }
}
