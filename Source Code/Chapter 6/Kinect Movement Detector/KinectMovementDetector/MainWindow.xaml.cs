using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

using Microsoft.Kinect;
using Emgu.CV;

namespace KinectMovementDetector
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private KinectSensor kinect;
        FrameDescription colorFrameDesc;
        private WriteableBitmap colorBitmap;

        Mat priorFrame;
        Queue<Mat> subtractedMats = new Queue<Mat>();
        Mat culmativeFrame;

        public MainWindow()
        {
            kinect = KinectSensor.GetDefault();
            ColorFrameSource colorFrameSource = kinect.ColorFrameSource;
            colorFrameDesc = colorFrameSource.FrameDescription;
            ColorFrameReader colorFrameReader = colorFrameSource.OpenReader();
            colorFrameReader.FrameArrived += Color_FrameArrived;
            colorBitmap = new WriteableBitmap(colorFrameDesc.Width,
                colorFrameDesc.Height,
                96.0,
                96.0,
                PixelFormats.Gray8,
                null);

            DataContext = this;

            kinect.Open();

            InitializeComponent();
        }

        public ImageSource ImageSource
        {
            get
            {
                return colorBitmap;
            }
        }

        private void Color_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    if ((colorFrameDesc.Width == colorBitmap.PixelWidth) && (colorFrameDesc.Height == colorBitmap.PixelHeight))
                    {
                        using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                        {

                            Mat img = new Mat(colorFrameDesc.Height, colorFrameDesc.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
                            colorFrame.CopyConvertedFrameDataToIntPtr(img.DataPointer, (uint)(colorFrameDesc.Width * colorFrameDesc.Height * 4), ColorImageFormat.Bgra);
                            CvInvoke.CvtColor(img, img, Emgu.CV.CvEnum.ColorConversion.Bgra2Gray);

                            if (priorFrame != null)
                            {
                                CvInvoke.Subtract(priorFrame, img, priorFrame);
                                CvInvoke.Threshold(priorFrame, priorFrame, 20, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                                CvInvoke.GaussianBlur(priorFrame, priorFrame, new System.Drawing.Size(3, 3), 5);
                                subtractedMats.Enqueue(priorFrame);
                            }
                            if (subtractedMats.Count > 4)
                            {
                                subtractedMats.Dequeue().Dispose();

                                Mat[] subtractedMatsArray = subtractedMats.ToArray();
                                culmativeFrame = subtractedMatsArray[0];

                                for (int i = 1; i < 4; i++)
                                {
                                    CvInvoke.Add(culmativeFrame, subtractedMatsArray[i], culmativeFrame);
                                }
                                colorBitmap.Lock();

                                CopyMemory(colorBitmap.BackBuffer, culmativeFrame.DataPointer, (uint)(colorFrameDesc.Width * colorFrameDesc.Height));
                                colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));

                                colorBitmap.Unlock();
                            }
                            priorFrame = img.Clone();
                            img.Dispose();
                        }
                    }
                }
            }
        }
    }
}
