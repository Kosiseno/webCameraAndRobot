using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using DirectShowLib;

namespace webCamera
{
    public partial class Form1 : Form
    {
        private VideoCapture capture = null; // Поле для захвата данных с камеры

        private DsDevice[] webCams = null;

        private int selectedCameraId = 0; //ID камеры в списке webCams

        SerialPort port; // для управления ардуинкой

        private Image<Bgr, byte> imageContr = null;

        Reverse reverse = new Reverse();

        double x,y; // для координат курсора

        double[] theta = { 0.0, 0.0, 0.0 }; // для работы с областью cхвата манипулятора


        public Form1()
        {
            InitializeComponent();
        }

        // нажатие на кнопку смотреть
        private void toolStripButton1_Click(object sender, EventArgs e) 
        {
            try
            {
                if (webCams.Length == 0)
                {
                    throw new Exception("Нет доступных камер!");
                }
                else if (toolStripComboBox1.SelectedItem == null)
                {
                    throw new Exception("Необходимо выбрать камеру!");
                }
                else if (capture != null)
                {
                    capture.Start();
                }
                else
                {
                    capture = new VideoCapture(selectedCameraId);

                    capture.ImageGrabbed += Capture_ImageGrabbed;

                    capture.Start();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();

                capture.Retrieve(m);

                pictureBox1.Image = m.ToImage<Bgr, byte>().Flip(Emgu.CV.CvEnum.FlipType.Horizontal).Bitmap;

                AcceptMouseMoveDelagate(this, 0, 0);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //загрузка формы
        private void Form1_Load(object sender, EventArgs e)
        {
            webCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            
            // перебераем в цикле массив для заполненя бокса

            for(int i = 0; i < webCams.Length; i++)
            {
                toolStripComboBox1.Items.Add(webCams[i].Name);
            }
        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCameraId = toolStripComboBox1.SelectedIndex;
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture != null)
                {
                    capture.Pause();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture != null)
                {
                    capture.Pause();

                    capture.Dispose();

                    capture = null;

                    pictureBox1.Image.Dispose();

                    pictureBox1.Image = null;

                    selectedCameraId = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            StringBuilder msgBuilder = new StringBuilder("Performance: ");

            try
            {
                Mat contr = new Mat();

                UMat uimage = new UMat();

                capture.Retrieve(contr);
 
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

                imageContr = capture.QueryFrame().ToImage<Bgr, byte>();

                CvInvoke.CvtColor(imageContr, uimage, ColorConversion.Bgr2Gray);
                
                UMat pyrDown = new UMat();
                CvInvoke.PyrDown(uimage, pyrDown);
                CvInvoke.PyrUp(pyrDown, uimage);


                //попытка прикрутить распознование объектов
                #region circle detection
                Stopwatch watch = Stopwatch.StartNew();
                double cannyThreshold = 100.0;
                double circleAccumulatorThreshold = 60;
                CircleF[] circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 1.0, 2.0, cannyThreshold, circleAccumulatorThreshold);

                watch.Stop();
                msgBuilder.Append(String.Format("Hough circles - {0} ms; ", watch.ElapsedMilliseconds));
                #endregion 

                #region draw circles
                Image<Bgr, Byte> circleImage = imageContr.CopyBlank();
                foreach (CircleF circle in circles)
                    circleImage.Draw(circle, new Bgr(Color.Brown), 2);
                pictureBox2.Image = circleImage.Bitmap;
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
       
        private void label1_Click(object sender, EventArgs e)
        {
            label1.Text = capture.Width.ToString();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Point p = pictureBox1.PointToClient(System.Windows.Forms.Cursor.Position);
            label1.Text = p.X.ToString();
        }

        public void AcceptMouseMoveDelagate(Control c, int offsetX, int offsetY)
        {
            c.MouseMove += (sender, e) => label1.Text = string.Format("{0}:{1}", offsetX + e.X - pictureBox1.Location.X -320 , offsetY + e.Y - pictureBox1.Location.Y);
            foreach (Control control in c.Controls) {
                AcceptMouseMoveDelagate(control, offsetX + control.Location.X, offsetY + control.Location.Y);
            }
        }

        private void servoControll() // Подключаемся к ардунке
        {
            SerialPort port = new SerialPort();
            port.PortName = "COM9";
            port.BaudRate = 9600;

            try
            {
                port.Open();
                string test = "q=" +theta[0] + ";w=" +theta[1] + ";e=" +theta[2];
                // port.Write(string.Format("q={0};w={1};e={2}", theta[0], theta[1], theta[2]));
                port.Write(test);
                port.Close();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            string[] logTheta = label1.Text.Split(':');
            
            theta = reverse.reverse(Convert.ToDouble(logTheta[0])*-0.00069, Convert.ToDouble(logTheta[1])*0.00069, 0.05);
            label2.Text = string.Format("{0}:{1}:{2}",theta[0],theta[1],theta[2]);
            servoControll();
        }
    }
}
