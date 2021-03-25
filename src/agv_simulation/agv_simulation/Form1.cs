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
        double carHead, wheelHead, accuracy;
        int closeNum, frontNum, frontDis, basicSpeed, carLength;
        PointF[] waypoints, circlePoints;
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
                case 1:
                    // S path
                    // Console.WriteLine("S");
                    waypoints = GenWave(pictureBox1.Width, pictureBox1.Height, 100);
                    carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
                    break;
                case 2:
                    // square path
                    // Console.WriteLine("square");
                    waypoints = GenSquare(pictureBox1.Width, pictureBox1.Height);
                    carPoint = new PointF((float)pictureBox1.Width / 2 - 200, (float)450);
                    break;
                case 3:
                    // GenCircle path
                    // Console.WriteLine("GenCircle");
                    waypoints = GenCircle();
                    carPoint = new PointF((float)pictureBox1.Width / 2 - 250, (float)350);
                    break;
                default:
                    // line path
                    // Console.WriteLine("line");
                    waypoints = GenLine(pictureBox1.Width, pictureBox1.Height);
                    carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
                    break;
            }

            carHead = 1.57;
            wheelHead = carHead;
            frontNum = 0;
            carHis = new List<PointF>();
        }

        PointF[] GenLine(int width, int height)
        {
            int side = 50;
            int num = height - side * 2;
            PointF[] points = new PointF[num];
            for (int i = height - side, j = 0; i > side; i--, j++)
            {
                points[j].X = (float)width / 2;
                points[j].Y = (float)i;
            }
            return points;
        }

        PointF[] GenWave(int width, int height, int wave = 40)
        {
            int side = 50;
            int num = height - side * 2;
            PointF[] points = new PointF[num];
            for (int i = 0; i < num; i++)
            {
                points[i].X = (float)pictureBox1.Width / 2 + (float)Math.Sin(i / (float)pictureBox1.Height * 2 * (Math.PI)) * wave;
                points[i].Y = (float)height - side - i;
            }
            return points;
        }

        PointF[] GenSquare(int width, int height)
        {
            int side = 50, distance = 80;
            int num = height - side * 2;
            List<PointF> points = new List<PointF>();
            PointF point = new PointF((float)side, (float)height - side);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    point.Y = point.Y - 1;
                    points.Add(point);
                }
                num -= distance;

                for (int j = 0; j < num; j++)
                {
                    point.X = point.X + 1;
                    points.Add(point);
                }

                for (int j = 0; j < num; j++)
                {
                    point.Y = point.Y + 1;
                    points.Add(point);
                }
                num -= distance;

                for (int j = 0; j < num; j++)
                {
                    point.X = point.X - 1;
                    points.Add(point);
                }
            }

            return points.ToArray();
        }

        PointF[] GenCircle()
        {
            PointF point = new PointF();
            PointF centerPoint = new PointF(pictureBox1.Width / 2, pictureBox1.Height / 2);
            PointF startPoint = new PointF((float)pictureBox1.Width / 2 - 200, (float)450);
            List<PointF> points = new List<PointF>();
            float radius = (float)Pythagorean(centerPoint.X - startPoint.X, centerPoint.Y - startPoint.Y);
            
            while(radius > 0)
            {
                float perNum = (float)0.05;
                for (float i = (float)-3.14, j = 0; i <= (float)3.14; i=i+perNum, j++)
                {
                    point.X = (int)(centerPoint.X + radius * (float)Math.Cos((float)i));
                    point.Y = (int)(centerPoint.Y + radius * (float)Math.Sin((float)i));
                    points.Add(point);
                    radius = radius - perNum*14;
                    if(radius < 0)
                        break;
                }
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
            Pen penColorGray = new Pen(Color.Gray, 1);
            Pen penColorOrg = new Pen(Color.Orange, 1);
            Pen penColorBlu = new Pen(Color.Blue, 1);
            Pen penColorBlack = new Pen(Color.Black, 4);
            Pen penColorThinBlack = new Pen(Color.Black, 1);
            Pen penColorBigBlack = new Pen(Color.Black, 10);

            Brush brushColorBlue = new SolidBrush(Color.Blue);
            Brush brushColorBlack = new SolidBrush(Color.Black);
            Brush brushColorOrange = new SolidBrush(Color.Orange);

            // ============================== 路徑 =============================
            g.DrawCurve(penColorRed, waypoints);

            // ============================== 歷史軌跡 =============================
            if (carHis.Count > 1)
                g.DrawCurve(penColorBlu, carHis.ToArray());
            
            // ============================== 最近點 =============================
            g.FillEllipse(brushColorBlue, waypoints[closeNum].X - 5,waypoints[closeNum].Y - 5, 10, 10);
            
            
            if(comboBox2.SelectedIndex == 1 || comboBox2.SelectedIndex == 2)
            {
                // ============================ 轉向輪位置 =====================================
                PointF wheelPoint = new PointF();
                if(comboBox2.SelectedIndex == 1)
                {
                    wheelPoint = new PointF(carPoint.X + carLength * (float)Math.Cos(carHead), carPoint.Y - carLength * (float)Math.Sin(carHead));
                }
                else if(comboBox2.SelectedIndex == 2)
                {
                    wheelPoint = new PointF(carPoint.X - carLength * (float)Math.Cos(carHead), carPoint.Y + carLength * (float)Math.Sin(carHead));
                }

                // ============================ 輔助輪位置 =====================================
                PointF carPointRight = new PointF(carPoint.X + 30 * (float)Math.Cos(carHead + 1.57), carPoint.Y - 30 * (float)Math.Sin(carHead + 1.57));
                PointF carPointLift = new PointF(carPoint.X + 30 * (float)Math.Cos(carHead - 1.57), carPoint.Y - 30 * (float)Math.Sin(carHead - 1.57));
                
                // ============================ 轉向輪 =====================================
                g.DrawLine(penColorThinBlack, wheelPoint.X, wheelPoint.Y, carPoint.X, carPoint.Y);
                g.DrawLine(penColorBigBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X + 10 * (float)Math.Cos(wheelHead), wheelPoint.Y - 10 * (float)Math.Sin(wheelHead));
                g.DrawLine(penColorBigBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X - 10 * (float)Math.Cos(wheelHead), wheelPoint.Y + 10 * (float)Math.Sin(wheelHead));

                // ============================ 輔助輪右 =====================================
                g.DrawLine(penColorThinBlack, carPointRight.X, carPointRight.Y, carPoint.X, carPoint.Y);
                g.DrawLine(penColorBigBlack, carPointRight.X, carPointRight.Y, carPointRight.X + 10 * (float)Math.Cos(carHead), carPointRight.Y - 10 * (float)Math.Sin(carHead));
                g.DrawLine(penColorBigBlack, carPointRight.X, carPointRight.Y, carPointRight.X - 10 * (float)Math.Cos(carHead), carPointRight.Y + 10 * (float)Math.Sin(carHead));

                // ============================ 輔助輪左 =====================================
                g.DrawLine(penColorThinBlack, carPointLift.X, carPointLift.Y, carPoint.X, carPoint.Y);
                g.DrawLine(penColorBigBlack, carPointLift.X, carPointLift.Y, carPointLift.X + 10 * (float)Math.Cos(carHead), carPointLift.Y - 10 * (float)Math.Sin(carHead));
                g.DrawLine(penColorBigBlack, carPointLift.X, carPointLift.Y, carPointLift.X - 10 * (float)Math.Cos(carHead), carPointLift.Y + 10 * (float)Math.Sin(carHead));
            }
            
            // ============================== 前視距離圓圈 =============================
            if(comboBox3.SelectedIndex == 1)
                g.DrawCurve(penColorGray, circlePoints);

            // ============================== 前視距離 =============================
            g.DrawLine(penColorOrg, carPoint.X, carPoint.Y, waypoints[frontNum].X, waypoints[frontNum].Y);
            g.FillEllipse(brushColorOrange, waypoints[frontNum].X - 5, waypoints[frontNum].Y - 5, 10, 10);

            // ============================== 車中心 =============================
            g.DrawLine(penColorBlack, carPoint.X, carPoint.Y, carPoint.X + 15 * (float)Math.Cos(carHead), carPoint.Y - 15 * (float)Math.Sin(carHead));
            g.FillEllipse(brushColorBlack, carPoint.X - 5, carPoint.Y - 5, 10, 10);
        }

        double Pythagorean(float x, float y)
        {
            double ans = Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 0.5);
            return ans;
        }

        int FindClosestPoint(PointF basic, PointF[] compare)
        {
            double errCalc;
            double err = 999999;
            PointF point = new PointF(0, 0);
            int placeNum = 0;
            for (int i = 0; i < compare.Length; i++)
            {
                // errCalc = Math.Pow(Math.Pow(basic.X - compare[i].X, 2) + Math.Pow(basic.Y - compare[i].Y, 2), 0.5);
                errCalc = Pythagorean(basic.X - compare[i].X, basic.Y - compare[i].Y);
                // Console.WriteLine("test " + i); //debug
                if (errCalc < err)
                {
                    err = errCalc;
                    point = compare[i];
                    placeNum = i;
                }
            }
            // Console.WriteLine(i); //debug
            return placeNum;
        }

        int FindFrontPoint(PointF basic, PointF[] compare, int closeNum, int frontDis, int frontNum = 0)
        {
            double errCalc;
            double err = 999999;
            PointF point = compare[compare.Length - 1];
            int placeNum = compare.Length - 1;
            // Console.WriteLine("i:" + i); //debug
            for (int i = closeNum; i < compare.Length; i++)
            {
                errCalc = Pythagorean(basic.X - compare[i].X, basic.Y - compare[i].Y);
                if ((errCalc < err) && (errCalc > frontDis) && (i >= frontNum))
                {
                    err = errCalc;
                    point = compare[i];
                    placeNum = i;
                    break;
                }
            }
            return placeNum;
        }

        PointF[] GenCarCircle(PointF basic, PointF[] compare, int frontDis)
        {
            PointF point = new PointF(0, 0);
            PointF[] circlePoints = new PointF[314*2 + 1];

            for (int i = -314, j = 0; i <= 314; i++, j++)
            {
                point.X = (int)(basic.X + frontDis * (float)Math.Cos((float)i/100));
                point.Y = (int)(basic.Y + frontDis * (float)Math.Sin((float)i/100));
                circlePoints[j] = point;
            }

            return circlePoints;
        }

        int FindRadiusPoint(PointF[] basic, PointF[] compare, int frontNum = 0)
        {
            int placeNum = frontNum;

            for(int i = 0; i <= 314*2; i++)
            {
                for(int j = 0; j < compare.Length; j++)
                {
                    int precision = 2;
                    // Console.WriteLine(basic[i].X + " " + basic[i].Y + " " + compare[j].X + " " + compare[j].Y);
                    if((Math.Abs(basic[i].X - compare[j].X) < precision) && (Math.Abs(basic[i].Y - compare[j].Y) < precision) && (j > frontNum))
                    {
                        if(j > placeNum)
                        {
                            placeNum = j;
                            // Console.WriteLine(placeNum + " " + j);
                        }
                    }
                }
            }
            // if((placeNum  == frontNum) && placeNum != 0)
            //     placeNum = compare.Length - 1;
                
            return placeNum;
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

        void Kinematics(int basicSpeed)
        {
            double errX = 0, errY = 0;
            double carV = 0, carW = 0;

            errX = waypoints[frontNum].X - carPoint.X;
            errY = -(waypoints[frontNum].Y - carPoint.Y);

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
            carHis.Add(carPoint);

            if (carV > 30)
                carV = 30;

            carPoint.X += (float)(carV * Math.Cos(carHead));
            carPoint.Y -= (float)(carV * Math.Sin(carHead));
            carHead -= carW;
            wheelHead = carHead + Math.Atan(carW * -1 * carLength / carV);
        }
        void MoveForkliftReverse(double carV, double carW)
        {
            carHis.Add(carPoint);

            if (carV > 30)
                carV = 30;

            carPoint.X += (float)(carV * Math.Cos(carHead));
            carPoint.Y -= (float)(carV * Math.Sin(carHead));
            carHead -= carW;
            wheelHead = carHead - Math.Atan(carW * -1 * carLength / carV);
        }

        public Form1()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

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
            carLength = trackBar10.Value;

            label10.Text = Convert.ToString(frontDis);
            label11.Text = Convert.ToString(basicSpeed);
            label12.Text = Convert.ToString(accuracy);
            label13.Text = Convert.ToString(errDis.kp);
            label14.Text = Convert.ToString(errDis.ki);
            label15.Text = Convert.ToString(errDis.kd);
            label16.Text = Convert.ToString(errSita.kp);
            label17.Text = Convert.ToString(errSita.ki);
            label18.Text = Convert.ToString(errSita.kd);
            label23.Text = Convert.ToString(carLength);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GetControlInfo();

            // Console.WriteLine("timer1 tick");
            if(comboBox3.SelectedIndex == 0)
            {
                closeNum = FindClosestPoint(carPoint, waypoints);
                frontNum = FindFrontPoint(waypoints[closeNum], waypoints, closeNum, frontDis, frontNum);
            }
            else if(comboBox3.SelectedIndex == 1)
            {
                closeNum = FindClosestPoint(carPoint, waypoints);
                circlePoints = GenCarCircle(carPoint, waypoints, frontDis);
                frontNum = FindRadiusPoint(circlePoints, waypoints, frontNum);
            }

            Kinematics(basicSpeed);
            closeNum = FindClosestPoint(carPoint, waypoints);

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
