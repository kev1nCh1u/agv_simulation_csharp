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

        PointF carPoint;
        double carHead;
        int closePoint, frontPoint, frontDis, basicSpeed;
        double accuracy;
        PointF[] navPath;
        List<PointF> carHis;

        public struct ErrType
        {
            public double kp, ki, kd;
            public double err, errlast, errsum;
        }

        double errX = 0, errY = 0;
        // double kp = 0.1, ki = 0, kd = 0;
        // double errDis = 0, errDisLast = 0, errDisSum = 0;
        // double errSita = 0, errSitaLast = 0, errSitaSum = 0;

        ErrType errDis, errSita;
        double carV = 0, carW = 0;

        void ReStart()
        {
            navPath = GenS(new PointF(pictureBox1.Height / 2, 500), new PointF(pictureBox1.Height / 2, 50));
            carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
            carHead = 1.57;
            frontPoint = 0;
            carHis = new List<PointF>();
        }

        PointF[] GenLine(PointF startPoint, PointF endPoint)
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

        PointF[] GenS(PointF startPoint, PointF endPoint)
        {
            int num = 450;
            PointF[] points = new PointF[num];
            for (int i = 0; i < num; i++)
            {

                points[i].X = (float)pictureBox1.Width / 2 + (float)Math.Sin(i / (float)pictureBox1.Height * 2 * (Math.PI)) * 40;
                points[i].Y = (float)500 - i;

            }
            return points;
        }

        void DrawOnPic()
        {
            //Console.WriteLine($">> {DateTime.Now.ToString()}");
            pictureBox1.Invalidate();

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;

            Graphics g = Graphics.FromImage(bmp);
            //Graphics g = pictureBox1.CreateGraphics();

            Pen penColorRed = new Pen(Color.Red, 4);
            Pen penColorOrg = new Pen(Color.Orange, 1);
            Pen penColorBlu = new Pen(Color.Blue, 1);
            Pen penColorBlack = new Pen(Color.Black, 2);
            Brush brushColorBlack = new SolidBrush(Color.Black);
            Brush brushColorOrange = new SolidBrush(Color.Orange);

            g.DrawCurve(penColorRed, navPath);
            if(carHis.Count > 1)
                g.DrawCurve(penColorBlu, carHis.ToArray());
            g.DrawLine(penColorBlack, carPoint.X, carPoint.Y, carPoint.X + 10 * (float)Math.Cos(carHead), carPoint.Y - 10 * (float)Math.Sin(carHead));
            g.DrawLine(penColorOrg, carPoint.X, carPoint.Y, navPath[frontPoint].X , navPath[frontPoint].Y);
            g.FillEllipse(brushColorBlack, carPoint.X - 5, carPoint.Y - 5, 10, 10);
            g.FillEllipse(brushColorOrange, navPath[frontPoint].X - 5, navPath[frontPoint].Y - 5, 10, 10);

        }

        double Pythagorean(float x, float y)
        {
            double ans = Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 0.5);
            return ans;
        }

        int FindFrontPoint(PointF basic, PointF[] compare, int closePoint, int frontDis)
        {
            double errCalc;
            double err = 999999;
            PointF point = compare[compare.Length - 1];
            int place = compare.Length - 1;
            // Console.WriteLine("i:" + i); //debug
            for (int i = closePoint; i < compare.Length; i++)
            {
                errCalc = Pythagorean(basic.X - compare[i].X, basic.Y - compare[i].Y);
                if (errCalc < err && errCalc > frontDis && i >= frontPoint)
                {
                    err = errCalc;
                    point = compare[i];
                    place = i;
                }
            }
            return place;
        }

        int FindClosestPoint(PointF basic, PointF[] compare)
        {
            double errCalc;
            double err = 999999;
            PointF point = new PointF(0, 0);
            int place = 0;
            for (int i = 0; i < compare.Length; i++)
            {
                // errCalc = Math.Pow(Math.Pow(basic.X - compare[i].X, 2) + Math.Pow(basic.Y - compare[i].Y, 2), 0.5);
                errCalc = Pythagorean(basic.X - compare[i].X, basic.Y - compare[i].Y);
                // Console.WriteLine("test " + i); //debug
                if (errCalc < err)
                {
                    err = errCalc;
                    point = compare[i];
                    place = i;
                }
            }
            // Console.WriteLine(i); //debug
            return place;
        }

        double PidFuc(double kp, double ki, double kd, double err, double errSum, double errLast)
        {
            double ans = kp * err + ki * errSum + kd * (err - errLast);
            return ans;
        }

        void PurePursuit(int basicSpeed)
        {
            errX = navPath[frontPoint].X - carPoint.X;
            errY = -(navPath[frontPoint].Y - carPoint.Y);

            errDis.err = Pythagorean((float)errX, (float)errY);
            errSita.err = carHead - Math.Atan2(errY, errX);
            if (errSita.err > Math.PI)
                errSita.err = errSita.err - 2 * Math.PI;
            else if (errSita.err < -Math.PI)
                errSita.err = errSita.err + 2 * Math.PI;
            // Console.WriteLine(errX + "  " + errY + "  " + errDis.err + "  " + errSita.err); //debug

            carV = PidFuc(errDis.kp, errDis.ki, errDis.kd, errDis.err, errDis.errsum, errDis.errlast) + basicSpeed;
            carW = PidFuc(errSita.kp, errSita.ki, errSita.kd, errSita.err, errSita.errsum, errSita.errlast);
            errDis.errlast = errDis.err;
            errDis.errsum += errDis.err;
            errSita.errlast = errSita.err;
            errSita.errsum += errSita.err;
            // Console.WriteLine("carhead:" + carHead + "  atan:" + Math.Atan2(errY, errX) + "  errSita:" + errSita.err + "  carV:" + carV + "  carW:" + carW); //debug

            MoveCar(carV, carW);
        }

        void MoveCar(double carV, double carW)
        {
            carHis.Add(carPoint);

            if (carV > 30)
                carV = 30;
            // if (carW > Math.PI)
            //     carW = Math.PI;
            // if (carW < -Math.PI)
            //     carW = -Math.PI;

            carPoint.X += (float)(carV * Math.Cos(carHead));
            carPoint.Y -= (float)(carV * Math.Sin(carHead));
            carHead -= carW;
        }

        public Form1()
        {
            InitializeComponent();

            ReStart();
        }

        void GetControlInfo()
        {
            frontDis = trackBar1.Value;
            basicSpeed = trackBar2.Value;
            accuracy = trackBar3.Value / (float)10;
            errDis.kp = trackBar4.Value / (float)100;
            errDis.ki = trackBar5.Value / (float)100;
            errDis.kd = trackBar6.Value / (float)100;
            errSita.kp = trackBar7.Value / (float)100;
            errSita.ki = trackBar8.Value / (float)100;
            errSita.kd = trackBar9.Value / (float)100;

            label10.Text = Convert.ToString(frontDis);
            label11.Text = Convert.ToString(basicSpeed);
            label12.Text = Convert.ToString(accuracy);
            label13.Text = Convert.ToString(errDis.kp);
            label14.Text = Convert.ToString(errDis.ki);
            label15.Text = Convert.ToString(errDis.kd);
            label16.Text = Convert.ToString(errSita.kp);
            label17.Text = Convert.ToString(errSita.ki);
            label18.Text = Convert.ToString(errSita.kd);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GetControlInfo();

            // Console.WriteLine("timer1 tick");
            closePoint = FindClosestPoint(carPoint, navPath);
            frontPoint = FindFrontPoint(navPath[closePoint], navPath, closePoint, frontDis);
            PurePursuit(basicSpeed);

            DrawOnPic();

            if (errDis.err < accuracy && errSita.err < accuracy)
            {
                ReStart();
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            ReStart();
        }
    }
}
