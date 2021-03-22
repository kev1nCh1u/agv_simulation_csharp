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
        double wheelHead;
        int closePoint, frontPoint, frontDis, basicSpeed;
        double accuracy;
        PointF[] navPath;
        List<PointF> carHis;

        public struct ErrType
        {
            public double kp, ki, kd;
            public double err, errlast, errsum;
        }

        ErrType errDis, errSita;
        

        void ReStart()
        {
            int pathItem = comboBox1.SelectedIndex;
            // Console.WriteLine(pathItem);
            switch (pathItem)
            {
                case 0:
                    // line path
                    // Console.WriteLine("line");
                    navPath = GenLine(new PointF(pictureBox1.Height / 2, 500), new PointF(pictureBox1.Height / 2, 50));
                    carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
                    break;
                case 1:
                    // S path
                    // Console.WriteLine("S");
                    navPath = GenS(new PointF(pictureBox1.Height / 2, 500), new PointF(pictureBox1.Height / 2, 50));
                    carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
                    break;
                case 2:
                    // square path
                    // Console.WriteLine("square");
                    navPath = GenSquare(new PointF(pictureBox1.Height / 2, 500), new PointF(pictureBox1.Height / 2, 50));
                    carPoint = new PointF((float)pictureBox1.Width / 2 - 200, (float)450);
                    break;
                default:
                    // line path
                    // Console.WriteLine("line");
                    navPath = GenLine(new PointF(pictureBox1.Height / 2, 500), new PointF(pictureBox1.Height / 2, 50));
                    carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
                    break;
            }

            carHead = 1.57;
            wheelHead = carHead;
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

        PointF[] GenS(PointF startPoint, PointF endPoint, int wave = 40)
        {
            int num = 450;
            PointF[] points = new PointF[num];
            for (int i = 0; i < num; i++)
            {

                points[i].X = (float)pictureBox1.Width / 2 + (float)Math.Sin(i / (float)pictureBox1.Height * 2 * (Math.PI)) * wave;
                points[i].Y = (float)500 - i;

            }
            return points;
        }

        PointF[] GenSquare(PointF startPoint, PointF endPoint)
        {
            int num = 400;
            List<PointF> points = new List<PointF>();
            PointF point = new PointF();
            PointF pointLast = new PointF((float)pictureBox1.Width / 2 - 200, (float)450);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    point.X = pointLast.X;
                    point.Y = pointLast.Y - j;
                    points.Add(point);
                }
                pointLast = point;
                num -= 80;

                for (int j = 0; j < num; j++)
                {

                    point.X = pointLast.X + j;
                    point.Y = pointLast.Y;
                    points.Add(point);
                }
                pointLast = point;

                for (int j = 0; j < num; j++)
                {

                    point.X = pointLast.X;
                    point.Y = pointLast.Y + j;
                    points.Add(point);
                }
                pointLast = point;
                num -= 80;

                for (int j = 0; j < num; j++)
                {
                    point.X = pointLast.X - j;
                    point.Y = pointLast.Y;
                    points.Add(point);
                }
                pointLast = point;
                
            }

            return points.ToArray();
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
            Pen penColorBlack = new Pen(Color.Black, 4);
            Pen penColorThinBlack = new Pen(Color.Black, 1);
            Pen penColorBigBlack = new Pen(Color.Black, 10);

            Brush brushColorBlue = new SolidBrush(Color.Blue);
            Brush brushColorBlack = new SolidBrush(Color.Black);
            Brush brushColorOrange = new SolidBrush(Color.Orange);

            g.DrawCurve(penColorRed, navPath);
            if (carHis.Count > 1)
                g.DrawCurve(penColorBlu, carHis.ToArray());

            g.FillEllipse(brushColorBlue, navPath[closePoint].X - 5,navPath[closePoint].Y - 5, 10, 10);

            g.DrawLine(penColorOrg, carPoint.X, carPoint.Y, navPath[frontPoint].X, navPath[frontPoint].Y);
            g.FillEllipse(brushColorOrange, navPath[frontPoint].X - 5, navPath[frontPoint].Y - 5, 10, 10);

            g.DrawLine(penColorBlack, carPoint.X, carPoint.Y, carPoint.X + 15 * (float)Math.Cos(carHead), carPoint.Y - 15 * (float)Math.Sin(carHead));
            g.FillEllipse(brushColorBlack, carPoint.X - 5, carPoint.Y - 5, 10, 10);
            

            if(comboBox2.SelectedIndex == 1)
            {
                PointF wheelPoint = new PointF(carPoint.X + 50 * (float)Math.Cos(carHead), carPoint.Y - 50 * (float)Math.Sin(carHead));

                g.DrawLine(penColorThinBlack, wheelPoint.X, wheelPoint.Y, carPoint.X, carPoint.Y);

                // g.DrawLine(penColorBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X + 15 * (float)Math.Cos(wheelHead), wheelPoint.Y - 15 * (float)Math.Sin(wheelHead));
                // g.FillEllipse(brushColorBlack, wheelPoint.X - 5, wheelPoint.Y - 5, 10, 10);

                g.DrawLine(penColorBigBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X + 10 * (float)Math.Cos(wheelHead), wheelPoint.Y - 10 * (float)Math.Sin(wheelHead));
                g.DrawLine(penColorBigBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X - 10 * (float)Math.Cos(wheelHead), wheelPoint.Y + 10 * (float)Math.Sin(wheelHead));
            }
            if(comboBox2.SelectedIndex == 2)
            {
                PointF wheelPoint = new PointF(carPoint.X - 50 * (float)Math.Cos(carHead), carPoint.Y + 50 * (float)Math.Sin(carHead));

                g.DrawLine(penColorThinBlack, wheelPoint.X, wheelPoint.Y, carPoint.X, carPoint.Y);

                // g.DrawLine(penColorBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X + 15 * (float)Math.Cos(wheelHead), wheelPoint.Y - 15 * (float)Math.Sin(wheelHead));
                // g.FillEllipse(brushColorBlack, wheelPoint.X - 5, wheelPoint.Y - 5, 10, 10);

                g.DrawLine(penColorBigBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X + 10 * (float)Math.Cos(wheelHead), wheelPoint.Y - 10 * (float)Math.Sin(wheelHead));
                g.DrawLine(penColorBigBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X - 10 * (float)Math.Cos(wheelHead), wheelPoint.Y + 10 * (float)Math.Sin(wheelHead));
            }

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
                    break;
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

        void PurePursuit()
        {

        }

        double PidFuc(double kp, double ki, double kd, double err, double errSum, double errLast)
        {
            double ans = kp * err + ki * errSum + kd * (err - errLast);
            return ans;
        }

        private void PathChange(object sender, EventArgs e)
        {
            ReStart();
        }

        void PointCarKinematics(int basicSpeed)
        {
            double errX = 0, errY = 0;
            double carV = 0, carW = 0;

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

            if(comboBox2.SelectedIndex == 0)
                MovePointCar(carV, carW);
            else if(comboBox2.SelectedIndex == 1)
                MoveForkliftFront(carV, carW);
            else if(comboBox2.SelectedIndex == 2)
                MoveForkliftReverse(carV, carW);

        }

        void MovePointCar(double carV, double carW)
        {
            carHis.Add(carPoint);

            if (carV > 30)
                carV = 30;

            carPoint.X += (float)(carV * Math.Cos(carHead));
            carPoint.Y -= (float)(carV * Math.Sin(carHead));
            carHead -= carW;
        }

        void MoveForkliftFront(double carV, double carW)
        {
            int Lb = 30;
            carHis.Add(carPoint);

            if (carV > 30)
                carV = 30;

            carPoint.X += (float)(carV * Math.Cos(carHead));
            carPoint.Y -= (float)(carV * Math.Sin(carHead));
            carHead -= carW;
            wheelHead = carHead + Math.Atan(carW * -1 * Lb / carV);
        }
        void MoveForkliftReverse(double carV, double carW)
        {
            int Lb = 30;
            carHis.Add(carPoint);

            if (carV > 30)
                carV = 30;

            carPoint.X += (float)(carV * Math.Cos(carHead));
            carPoint.Y -= (float)(carV * Math.Sin(carHead));
            carHead -= carW;
            wheelHead = carHead - Math.Atan(carW * -1 * Lb / carV);
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
            PointCarKinematics(basicSpeed);

            closePoint = FindClosestPoint(carPoint, navPath);
            frontPoint = FindFrontPoint(navPath[closePoint], navPath, closePoint, frontDis);

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
