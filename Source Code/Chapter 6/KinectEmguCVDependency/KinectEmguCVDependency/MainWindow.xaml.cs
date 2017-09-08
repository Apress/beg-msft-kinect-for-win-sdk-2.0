using System.Windows;
using Emgu.CV;
using Microsoft.Kinect;

namespace KinectEmguCVDependency
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            Mat img = CvInvoke.Imread("img.jpg", Emgu.CV.CvEnum.LoadImageType.AnyColor);
            CvInvoke.Imshow("Test image", img);
        }
    }
}
