using System;

using System.IO;

namespace webCamera
{

public class Reverse
{
        string[] thetaRead;

    public double[] reverse(double x, double y, double z)
	{
        double delta = 0.007;
            StreamReader str = new StreamReader("log2.txt");
            while (!str.EndOfStream)
            {
                string st =  str.ReadLine() ;

                thetaRead = st.Split(';');
                double[] cord = { Convert.ToDouble(thetaRead[0]), Convert.ToDouble(thetaRead[1]),
                    Convert.ToDouble(thetaRead[2]) };

                if ((x + delta > cord[0]) & (x - delta < cord[0]) &
                    (y + delta > cord[1]) & (y - delta < cord[1]) &
                    (z + delta > cord[2]) & (z - delta < cord[2]))
                {
                    double[] theta = { Convert.ToDouble(thetaRead[3]), Convert.ToDouble(thetaRead[4]),
                        Convert.ToDouble(thetaRead[5]) };

                    return theta;
                }
                else Array.Clear(thetaRead,0,4);
            }
            double[] thetaNo = { 0,0,0};
            return thetaNo;
                    }
        }
	}

        

