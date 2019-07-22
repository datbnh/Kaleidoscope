#define DEBUG

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Kvh.Kaleidoscope
{
    public partial class Form1 : Form
    {
        private static Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private static DateTime buildDate = new DateTime(2000, 1, 1)
                                .AddDays(version.Build).AddSeconds(version.Revision * 2);
        private static string name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        string softwareInfo = $"{name} {version} by Đạt Bùi\r\n(Built {buildDate})";

        private readonly int MIN_PATTERN_SIZE = 100;
        private readonly int MAX_PATTERN_SIZE = 450;
        private readonly int MIN_IMG_SIZE = 100;
        private readonly int MAX_IMG_SIZE = 4096;

        private bool IsARLocked { get => toolStripButton3.Checked; set => toolStripButton3.Checked = value; }
        public int ImageScaledWidth
        {
            get => imageScaledWidth;
            set { imageScaledWidth = value; toolStripTextBox3.Text = value.ToString(); }
        }
        public int ImageScaledHeight
        {
            get => imageScaledHeight;
            set { imageScaledHeight = value; toolStripTextBox4.Text = value.ToString(); }
        }
        public int PatternSize
        {
            get => patternSize;
            set { patternSize = value; toolStripTextBox1.Text = value.ToString(); }
        }
        public int PatternXOffset
        {
            get => patternXOffset;
            set { patternXOffset = value; toolStripTextBox2.Text = value.ToString(); }
        }
        public int PatternYOffset
        {
            get => patternYOffset;
            set { patternYOffset = value; toolStripTextBox5.Text = value.ToString(); }
        }
        public float PatternRotation
        {
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
        private KaleidoscopeTemplate template;
        private KaleidoscopeTemplateExtractor kaleidoscopeTemplateExtractor;
        //private Bitmap pattern;

        private Point clickedLocation;
        private int previousPatternSize = 250;
        private int previousClipPathOffsetX;
        private int previousClipPathOffsetY;
        private float previousClipPathRotation;

        private RenderWindow renderWindow = new RenderWindow();
        private RenderWindow previewWindow = new RenderWindow()
        {
            Text = "Preview - KVH",
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

            InitializeComponent();

            kaleidoscopeTemplateExtractor = new KaleidoscopeTemplateExtractor(SmoothingMode.HighQuality,
                PixelOffsetMode.HighQuality, InterpolationMode.HighQualityBicubic);

            var filter =
                "Joint Photographic Experts Group|*.jpg" +
                "|Portable Network Graphics|*.png" +
                "|BMP|*.bmp" +
                "|Graphics Interchange Format|*.gif" +
                "|Exchangeable Image File|*.exif" +
                "|Tag Image File Format|*.tiff";
            var filterOpen = "|All Picture Files|*.jpg;*.png;*.bmp;*.gif;*.exif;*.tiff|All Files|*.*";
            saveFileDialog1.Filter = filter;
            saveFileDialog1.DefaultExt = "jpg";
            saveFileDialog1.FilterIndex = 1;
            openFileDialog1.Filter = filter + filterOpen;
            openFileDialog1.FilterIndex = 7;
        }

        private Point lastCursorPosition;
        void gmhMouseMoved()
        {
            Point cursorPosition = Cursor.Position;
            var dis = Math.Sqrt(
                Math.Pow(cursorPosition.X - lastCursorPosition.X, 2) +
                Math.Pow(cursorPosition.Y - lastCursorPosition.Y, 2));
            //Debug(cursorPosition + " -> Distance = " + dis);

            lastCursorPosition = cursorPosition;

            if (dis < 5)
                return;

            if (Opacity < 1)
            {
                Opacity = 1;
                previewWindow.Opacity = 1;
            }
        }

        private void Render()
        {
            SetStatus("Rendering...");
            this.Cursor = Cursors.WaitCursor;
            toolStripStatusLabel3.Text = renderWindow.PictureBox.Width + "";
            toolStripStatusLabel5.Text = renderWindow.PictureBox.Height + "";

            Application.DoEvents();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var tmp = Kaleidoscope.Render(
                renderWindow.PictureBox.Width, renderWindow.PictureBox.Height, template);

            renderWindow.PictureBox.Image = tmp;

            stopwatch.Stop();
            var renderTime = stopwatch.ElapsedMilliseconds;
            SetStatus("Rendered in " + renderTime + " ms.");

            SetStatus("Rendered in " + renderTime + " ms. Filling gaps...");
            Application.DoEvents();
            stopwatch.Restart();
            Kaleidoscope.FillVoid(tmp);
            stopwatch.Stop();
            SetStatus("Rendered in " + renderTime + " ms. Filled gaps in " + stopwatch.ElapsedMilliseconds + " ms.");
            renderWindow.PictureBox.Image = tmp;
            this.Cursor = Cursors.Default;
            Application.DoEvents();

            Opacity = 0.25;
            if (!renderWindow.Visible)
                renderWindow.Show();
            previewWindow.Opacity = 0.25;
        }

        private void SetStatus(string statusText)
        {
            toolStripStatusLabel1.Text = statusText;
            toolStripStatusLabel1.IsLink = false;
            toolStripStatusLabel1.Invalidate();
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

            // TODO
            var angle = float.Parse(toolStripTextBox6.Text);

            template = kaleidoscopeTemplateExtractor.Extract(img, imageScaledWidth,
                imageScaledHeight, PatternSize, PatternXOffset, PatternYOffset, PatternRotation, pictureBox1);

            previewWindow.PictureBox.Image = template.Bitmap;

            ActivatePreviewWindowAtBottomRight();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imgFileName = openFileDialog1.FileName;
            LoadImage(imgFileName);

            var MAX_LEN = 32;
            var fileFullPath = openFileDialog1.FileName;

            toolStripLabel1.Text = TrimFilePath(fileFullPath, MAX_LEN);
            toolStripLabel1.ToolTipText = fileFullPath;

            GeneratePattern();
        }

        private string TrimFilePath(string fileFullPath, int maxLength)
        {
            var fileName = System.IO.Path.GetFileName(fileFullPath);
            if (fileFullPath.Length > maxLength)
            {
                if (fileName.Length > maxLength)
                    return "..." + fileFullPath.Substring(fileFullPath.Length - maxLength);
                else
                {
                    var len = maxLength - fileName.Length;
                    return fileFullPath.Substring(0, len) + "...\\" + fileName;
                }
            }
            else
                return fileFullPath;
        }

        private bool isPictureBoxLMouseDown = false;
        private bool isPictureBoxRMouseDown = false;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //Debug(e.Button.ToString());

            if (e.Button == MouseButtons.Left)
                isPictureBoxLMouseDown = true;
            else if (e.Button == MouseButtons.Right)
                isPictureBoxRMouseDown = true;

            if (isPictureBoxLMouseDown && isPictureBoxRMouseDown)
            {
                previousPatternSize = PatternSize;
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
                Debug("Skipped");
                return;
            }

            var dX = e.X - clickedLocation.X;
            var dY = e.Y - clickedLocation.Y;

            if (isPictureBoxLMouseDown && isPictureBoxRMouseDown)
            {
                if (previousPatternSize + dX > MAX_PATTERN_SIZE)
                    PatternSize = MAX_PATTERN_SIZE;
                else if (previousPatternSize + dX < MIN_PATTERN_SIZE)
                    PatternSize = MIN_PATTERN_SIZE;
                else
                    PatternSize = previousPatternSize + dX;
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
            var previousValue = imageScaledWidth;
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                 MIN_IMG_SIZE, MAX_IMG_SIZE,
                 "Width", ref imageScaledWidth, toolStrip2))
                e.Cancel = true;
            else if (IsARLocked && img != null)
            {
                if (previousValue != imageScaledWidth)
                {
                    ImageScaledHeight = (int)(Math.Round((float)ImageScaledWidth / img.Width * img.Height, 0));
                    GeneratePattern();
                }
            }
        }

        private void toolStripTextBox4_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var previousValue = imageScaledHeight;
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                MIN_IMG_SIZE, MAX_IMG_SIZE,
                "Height", ref imageScaledHeight, toolStrip2))
                e.Cancel = true;
            else if (IsARLocked && img != null)
            {
                if (previousValue != imageScaledHeight)
                {
                    ImageScaledWidth = (int)(Math.Round((float)ImageScaledHeight / img.Height * img.Width, 0));
                    GeneratePattern();
                }
            }
        }

        private void toolStripTextBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var previousValue = patternSize;
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                MIN_PATTERN_SIZE, MAX_PATTERN_SIZE,
                "Size", ref patternSize, toolStrip4))
                e.Cancel = true;
            else if (previousValue != patternSize)
                GeneratePattern();
        }

        private void toolStripTextBox2_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var previousValue = patternXOffset;
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                int.MinValue, int.MaxValue,
                "X Offset", ref patternXOffset, toolStrip4))
                e.Cancel = true;
            else if (previousValue != patternXOffset)
                GeneratePattern();
        }

        private void toolStripTextBox5_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var previousValue = patternYOffset;
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                int.MinValue, int.MaxValue,
                "Y Offset", ref patternYOffset, toolStrip4))
                e.Cancel = true;
            else if (previousValue != patternYOffset)
                GeneratePattern();
        }

        private void toolStripTextBox6_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // TODO: validate angle
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

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            PatternSize = random.Next(MIN_PATTERN_SIZE, Math.Min(ImageScaledWidth, imageScaledHeight));
            PatternXOffset = random.Next(0, ImageScaledWidth - PatternSize);
            PatternYOffset = random.Next(0, ImageScaledHeight - PatternSize);
            PatternRotation = random.Next(0, 300) / 10f;

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

            renderWindow.Show();
            renderWindow.WindowState = FormWindowState.Maximized;
            //previewWindow.Show();

            Activate();
            PrintWatermark(panel1.CreateGraphics());
            //PrintWatermark();
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
            previewWindow.Show();
            ActivatePreviewWindowAtBottomRight();
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

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (renderWindow.PictureBox.Image == null)
                return;

            saveFileDialog1.InitialDirectory =
                 System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
            saveFileDialog1.FileName =
                "Kaleidoscope_" +
                System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName);

            var format = ImageFormat.Jpeg;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                    case ".png":
                        format = ImageFormat.Png;
                        break;
                    case ".exif":
                        format = ImageFormat.Exif;
                        break;
                    case ".tiff":
                        format = ImageFormat.Tiff;
                        break;
                }
                try
                {
                    renderWindow.PictureBox.Image.Save(saveFileDialog1.FileName, format);
                    toolStripStatusLabel1.Text = "Image has been saved to " + TrimFilePath(saveFileDialog1.FileName, 64) + ".";
                    toolStripStatusLabel1.Tag = saveFileDialog1.FileName;
                    toolStripStatusLabel1.IsLink = true;
                    toolStripStatusLabel1.LinkVisited = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving image: " + ex.Message,
                        "Error Saving Image", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

        }

        private void Debug(string msg)
        {
#if DEBUG
            toolStripStatusLabel1.Text = msg;
#endif
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            if (toolStripStatusLabel1.IsLink)
            {
                Process.Start("EXPLORER.EXE", "/select, \"" + toolStripStatusLabel1.Tag.ToString() + "\"");
                toolStripStatusLabel1.LinkVisited = true;
            }
        }

        private void PrintWatermark(Graphics g)
        {
            int margin = 5;
            Rectangle rect = new Rectangle(margin, margin,
                (int)g.VisibleClipBounds.Width - 2 * margin, (int)g.VisibleClipBounds.Height - 2 * margin);
            g.Clear(panel1.BackColor);
            Debug(rect.ToString());
            using (Font font = new Font("Segoe UI", 9, FontStyle.Italic, GraphicsUnit.Point))
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.LineAlignment = StringAlignment.Far;
                    sf.Alignment = StringAlignment.Far;
                    g.DrawString(softwareInfo, font, Brushes.Black, rect, sf);
                }
            }
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            PrintWatermark(panel1.CreateGraphics());
        }
    }
}
