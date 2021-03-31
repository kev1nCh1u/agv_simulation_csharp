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
        /********************************************************************************************************************************
        * 變數定義
        ********************************************************************************************************************************/
        public struct ErrType
        {
            public double kp, ki, kd;
            public double err, errlast, errsum;
        }

        PointF g_carPoint;
        double g_carHead, g_wheelHead, g_accuracy;
        int g_closeNum, g_frontNum, g_frontDis, g_basicSpeed, g_carLength;
        PointF[] g_waypoints, g_circlePoints;
        List<PointF> g_carHistory;
        ErrType g_errDis, g_errSita;
        
        /********************************************************************************************************************************
        * 重新啟動
        ********************************************************************************************************************************/
        void ReStart()
        {
            int pathItem = comboBox1.SelectedIndex;
            // Console.WriteLine(pathItem);
            switch (pathItem)
            {
                case 1:
                    // S path
                    // Console.WriteLine("S");
                    g_waypoints = GenWave(pictureBox1.Width, pictureBox1.Height, 100);
                    g_carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
                    break;
                case 2:
                    // square path
                    // Console.WriteLine("square");
                    g_waypoints = GenSquare(pictureBox1.Width, pictureBox1.Height);
                    g_carPoint = new PointF((float)50, (float)pictureBox1.Height - 50);
                    break;
                case 3:
                    // GenCircle path
                    // Console.WriteLine("GenCircle");
                    g_waypoints = GenCircle(pictureBox1.Width, pictureBox1.Height);
                    g_carPoint = new PointF((float)pictureBox1.Width / 2 - 250, (float)350);
                    break;
                default:
                    // line path
                    // Console.WriteLine("line");
                    g_waypoints = GenLine(pictureBox1.Width, pictureBox1.Height);
                    g_carPoint = new PointF(pictureBox1.Width / 2 - 100, pictureBox1.Height - 100);
                    break;
            }

            g_carHead = 1.57;
            g_wheelHead = g_carHead;
            g_frontNum = 0;
            g_carHistory = new List<PointF>();
        }

        /********************************************************************************************************************************
        * 產生直線路徑
        ********************************************************************************************************************************/
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

        /********************************************************************************************************************************
        * 產生波浪路徑
        ********************************************************************************************************************************/
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

        /********************************************************************************************************************************
        * 產生方形路徑
        ********************************************************************************************************************************/
        PointF[] GenSquare(int width, int height)
        {
            int side = 100, distance = 150;
            int num = height - side * 2;
            PointF point = new PointF((float)distance/2 + side, (float)height - side);
            List<PointF> points = new List<PointF>();

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

        /********************************************************************************************************************************
        * 產生圓形路徑
        ********************************************************************************************************************************/
        PointF[] GenCircle(int width, int height)
        {
            PointF centerPoint = new PointF(width / 2, height / 2);
            PointF startPoint = new PointF((float)width / 2 - 200, (float)450);
            float radius = (float)Pythagorean(centerPoint.X - startPoint.X, centerPoint.Y - startPoint.Y);
            
            PointF point = new PointF();
            List<PointF> points = new List<PointF>();
            
            float perNum = (float)0.01; //解析度
            float distance = 18;
            float endRadius = (float)50; //停止點

            while(radius > endRadius)
            {
                
                for (float i = (float)-3.14, j = 0; i <= (float)3.14; i=i+perNum, j++)
                {
                    point.X = (int)(centerPoint.X + radius * (float)Math.Cos((float)i));
                    point.Y = (int)(centerPoint.Y + radius * (float)Math.Sin((float)i));
                    points.Add(point);

                    radius = radius - perNum * distance;
                    if(radius < endRadius)
                        break;
                }
            }

            return points.ToArray();
        }

        /********************************************************************************************************************************
        * 顯示在圖上
        ********************************************************************************************************************************/
        void DrawOnPic()
        {
            //Console.WriteLine($">> {DateTime.Now.ToString()}");
            pictureBox1.Invalidate();

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;

            Graphics g = Graphics.FromImage(bmp);
            //Graphics g = pictureBox1.CreateGraphics();

            Pen penColorRed = new Pen(Color.Red, 3);
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
            g.DrawCurve(penColorRed, g_waypoints);

            // ============================== 歷史軌跡 =============================
            if (g_carHistory.Count > 1)
                g.DrawCurve(penColorBlu, g_carHistory.ToArray());
            
            // ============================== 最近點 =============================
            g.FillEllipse(brushColorBlue, g_waypoints[g_closeNum].X - 5, g_waypoints[g_closeNum].Y - 5, 10, 10);
            
            
            if(comboBox2.SelectedIndex == 1 || comboBox2.SelectedIndex == 2)
            {
                // ============================ 轉向輪位置 =====================================
                PointF wheelPoint = new PointF();
                if(comboBox2.SelectedIndex == 1)
                {
                    wheelPoint = new PointF(g_carPoint.X + g_carLength * (float)Math.Cos(g_carHead), g_carPoint.Y - g_carLength * (float)Math.Sin(g_carHead));
                }
                else if(comboBox2.SelectedIndex == 2)
                {
                    wheelPoint = new PointF(g_carPoint.X - g_carLength * (float)Math.Cos(g_carHead), g_carPoint.Y + g_carLength * (float)Math.Sin(g_carHead));
                }

                // ============================ 輔助輪位置 =====================================
                PointF carPointRight = new PointF(g_carPoint.X + 30 * (float)Math.Cos(g_carHead + 1.57), g_carPoint.Y - 30 * (float)Math.Sin(g_carHead + 1.57));
                PointF carPointLift = new PointF(g_carPoint.X + 30 * (float)Math.Cos(g_carHead - 1.57), g_carPoint.Y - 30 * (float)Math.Sin(g_carHead - 1.57));
                
                // ============================ 轉向輪 =====================================
                g.DrawLine(penColorThinBlack, wheelPoint.X, wheelPoint.Y, g_carPoint.X, g_carPoint.Y);
                g.DrawLine(penColorBigBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X + 10 * (float)Math.Cos(g_wheelHead), wheelPoint.Y - 10 * (float)Math.Sin(g_wheelHead));
                g.DrawLine(penColorBigBlack, wheelPoint.X, wheelPoint.Y, wheelPoint.X - 10 * (float)Math.Cos(g_wheelHead), wheelPoint.Y + 10 * (float)Math.Sin(g_wheelHead));

                // ============================ 輔助輪右 =====================================
                g.DrawLine(penColorThinBlack, carPointRight.X, carPointRight.Y, g_carPoint.X, g_carPoint.Y);
                g.DrawLine(penColorBigBlack, carPointRight.X, carPointRight.Y, carPointRight.X + 10 * (float)Math.Cos(g_carHead), carPointRight.Y - 10 * (float)Math.Sin(g_carHead));
                g.DrawLine(penColorBigBlack, carPointRight.X, carPointRight.Y, carPointRight.X - 10 * (float)Math.Cos(g_carHead), carPointRight.Y + 10 * (float)Math.Sin(g_carHead));

                // ============================ 輔助輪左 =====================================
                g.DrawLine(penColorThinBlack, carPointLift.X, carPointLift.Y, g_carPoint.X, g_carPoint.Y);
                g.DrawLine(penColorBigBlack, carPointLift.X, carPointLift.Y, carPointLift.X + 10 * (float)Math.Cos(g_carHead), carPointLift.Y - 10 * (float)Math.Sin(g_carHead));
                g.DrawLine(penColorBigBlack, carPointLift.X, carPointLift.Y, carPointLift.X - 10 * (float)Math.Cos(g_carHead), carPointLift.Y + 10 * (float)Math.Sin(g_carHead));
            }
            
            // ============================== 前視距離圓圈 =============================
            if(comboBox3.SelectedIndex == 1)
                g.DrawCurve(penColorGray, g_circlePoints);

            // ============================== 前視距離 =============================
            g.DrawLine(penColorOrg, g_carPoint.X, g_carPoint.Y, g_waypoints[g_frontNum].X, g_waypoints[g_frontNum].Y);
            g.FillEllipse(brushColorOrange, g_waypoints[g_frontNum].X - 5, g_waypoints[g_frontNum].Y - 5, 10, 10);

            // ============================== 車中心 =============================
            g.DrawLine(penColorBlack, g_carPoint.X, g_carPoint.Y, g_carPoint.X + 15 * (float)Math.Cos(g_carHead), g_carPoint.Y - 15 * (float)Math.Sin(g_carHead));
            g.FillEllipse(brushColorBlack, g_carPoint.X - 5, g_carPoint.Y - 5, 10, 10);
        }

        /********************************************************************************************************************************
        * 畢氏定理
        ********************************************************************************************************************************/
        double Pythagorean(float x, float y)
        {
            double ans = Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 0.5);
            return ans;
        }

        /********************************************************************************************************************************
        * 找到最近點
        ********************************************************************************************************************************/
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

        /********************************************************************************************************************************
        * 從最近點 找前視點
        ********************************************************************************************************************************/
        int FindFrontPoint(PointF basic, PointF[] compare, int closeNum, int frontDis, int frontNum = 0)
        {
            double errCalc; //誤差計算暫存
            double err = 999999; //假設誤差極大
            int placeNum = compare.Length - 1; //path index
            // Console.WriteLine("i:" + i); //debug
            for (int i = closeNum; i < compare.Length; i++)
            {
                errCalc = Pythagorean(basic.X - compare[i].X, basic.Y - compare[i].Y); //畢氏定理 找斜邊誤差
                if ((errCalc < err) && (errCalc > frontDis) && (i >= frontNum)) //更小的誤差 大於設定距離 不是過去的點
                {
                    err = errCalc;
                    placeNum = i;
                    break;
                }
            }
            return placeNum;
        }

        /********************************************************************************************************************************
        * 產生半徑
        ********************************************************************************************************************************/
        PointF[] GenCarCircle(PointF basic, PointF[] compare, int frontDis)
        {
            PointF point = new PointF(0, 0); //點
            PointF[] circlePoints = new PointF[314*2 + 1]; //線

            for (int i = -314, j = 0; i <= 314; i++, j++)
            {
                point.X = (int)(basic.X + frontDis * (float)Math.Cos((float)i/100)); //X點
                point.Y = (int)(basic.Y + frontDis * (float)Math.Sin((float)i/100)); //Y點
                circlePoints[j] = point; //存到線
            }

            return circlePoints;
        }

        /********************************************************************************************************************************
        * 半徑內 找前視點 
        ********************************************************************************************************************************/
        int FindRadiusPoint(PointF[] basic, PointF[] compare, int frontNum = 0)
        {
            int placeNum = frontNum; //不走回頭路

            for(int i = 0; i < basic.Length; i++) //basic 大小的 loop
            {
                for(int j = 0; j < compare.Length; j++) //compare 大小的 loop
                {
                    int precision = 2; //精準度 容許誤差
                    // Console.WriteLine(basic[i].X + " " + basic[i].Y + " " + compare[j].X + " " + compare[j].Y);
                    if((Math.Abs(basic[i].X - compare[j].X) < precision) && (Math.Abs(basic[i].Y - compare[j].Y) < precision) && (j > frontNum)) //圓圈與路徑相交點
                    {
                        if(j > placeNum) //最大index點 距離終點最近
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

        /********************************************************************************************************************************
        * 前視距離
        ********************************************************************************************************************************/
        void PurePursuit()
        {
            double errX = 0, errY = 0;
            
            errX = g_waypoints[g_frontNum].X - g_carPoint.X; //X 誤差
            errY = -(g_waypoints[g_frontNum].Y - g_carPoint.Y); //Y 誤差

            g_errDis.err = Pythagorean((float)errX, (float)errY); //斜邊誤差
            g_errSita.err = g_carHead - Math.Atan2(errY, errX); //角度誤差
            
            //屌度誤差限制
            if (g_errSita.err > Math.PI)
                g_errSita.err = g_errSita.err - 2 * Math.PI;
            else if (g_errSita.err < -Math.PI)
                g_errSita.err = g_errSita.err + 2 * Math.PI;
            // Console.WriteLine(errX + "  " + errY + "  " + g_errDis.err + "  " + g_errSita.err); //debug

        }

        /********************************************************************************************************************************
        * PID
        ********************************************************************************************************************************/
        double PidFuc(double kp, double ki, double kd, double err, double errSum, double errLast)
        {
            double ans = kp * err + ki * errSum + kd * (err - errLast);
            return ans;
        }

        /********************************************************************************************************************************
        * 逆向運動學
        ********************************************************************************************************************************/
        Tuple<double, double> Kinematics(int basicSpeed)
        {
            double carV = 0, carW = 0;

            carV = PidFuc(g_errDis.kp, g_errDis.ki, g_errDis.kd, g_errDis.err, g_errDis.errsum, g_errDis.errlast) + basicSpeed; //pid 求V
            carW = PidFuc(g_errSita.kp, g_errSita.ki, g_errSita.kd, g_errSita.err, g_errSita.errsum, g_errSita.errlast); //pid 求W
            g_errDis.errlast = g_errDis.err; //紀錄v誤差
            g_errDis.errsum += g_errDis.err; //紀錄v累計誤差
            g_errSita.errlast = g_errSita.err; //紀錄w誤差
            g_errSita.errsum += g_errSita.err; //紀錄w累計誤差
            // Console.WriteLine("carhead:" + carHead + "  atan:" + Math.Atan2(errY, errX) + "  errSita:" + errSita.err + "  carV:" + carV + "  carW:" + carW); //debug
        
            return Tuple.Create(carV, carW);
        }

        /********************************************************************************************************************************
        * 單點運動學
        ********************************************************************************************************************************/
        void MovePointCar(double carV, double carW)
        {
            g_carHistory.Add(g_carPoint);  //速度限制

            if (carV > 30)  //速度限制
                carV = 30;

            g_carPoint.X += (float)(carV * Math.Cos(g_carHead)); //正向運動學 求車中心X
            g_carPoint.Y -= (float)(carV * Math.Sin(g_carHead)); //正向運動學 求車中心Y
            g_carHead -= carW; //正向運動學 求車中心角度
        }

        /********************************************************************************************************************************
        * 差車正向運動學
        ********************************************************************************************************************************/
        void MoveForkliftFront(double carV, double carW)
        {
            g_carHistory.Add(g_carPoint); //記錄歷史軌跡

            if (carV > 30)  //速度限制
                carV = 30;

            g_carPoint.X += (float)(carV * Math.Cos(g_carHead)); //正向運動學 求車中心X
            g_carPoint.Y -= (float)(carV * Math.Sin(g_carHead)); //正向運動學 求車中心Y
            g_carHead -= carW; //正向運動學 求車中心角度
            g_wheelHead = g_carHead + Math.Atan(carW * -1 * g_carLength / carV); //正向運動學 求轉向輪角度
        }

        /********************************************************************************************************************************
        * 差車反向運動學(叉子導航)
        ********************************************************************************************************************************/
        void MoveForkliftReverse(double carV, double carW)
        {
            g_carHistory.Add(g_carPoint); //記錄歷史軌跡

            if (carV > 30) //速度限制
                carV = 30;

            g_carPoint.X += (float)(carV * Math.Cos(g_carHead)); //正向運動學 求車中心X
            g_carPoint.Y -= (float)(carV * Math.Sin(g_carHead)); //正向運動學 求車中心Y
            g_carHead -= carW; //正向運動學 求車中心角度
            g_wheelHead = g_carHead - Math.Atan(carW * -1 * g_carLength / carV); //正向運動學 求轉向輪角度
        }

        /********************************************************************************************************************************
        * 抓面板設定
        ********************************************************************************************************************************/
        void GetControlInfo()
        {
            g_frontDis = trackBar1.Value;
            g_basicSpeed = trackBar2.Value;
            g_accuracy = trackBar3.Value / (float)10;
            g_errDis.kp = trackBar4.Value / (float)100;
            g_errDis.ki = trackBar5.Value / (float)100;
            g_errDis.kd = trackBar6.Value / (float)100;
            g_errSita.kp = trackBar7.Value / (float)100;
            g_errSita.ki = trackBar8.Value / (float)100;
            g_errSita.kd = trackBar9.Value / (float)100;
            g_carLength = trackBar10.Value;

            label10.Text = Convert.ToString(g_frontDis);
            label11.Text = Convert.ToString(g_basicSpeed);
            label12.Text = Convert.ToString(g_accuracy);
            label13.Text = Convert.ToString(g_errDis.kp);
            label14.Text = Convert.ToString(g_errDis.ki);
            label15.Text = Convert.ToString(g_errDis.kd);
            label16.Text = Convert.ToString(g_errSita.kp);
            label17.Text = Convert.ToString(g_errSita.ki);
            label18.Text = Convert.ToString(g_errSita.kd);
            label23.Text = Convert.ToString(g_carLength);
        }

        /********************************************************************************************************************************
        * main
        ********************************************************************************************************************************/
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Console.WriteLine("timer1 tick");

            GetControlInfo(); //抓面板設定

            if(comboBox3.SelectedIndex == 0)
            {
                g_closeNum = FindClosestPoint(g_carPoint, g_waypoints); //找到路徑上離車的最近點
                g_frontNum = FindFrontPoint(g_waypoints[g_closeNum], g_waypoints, g_closeNum, g_frontDis, g_frontNum); //算出前視點
            }
            else if(comboBox3.SelectedIndex == 1)
            {
                g_closeNum = FindClosestPoint(g_carPoint, g_waypoints); //找到路徑上離車的最近點
                g_circlePoints = GenCarCircle(g_carPoint, g_waypoints, g_frontDis); //產生車子圓圈
                g_frontNum = FindRadiusPoint(g_circlePoints, g_waypoints, g_frontNum); //算出前視點
            }

            PurePursuit(); //算出前視點與車子的誤差
            var carVW = Kinematics(g_basicSpeed); //算出對應的V W
            if(comboBox2.SelectedIndex == 0)
                MovePointCar(carVW.Item1, carVW.Item2); //單一點運動學 算出新的位置
            else if(comboBox2.SelectedIndex == 1)
                MoveForkliftFront(carVW.Item1, carVW.Item2); //差車前進運動學 算出新的位置
            else if(comboBox2.SelectedIndex == 2)
                MoveForkliftReverse(carVW.Item1, carVW.Item2); //差車後退運動學 算出新的位置

            g_closeNum = FindClosestPoint(g_carPoint, g_waypoints); //算出新的最近點

            DrawOnPic(); //輸出

            if (g_errDis.err < g_accuracy && g_errSita.err < g_accuracy) //容許範圍內到達目標點
            {
                ReStart();
            }
        }

        /********************************************************************************************************************************
        * 視窗開啟
        ********************************************************************************************************************************/
        public Form1()
        {
            InitializeComponent();   

            comboBox1.SelectedIndex = 0; //預設下拉選單
            comboBox2.SelectedIndex = 0; //預設下拉選單
            comboBox3.SelectedIndex = 0; //預設下拉選單

            ReStart();
        }

        /********************************************************************************************************************************
        * restart 按鈕
        ********************************************************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            ReStart();
        }

        /********************************************************************************************************************************
        * 路線選單
        ********************************************************************************************************************************/
        private void PathChange(object sender, EventArgs e)
        {
            ReStart();
        }
    }
}
