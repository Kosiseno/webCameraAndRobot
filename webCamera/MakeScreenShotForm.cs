using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.Util;
using System.IO;

namespace webCamera
{
    public partial class MakeScreenShotForm : Form
    {
        private Image<Bgr, byte> image = null;

        private string fileName = string.Empty;

        public MakeScreenShotForm(Image<Bgr, byte> image)
        {
            this.image = image;

            InitializeComponent();


        }

        private void MakeScreenShotForm_Load(object sender, EventArgs e)
        {
            fileName = $"WCVC_{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}_{DateTime.Now.Hour}_{DateTime.Now.Minute}.jpg";

            pictureBox1.Image = image.Bitmap;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {

                Image<Gray, byte> outputImage = image.Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

                Mat hierarchy = new Mat();

                CvInvoke.FindContours(outputImage, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);


                {
                    Image<Gray, byte> blackBackground = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));

                    CvInvoke.DrawContours(blackBackground, contours, -1, new MCvScalar(255, 0, 0), 3);

                    pictureBox1.Image = blackBackground.Bitmap;

                    pictureBox1.Image.Save(fileName, ImageFormat.Jpeg);

                    if (File.Exists(fileName))
                    {
                        Close();
                    }
                    else
                    {
                        throw new Exception("Не удалось сохранить изображение!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
