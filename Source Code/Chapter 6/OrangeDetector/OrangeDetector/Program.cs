using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;

namespace OrangeDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            String win1 = "Orange Detector"; //The name of the window
            CvInvoke.NamedWindow(win1); //Create the window using the specific name

            MCvScalar orangeMin = new MCvScalar(10, 211, 140);
            MCvScalar orangeMax = new MCvScalar(18, 255, 255);

            Mat img = new Mat("fruits.jpg", ImreadModes.AnyColor);
            Mat hsvImg = new Mat();
            CvInvoke.CvtColor(img, hsvImg, ColorConversion.Bgr2Hsv);

            CvInvoke.InRange(hsvImg, new ScalarArray(orangeMin), new ScalarArray(orangeMax), hsvImg);

            CvInvoke.MorphologyEx(hsvImg, hsvImg, MorphOp.Close, new Mat(), new Point(-1, -1), 5, BorderType.Default, new MCvScalar());

            SimpleBlobDetectorParams param = new SimpleBlobDetectorParams();
            param.FilterByCircularity = false;
            param.FilterByConvexity = false;
            param.FilterByInertia = false;
            param.FilterByColor = false;
            param.MinArea = 1000;
            param.MaxArea = 50000;

            SimpleBlobDetector detector = new SimpleBlobDetector(param);
            MKeyPoint[] keypoints = detector.Detect(hsvImg);
            Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(keypoints), img, new Bgr(255, 0, 0), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);

            CvInvoke.Imshow(win1, img); //Show image
            CvInvoke.WaitKey(0); //Wait for key press before executing next line 
            CvInvoke.DestroyWindow(win1);
        }
    }
}
