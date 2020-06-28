using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Kvh.Kaleidoscope
{
    public partial class MVC_View : Form
    {
        public MVC_Controller Controller;
        public MVC_Model Model;

        /* Caution: changing the order of below fields related to software info 
         * may results in runtime error and/or incorrect softwareInfo data.
         * Prefixes were added to prevent CodeMaid from chaging such order. */
        private static readonly Version _a_version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly string _b_assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly DateTime _c_buildDate = new DateTime(2000, 1, 1).AddDays(_a_version.Build).AddSeconds(_a_version.Revision * 2);
        private static readonly string _d_softwareInfo = $"{_b_assemblyName} {_a_version} by Đạt Bùi\r\n(Built {_c_buildDate})";

        private readonly int MAX_IMG_SIZE = 4096;
        private readonly int MAX_PATTERN_SIZE = 450;
        private readonly int MIN_IMG_SIZE = 100;
        private readonly int MIN_PATTERN_SIZE = 100;
        private readonly RenderWindow previewWindow = new RenderWindow()
        {
            Text = "Preview - KVH",
            ControlBox = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };

        internal void UpdateMirrorSystem()
        {
            if (Model.MirrorSystem is MirrorSystem606060)
                toolStripDropDownButton1.Text = "60-60-60 Triangle";
            else if (Model.MirrorSystem is MirrorSystem306090)
                toolStripDropDownButton1.Text = "30-60-90 Triangle";
            else
                toolStripDropDownButton1.Text = "Unknown Mirror System";
        }

        private readonly RenderWindow renderWindow = new RenderWindow();
        private Point clickedLocation;
        private int imageScaledHeight;
        private int imageScaledWidth;
        private bool isPictureBoxLMouseDown = false;
        private bool isPictureBoxRMouseDown = false;
        private bool isUpdating = false;
        private Point lastCursorPosition;
        private float patternRotation;
        private int patternSize;
        private int patternXOffset;
        private int patternYOffset;
        private int previousClipPathOffsetX;
        private int previousClipPathOffsetY;
        private float previousClipPathRotation;
        private int previousPatternSize = 250;
        private Point previousUpdateLocation;
        public MVC_View()
        {
            InitializeGlobalMouseHandler();
            InitializePreviewWindow();
            InitializeComponent();
            InitializeFileDialogs();
        }

        public delegate void MouseMovedEvent();

        public int ImageScaledHeight
        {
            get => imageScaledHeight;
            set { imageScaledHeight = value; toolStripTextBoxScaledHeight.Text = value.ToString(); }
        }

        public int ImageScaledWidth
        {
            get => imageScaledWidth;
            set { imageScaledWidth = value; toolStripTextBoxScaledWidth.Text = value.ToString(); }
        }

        public float PatternRotation
        {
            get => patternRotation;
            set { patternRotation = value; toolStripTextBoxRotation.Text = value.ToString(); }
        }

        public int PatternSize
        {
            get => patternSize;
            set { patternSize = value; toolStripTextBoxClippingSize.Text = value.ToString(); }
        }

        public int PatternXOffset
        {
            get => patternXOffset;
            set { patternXOffset = value; toolStripTextBoxXOffset.Text = value.ToString(); }
        }

        public int PatternYOffset
        {
            get => patternYOffset;
            set { patternYOffset = value; toolStripTextBoxYOffset.Text = value.ToString(); }
        }

        private bool IsARLocked { get => toolStripButtonLockAR.Checked; set => toolStripButtonLockAR.Checked = value; }

        internal void DisplayError(string errorMessage, Exception ex)
        {
            MessageBox.Show(errorMessage + ":" + Environment.NewLine
                + ex.ToString(), errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Error);
            toolStripStatusLabel1.Text = errorMessage;
        }

        internal void UpdateRenderedImage()
        {
            renderWindow.PictureBox.Image = Model.RenderedImage;
        }

        internal void UpdateSavedImageInfo()
        {
            toolStripStatusLabel1.Text = "Image has been saved to " +
                Model.RenderedImageFullPath.TrimFilePath(64) + ".";
            toolStripStatusLabel1.Tag = Model.RenderedImageFullPath;
            toolStripStatusLabel1.IsLink = true;
            toolStripStatusLabel1.LinkVisited = false;
        }

        internal void UpdateScaledImage()
        {
            pictureBox1.Image = Model.ScaledImage;
            toolStripTextBoxScaledWidth.Text = Model.ScaledImage.Width.ToString();
            toolStripTextBoxScaledHeight.Text = Model.ScaledImage.Height.ToString();
        }

        internal void UpdateSourceImageInfo()
        {
            toolStripLabelSourceImage.Text = Model.SourceImageFullPath.TrimFilePath(32);
            toolStripLabelSourceImage.ToolTipText = Model.SourceImageFullPath;
        }

        internal void UpdateTemplateFinder(Bitmap templateFinderBitmap)
        {
            pictureBox1.Image = templateFinderBitmap;
        }

        internal void UpdateTemplatePreviewWindow()
        {
            if (previewWindow.Visible)
                previewWindow.PictureBox.Image = Model.Template;
        }

        private void ActivatePreviewWindowAtTopRightSide()
        {
            var x = Location.X + Size.Width;// - previewWindow.Size.Width;
            var y = Location.Y;// + Size.Height - previewWindow.Size.Height;

            previewWindow.Location = new Point(x, y);

            if (previewWindow.Right > Screen.PrimaryScreen.WorkingArea.Right)
                x = Screen.PrimaryScreen.WorkingArea.Right - previewWindow.Size.Width;

            if (previewWindow.Bottom > Screen.PrimaryScreen.WorkingArea.Bottom)
                y = Screen.PrimaryScreen.WorkingArea.Bottom - previewWindow.Size.Height;

            previewWindow.Location = new Point(x, y);
            previewWindow.Activate();
        }

        private void Debug(string msg)
        {
#if DEBUG
            toolStripStatusLabel1.Text = msg;
            Console.WriteLine(msg);
#endif
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

        private void GlobalMouseHandler_MouseMoved()
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

        private void InitializeFileDialogs()
        {
            saveFileDialog1.InitFiltersAsImageSaveFileDialog();
            openFileDialog1.InitFiltersAsImageOpenFileDialog();
        }

        private void InitializeGlobalMouseHandler()
        {
            GlobalMouseHandler globalMouseHandler = new GlobalMouseHandler();
            globalMouseHandler.TheMouseMoved += new MouseMovedEvent(GlobalMouseHandler_MouseMoved);
            Application.AddMessageFilter(globalMouseHandler);
        }

        private void InitializePreviewWindow()
        {
            previewWindow.PictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            previewWindow.TopLevel = true;
            previewWindow.TopMost = true;
            previewWindow.AutoSize = true;
        }
        private void LoadAndResizeSourceImage()
        {
            Controller.LoadSourceImage(openFileDialog1.FileName);
            if (Model.SourceImage == null)
                return;
            if (IsARLocked)
                Controller.ScaleSourceImageByWidth(ImageScaledWidth);
            else
                Controller.ScaleSourceImage(ImageScaledWidth, ImageScaledHeight);
            UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadAndResizeSourceImage();
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            PrintWatermark(panel1.CreateGraphics());
        }

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

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            isPictureBoxLMouseDown = false;
            isPictureBoxRMouseDown = false;
        }

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
                MoveTemplateClippingArea(dX);
            }
            else if (isPictureBoxLMouseDown)
            {
                ResizeTemplateClippingArea(dX, dY);
            }
            else
            {
                RotateTemplateClippingArea(dX);
            }

            isUpdating = true;
            UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder();

            if (previewWindow.Visible)
                Controller.ExtractTemplate();

            previousUpdateLocation = e.Location;
            GC.Collect();
            isUpdating = false;

            void MoveTemplateClippingArea(int _dX)
            {
                if (previousPatternSize + _dX > MAX_PATTERN_SIZE)
                    PatternSize = MAX_PATTERN_SIZE;
                else if (previousPatternSize + _dX < MIN_PATTERN_SIZE)
                    PatternSize = MIN_PATTERN_SIZE;
                else
                    PatternSize = previousPatternSize + _dX;
            }

            void ResizeTemplateClippingArea(int _dX, int _dY)
            {
                PatternXOffset = previousClipPathOffsetX + _dX;
                PatternYOffset = previousClipPathOffsetY + _dY;
            }

            void RotateTemplateClippingArea(int _dX)
            {
                var rotation = previousClipPathRotation - _dX / 10f;
                if (rotation > 360)
                    PatternRotation = rotation - 360;
                else if (rotation < 0)
                    PatternRotation = rotation + 360;
                else
                    PatternRotation = rotation;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                isPictureBoxLMouseDown = false;
            if (e.Button == MouseButtons.Right)
                isPictureBoxRMouseDown = false;
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
                    g.DrawString(_d_softwareInfo, font, Brushes.Black, rect, sf);
                }
            }
        }

        private void RandomizeTemplateExtractionParameters()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            PatternSize = random.Next(MIN_PATTERN_SIZE, Math.Min(ImageScaledWidth, imageScaledHeight));
            PatternXOffset = random.Next(0, ImageScaledWidth - PatternSize);
            PatternYOffset = random.Next(0, ImageScaledHeight - PatternSize);
            PatternRotation = random.Next(0, 300) / 10f;
        }

        private void Render()
        {
            Stopwatch stopwatch = new Stopwatch();

            UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder();

            Controller.ExtractTemplate();

            SetStatus("Rendering...");

            toolStripContainer1.TopToolStripPanel.Enabled = false;
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            stopwatch.Start();
            Controller.RenderKaleidoscopeImage(1920, 1080);
            stopwatch.Stop();

            SetStatus("Rendered in " + stopwatch.ElapsedMilliseconds + " ms.");
            toolStripStatusLabelRenderingWidth.Text = renderWindow.PictureBox.Width + "";
            toolStripStatusLabelRenderingHeight.Text = renderWindow.PictureBox.Height + "";

            toolStripContainer1.TopToolStripPanel.Enabled = true;
            Cursor = Cursors.Default;
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

        private void toolStripButtonAutoPatch_Click(object sender, EventArgs e)
        {
            toolStripButtonPatch.Enabled = !toolStripButtonAutoPatch.Checked;
        }

        private void toolStripButtonPatch_Click(object sender, EventArgs e)
        {
            var tmp = (Bitmap)renderWindow.PictureBox.Image;
            if (tmp == null)
                return;

            toolStripContainer1.TopToolStripPanel.Enabled = false;
            SetStatus("Filling gaps...");
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            GraphicsExtensions.FillGaps(tmp);
            stopwatch.Stop();
            SetStatus("Filled gaps in " + stopwatch.ElapsedMilliseconds + " ms.");

            renderWindow.PictureBox.Image = tmp;
            Cursor = Cursors.Default;
            toolStripContainer1.TopToolStripPanel.Enabled = true;
            Application.DoEvents();

            Opacity = 0.25;
            if (!renderWindow.Visible)
                renderWindow.Show();
            previewWindow.Opacity = 0.25;
        }

        private void toolStripButtonRandomize_Click(object sender, EventArgs e)
        {
            RandomizeTemplateExtractionParameters();
            Render();
        }

        private void toolStripButtonRender_Click(object sender, EventArgs e)
        {
            Render();
        }

        private void toolStripButtonSaveRenderedImage_Click(object sender, EventArgs e)
        {
            if (renderWindow.PictureBox.Image == null)
                return;

            saveFileDialog1.InitialDirectory =
                 System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
            saveFileDialog1.FileName =
                "Kaleidoscope_" +
                System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Controller.SaveRenderedImage(saveFileDialog1.FileName);
            }
        }

        private void toolStripButtonShowPreviewWindow_Click(object sender, EventArgs e)
        {
            previewWindow.Show();
            Controller.ExtractTemplate();
            ActivatePreviewWindowAtTopRightSide();
        }

        private void toolStripLabelSourceImage_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            if (toolStripStatusLabel1.IsLink)
            {
                Process.Start("EXPLORER.EXE", "/select, \"" + toolStripStatusLabel1.Tag.ToString() + "\"");
                toolStripStatusLabel1.LinkVisited = true;
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
                UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder();
        }

        private void toolStripTextBox2_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var previousValue = patternXOffset;
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                int.MinValue, int.MaxValue,
                "X Offset", ref patternXOffset, toolStrip4))
                e.Cancel = true;
            else if (previousValue != patternXOffset)
                UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder();
        }

        private void toolStripTextBox3_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var previousValue = imageScaledWidth;
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                 MIN_IMG_SIZE, MAX_IMG_SIZE,
                 "Width", ref imageScaledWidth, toolStrip2))
                e.Cancel = true;
            else if (IsARLocked && Model.SourceImage != null)
            {
                if (previousValue != imageScaledWidth)
                {
                    ImageScaledHeight = (int)(Math.Round((float)ImageScaledWidth / Model.SourceImage.Width * Model.SourceImage.Height, 0));
                    UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder();
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
            else if (IsARLocked && Model.SourceImage != null)
            {
                if (previousValue != imageScaledHeight)
                {
                    ImageScaledWidth = (int)(Math.Round((float)ImageScaledHeight / Model.SourceImage.Height * Model.SourceImage.Width, 0));
                    UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder();
                }
            }
        }

        private void toolStripTextBox5_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var previousValue = patternYOffset;
            if (!ValidateInt(((ToolStripTextBox)sender).Text,
                int.MinValue, int.MaxValue,
                "Y Offset", ref patternYOffset, toolStrip4))
                e.Cancel = true;
            else if (previousValue != patternYOffset)
                UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder();
        }

        private void toolStripTextBox6_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // TODO: validate angle
        }

        private void UpdateTemplateExtractionParametersToModelAndRefreshTemplateFinder()
        {
            Controller.UpdateTemplateExtractionParametersFromViewToModel();
            Controller.UpdateClippingPathOnTemplateFinder();
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
        public class GlobalMouseHandler : IMessageFilter
        {
            private const int WM_MOUSEMOVE = 0x0200;

            public event MouseMovedEvent TheMouseMoved;

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEMOVE)
                {
                    TheMouseMoved?.Invoke();
                }
                // Always allow message to continue to the next filter control
                return false;
            }
        }

        private void toolStripDropDownButton1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text.Contains("30-60-90"))
                Controller.SetMirorrSystem(typeof(MirrorSystem306090));
            else if (e.ClickedItem.Text.Contains("60-60-60"))
                Controller.SetMirorrSystem(typeof(MirrorSystem606060));
            else
                return;

            toolStripDropDownButton1.Text = e.ClickedItem.Text;
        }
    }
}