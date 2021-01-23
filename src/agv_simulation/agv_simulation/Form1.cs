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
        double carHead;
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
            Pen penColorBlack = new Pen(Color.Black, 2);
            Brush brushColorBlack = new SolidBrush(Color.Black);
            Brush brushColorOrange = new SolidBrush(Color.Orange);

            g.DrawCurve(penColorRed, navPath);
            g.DrawLine(penColorBlack, carPoint.X, carPoint.Y, carPoint.X + 10 * (float)Math.Cos(carHead), carPoint.Y - 10 * (float)Math.Sin(carHead));
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
            double errX, errY;
            double errDis, errDisLast = 0, errDisSum = 0;
            double errSita, errSitaLast = 0, errSitaSum  = 0;
            double carV, carW;
            double kp = 1, ki = 0, kd = 0;
            errX = closePoint.X - carPoint.X;
            errY = -(closePoint.Y - carPoint.Y);
            errDis = pythagorean((float)errX, (float)errY);
            errSita = Math.Atan2(errY, errX);
            // Console.WriteLine(x + "  " + y + "  " + pureSita);

            // motorLvalue = motorSpeed - (sensorError * motorSpeed * kp  + sensorErrorSum * ki + (sensorError - sensorErrorLast) * kd);

            carV = kp * errDis + ki * errDisSum + kd * (errDis - errDisLast);

        }

        public Form1()
        {
            InitializeComponent();

            navPath = genLine(new PointF(pictureBox1.Height / 2, 500), new PointF(pictureBox1.Height / 2, 50));
            carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
            carHead = 1.57;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Console.WriteLine("timer1 tick");
            closePoint = findClosePoint(carPoint, navPath);
            drawOnPic();
            purePursuit();
        }
    }
}
