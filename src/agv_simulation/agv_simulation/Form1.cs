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

        void genLine()
        {
            int num = 300;
            PointF[] points = new PointF[num];
            for (int i = 0; i < num; i++)
            {

                points[i].X = (float)pictureBox1.Width/2;
                points[i].Y = (float)i + 100;

            }
            drawOnPic(points);
        }

        void drawOnPic(PointF[] points)
        {
            //Console.WriteLine($">> {DateTime.Now.ToString()}");
            pictureBox1.Invalidate();

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;

            Graphics g = Graphics.FromImage(bmp);
            //Graphics g = pictureBox1.CreateGraphics();

            Pen penColor = new Pen(Color.Red, 4);
            Brush brushColor = new SolidBrush(Color.Black);

            g.DrawCurve(penColor, points);

        }

        public Form1()
        {
            InitializeComponent();
            genLine();
        }




    }
}
