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
            GlobalMouseHandler gmh = new GlobalMouseHandler();
            gmh.TheMouseMoved += new MouseMovedEvent(gmhMouseMoved);
            Application.AddMessageFilter(gmh);

            previewWindow.PictureBox.SizeMode = PictureBoxSizeMode.AutoSize;

            renderWindow.Show();
            renderWindow.WindowState = FormWindowState.Maximized;
            previewWindow.Show();

            InitializeComponent();
        }

        private Point lastCursorPosition;
        void gmhMouseMoved()
        {
            Point cursorPosition = Cursor.Position;
            var dis = Math.Sqrt(
                Math.Pow(cursorPosition.X - lastCursorPosition.X, 2) +
                Math.Pow(cursorPosition.Y - lastCursorPosition.Y, 2));
            Console.WriteLine(cursorPosition);
            Console.WriteLine(dis);
            lastCursorPosition = cursorPosition;

            if (dis < 3)
                return;

            if (Opacity < 1)
                Opacity = 1;
        }

        private void Render()
        {
            toolStripStatusLabel1.Text = "Rendering...";
            toolStripStatusLabel1.Invalidate();
            toolStripStatusLabel3.Text = renderWindow.PictureBox.Width + "";
            toolStripStatusLabel5.Text = renderWindow.PictureBox.Height + "";

            Application.DoEvents();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            renderWindow.PictureBox.Image = Kvh.Kaleidoscope.Kaleidoscope.Render(
                renderWindow.PictureBox.Width, renderWindow.PictureBox.Height, pattern);

            stopwatch.Stop();
            toolStripStatusLabel1.Text = "Rendered in " + stopwatch.ElapsedMilliseconds + " ms.";

            Opacity = 0.5;
        }

        private void LoadImage(string imgPath)
        {
            try
            {
                var tmp = new Bitmap(imgPath);
                img = tmp.Clone() as Bitmap;
                tmp.Dispose();

                toolStripLabel2.Text = img.Width + " × " + img.Height;

                if (IsARLocked && img != null)
                {
                    ImageScaledHeight = (int)(Math.Round((float)ImageScaledWidth / img.Width * img.Height, 0));
                }
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
            if (img == null)
                return;

            pictureBox1.Width = ImageScaledWidth + 2;
            pictureBox1.Height = ImageScaledHeight + 2;

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
            previewWindow.PictureBox.Image = pattern;

            ActivatePreviewWindowAtBottomRight();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imgFileName = openFileDialog1.FileName;
            LoadImage(imgFileName);

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
            }
            else if (isPictureBoxLMouseDown)
            {
                PatternXOffset = previousClipPathOffsetX + dX;
                PatternYOffset = previousClipPathOffsetY + dY;
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
            GeneratePattern();
            Render();
            
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

        private void toolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            GeneratePattern();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            PatternSize = random.Next(MIN_PATTERN_SIZE, Math.Min(ImageScaledWidth, imageScaledHeight));
            PatternXOffset = random.Next(0, ImageScaledWidth - PatternSize);
            PatternYOffset = random.Next(0, ImageScaledHeight - PatternSize);
            PatternRotation = random.Next(0, 300)/10f;

            GeneratePattern();
            Render();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            PatternSize = 250;
            PatternXOffset = 0;
            PatternYOffset = 0;
            PatternRotation = 0f;
            ImageScaledWidth = 400;
            ImageScaledHeight = 300;
        }

        public delegate void MouseMovedEvent();

        public class GlobalMouseHandler : IMessageFilter
        {
            private const int WM_MOUSEMOVE = 0x0200;

            public event MouseMovedEvent TheMouseMoved;

            #region IMessageFilter Members

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEMOVE)
                {
                    TheMouseMoved?.Invoke();
                }
                // Always allow message to continue to the next filter control
                return false;
            }

            #endregion
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            ActivatePreviewWindowAtBottomRight();

            previewWindow.Show();
        }

        private void ActivatePreviewWindowAtBottomRight()
        {
            var x = Location.X + Size.Width - previewWindow.Size.Width;
            var y = Location.Y + Size.Height - previewWindow.Size.Height;

            previewWindow.Location = new Point(x, y);


            if (previewWindow.Right > Screen.PrimaryScreen.WorkingArea.Right)
                x = Screen.PrimaryScreen.WorkingArea.Right - previewWindow.Size.Width;

            if (previewWindow.Bottom > Screen.PrimaryScreen.WorkingArea.Bottom)
                y = Screen.PrimaryScreen.WorkingArea.Bottom - previewWindow.Size.Height;

            previewWindow.Location = new Point(x, y);
            previewWindow.Activate();
        }
    }
}
