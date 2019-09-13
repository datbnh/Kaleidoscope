using System.Drawing;
using System.Windows.Forms;

namespace Kvh.Kaleidoscope
{
    public partial class RenderWindow : Form
    {
        public RenderWindow()
        {
            InitializeComponent();
        }

        public Image I { get => pictureBox1.Image; set => pictureBox1.Image = value; }
        public PictureBox PictureBox { get => pictureBox1; }

        private void RenderWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}