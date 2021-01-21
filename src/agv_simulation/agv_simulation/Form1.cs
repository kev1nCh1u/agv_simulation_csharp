using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace agv_simulation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            drawOnPic();
        }

        void drawOnPic()
        {
            //Console.WriteLine($">> {DateTime.Now.ToString()}");
            pictureBox1.Invalidate();

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;

            Graphics g = Graphics.FromImage(bmp);
            //Graphics g = pictureBox1.CreateGraphics();

            Pen color = new Pen(Color.Red, 4);
            Brush bb = new SolidBrush(Color.Red);

            Point p1 = new Point(pictureBox1.Width / 2 - 10 , 0);
            Point p2 = new Point(pictureBox1.Width / 2 - 10 , pictureBox1.Height);

            g.DrawLine(color, p1, p2);



        }

    }
}
