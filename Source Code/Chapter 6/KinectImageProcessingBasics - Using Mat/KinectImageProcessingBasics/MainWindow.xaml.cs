using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Emgu.CV;
using System.Runtime.InteropServices;
using System;

namespace KinectImageProcessingBasics
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private KinectSensor kinect;
        FrameDescription colorFrameDesc;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;

        public MainWindow()
        {
            kinect = KinectSensor.GetDefault();
            ColorFrameSource colorFrameSource = kinect.ColorFrameSource;
            colorFrameDesc = colorFrameSource.FrameDescription;
            ColorFrameReader colorFrameReader = colorFrameSource.OpenReader();
            colorFrameReader.FrameArrived += Color_FrameArrived;
            colorPixels = new byte[colorFrameDesc.Width * colorFrameDesc.Height * 4];
            colorBitmap = new WriteableBitmap(colorFrameDesc.Width,
                colorFrameDesc.Height,
                96.0,
                96.0,
                PixelFormats.Bgra32, //Gray8 if grayscale
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
                            colorBitmap.Lock();

                            Mat img = new Mat(colorFrameDesc.Height, colorFrameDesc.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
                            colorFrame.CopyConvertedFrameDataToIntPtr(img.DataPointer, (uint)(colorFrameDesc.Width * colorFrameDesc.Height * 4), ColorImageFormat.Bgra);



                            //uncomment to gray scale
                            //CvInvoke.CvtColor(img, img, Emgu.CV.CvEnum.ColorConversion.Bgra2Gray);
                            ////Threshold call
                            ////CvInvoke.Threshold(img, img, 220, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

                            //CopyMemory(colorBitmap.BackBuffer, img.DataPointer, (uint)(colorFrameDesc.Width * colorFrameDesc.Height));

                            //comment out if grayscaled
                            CopyMemory(colorBitmap.BackBuffer, img.DataPointer, (uint)(colorFrameDesc.Width * colorFrameDesc.Height * 4));

                            colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));

                            colorBitmap.Unlock();
                            img.Dispose();
                        }
                    }
                }
            }
        }
    }
}
