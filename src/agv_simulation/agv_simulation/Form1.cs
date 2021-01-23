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

        PointF carPoint, closePoint;
        PointF[] navPath;


        PointF[] genLine(PointF startPoint, PointF endPoint)
        {
            int num = 450;
            PointF[] points = new PointF[num];
            for (int i = 0; i < num; i++)
            {

                points[i].X = (float)pictureBox1.Width / 2;
                points[i].Y = (float)500 - i;

            }
            return points;
        }

        void drawOnPic()
        {
            //Console.WriteLine($">> {DateTime.Now.ToString()}");
            pictureBox1.Invalidate();

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;

            Graphics g = Graphics.FromImage(bmp);
            //Graphics g = pictureBox1.CreateGraphics();

            Pen penColorRed = new Pen(Color.Red, 4);
            Brush brushColorBlack = new SolidBrush(Color.Black);
            Brush brushColorOrange = new SolidBrush(Color.Orange);

            g.DrawCurve(penColorRed, navPath);
            g.FillEllipse(brushColorBlack, carPoint.X - 5, carPoint.Y - 5, 10, 10);
            g.FillEllipse(brushColorOrange, closePoint.X - 5, closePoint.Y - 5, 10, 10);

        }

        double pythagorean(float x, float y)
        {
            double ans = Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 0.5);
            return ans;
        }

        PointF findClosePoint(PointF basic, PointF[] compare)
        {
            double errCalc;
            double err = 999999;
            PointF point = new PointF(0, 0);
            for (int i = compare.Length - 1; i >= 0; i--)
            {
                // errCalc = Math.Pow(Math.Pow(basic.X - compare[i].X, 2) + Math.Pow(basic.Y - compare[i].Y, 2), 0.5);
                errCalc = pythagorean(basic.X - compare[i].X, basic.Y - compare[i].Y);
                // Console.WriteLine("test " + i); //debug
                if (errCalc < err && errCalc > 150)
                {
                    err = errCalc;
                    point = compare[i];
                }
            }
            return point;
        }

        void purePursuit()
        {

        }

        public Form1()
        {
            InitializeComponent();

            navPath = genLine(new PointF(pictureBox1.Height / 2, 500), new PointF(pictureBox1.Height / 2, 50));
            carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Console.WriteLine("timer1 tick");
            closePoint = findClosePoint(carPoint, navPath);
            drawOnPic();

        }
    }
}
