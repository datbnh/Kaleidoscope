using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaDoscope_Beta0
{
    public partial class RenderWindow : Form
    {
        public RenderWindow()
        {
            InitializeComponent();
        }

        public Image I { get => pictureBox1.Image; set => pictureBox1.Image = value; }
        public PictureBox PictureBox { get => pictureBox1; }
    }
}
