using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp_20200715
{
    [System.Runtime.InteropServices.Guid("67B3F98B-8D62-4F76-BE2E-5C6BB74E1309")]
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //==================================按鈕顯示============================================
        private void button1_Click_1(object sender, EventArgs e)
        {
            label6.Text = "1";
        }


        private void button2_Click(object sender, EventArgs e)
        {
            label6.Text = "2";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label6.Text = "3";
        }

        //==================================計時器============================================
        private void timer1_Tick(object sender, EventArgs e)
        {
            label7.Text = DateTime.Now.ToString();
            Console.WriteLine($">> {DateTime.Now.ToString()}");
            listBox2.Items.Add(DateTime.Now.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            timer1.Enabled = true;
        }


        private void button5_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        //==================================畫圖============================================
        private void button6_Click(object sender, EventArgs e)
        {
            //pictureBox2.Invalidate();

            Bitmap bmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);

            Graphics g = Graphics.FromImage(bmp);

            Brush bb = new SolidBrush(Color.Pink);
            g.FillRectangle(bb, 0, 0, 240, 320);

            bb = new SolidBrush(Color.Orange);
            g.FillEllipse(bb, 20, 20, 100, 100);

            Pen color = new Pen(Color.White, 10);
            Point p1 = new Point(10, 20);
            Point p2 = new Point(10, 205);
            Point p3 = new Point(10, 200);
            Point p4 = new Point(200, 200);

            g.DrawLine(color, p1, p2);
            g.DrawLine(color, p3, p4);

            pictureBox2.Image = bmp;

        }

        private void button7_Click(object sender, EventArgs e)
        {

            //pictureBox2.Invalidate();

            Bitmap img = new Bitmap(@"C:\Users\user\Desktop\WindowsFormsApp_20200715\2533867-XXL.jpg");

            Graphics g = pictureBox2.CreateGraphics();

            //g.DrawImage(img, 0, 0);
            pictureBox2.Image = img;


        }

        private void button8_Click(object sender, EventArgs e)
        {
            Graphics g = pictureBox2.CreateGraphics();

            Brush bb = new SolidBrush(Color.Red);

            g.FillEllipse(bb, 20, 20, 100, 100);


        }

        //==================================串列通訊============================================
        private void button12_Click(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(SerialPort.GetPortNames());
        }

        SerialPort serialPortVar;
        private void button9_Click(object sender, EventArgs e)
        {
            if(comboBox2.Text != "")
            {
                serialPortVar = new SerialPort();
                serialPortVar.PortName = comboBox2.Text;
                serialPortVar.BaudRate = Convert.ToInt32(textBox3.Text);
                if(!serialPortVar.IsOpen)
                {
                    serialPortVar.Open();
                    serialPortVar.DataReceived += new SerialDataReceivedEventHandler(Rece);
                }
            }
        }

        private void Rece(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if(serialPortVar.BytesToRead > 0)
            {
                byte[] bufferData = new byte[serialPortVar.BytesToRead];
                serialPortVar.Read(bufferData, 0, bufferData.Length);
                char[] output = Encoding.ASCII.GetChars(bufferData);

                BeginInvoke(new MethodInvoker(() =>
                {
                    foreach (char i in output)
                        textBox5.AppendText(Convert.ToString(i));
                    textBox5.AppendText("\r\n");
                }));
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            char[] x = new char[textBox4.Text.Length];

            if (serialPortVar.IsOpen)
            {
                for (int i = 0; i < textBox4.Text.Length; i++)
                    x[i] = textBox4.Text[i];
                serialPortVar.Write(x, 0, x.Length);
;            }

        }

        private void button11_Click(object sender, EventArgs e)
        {
            serialPortVar.Close();
        }

        //==================================畫沿線============================================
        int pointPlace = 0;
        Bitmap pBmp;
        bool flag = false;

        private void btStart_Click(object sender, EventArgs e)
        {
            if(timer2.Enabled != true)
            {
                timer2.Enabled = true;
                btStart.Text = "Stop";
            }
            else
            {
                timer2.Enabled = false;
                btStart.Text = "Start";
            }
            
        }

        
        private void timer2_Tick(object sender, EventArgs e)
        {

            pictureBox1.Invalidate();
            flag = true;
            //bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //pictureBox1.Image = bmp;



        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(textBox1.Text != "")
                timer2.Interval = Convert.ToInt32(textBox1.Text);
            //Console.WriteLine(textBox1.Text);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if(timer2.Enabled == true && flag == true)
            {
                drawOnPic();
                flag = false;
            }
                
            
        }

        void drawOnPic()
        {
            //Console.WriteLine($">> {DateTime.Now.ToString()}");
            //pictureBox1.Invalidate();

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;

            Graphics g = Graphics.FromImage(bmp);
            //Graphics g = pictureBox1.CreateGraphics();
            Pen color = new Pen(Color.Black, 2);

            Brush bb = new SolidBrush(Color.Red);

            if (comboBox1.SelectedItem.ToString() == "直線")
            {
                Point p1 = new Point(pictureBox1.Width / 2 - 10 + trackBar1.Value, 0 + trackBar2.Value);
                Point p2 = new Point(pictureBox1.Width / 2 - 10 + trackBar1.Value, pictureBox1.Height + trackBar2.Value);
                Point p3 = new Point(pictureBox1.Width / 2 + 10 + trackBar1.Value, 0 + trackBar2.Value);
                Point p4 = new Point(pictureBox1.Width / 2 + 10 + trackBar1.Value, pictureBox1.Height + trackBar2.Value);

                g.DrawLine(color, p1, p2);
                g.DrawLine(color, p3, p4);

                g.FillEllipse(bb, pictureBox1.Width / 2 - 5 + trackBar1.Value, pointPlace + trackBar2.Value, 10, 10);

                //listBox1.Items.Add(pictureBox1.Width / 2 - 5 + trackBar1.Value + "," + pointPlace);
                textBox6.AppendText(pictureBox1.Width / 2 - 5 + trackBar1.Value + " , " + pointPlace + "\r\n");
            }



            else if (comboBox1.SelectedItem.ToString() == "弧線")
            {

                PointF[] points = new PointF[pictureBox1.Height];
                for (int i = 0; i < pictureBox1.Height; i++)
                {
                    if (textBox2.Text != "")
                        points[i].X = pictureBox1.Width / 2 + (float)Math.Sin(i / (float)pictureBox1.Height * 2 * (Math.PI)) * Convert.ToInt32(textBox2.Text) + trackBar1.Value;
                    //points[i].X = pictureBox1.Width / 2 + (float)Math.Sin((Math.PI)/ (float)180/ (Math.PI) * (float)i *360.0/390.0) * Convert.ToInt32(textBox1.Text) + trackBar1.Value ;
                    else
                        points[i].X = pictureBox1.Width / 2;
                    points[i].Y = (float)i + trackBar2.Value;

                }
                g.DrawCurve(color, points);

                g.FillEllipse(bb, points[pointPlace].X, points[pointPlace].Y, -10, -10);

                //listBox1.Items.Add(points[pointPlace].X + "," + points[pointPlace].Y);
                textBox6.AppendText(points[pointPlace].X + " , " + points[pointPlace].Y + "\r\n");
            }

            if (pointPlace >= pictureBox1.Height - 2)
                pointPlace = 0;


            pointPlace = pointPlace + 2;
            pictureBox1.Image = bmp;
            
            
        }

    }
}
