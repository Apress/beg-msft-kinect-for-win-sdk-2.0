using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.ComponentModel;

namespace TakingaScreenshotinWPF
{
    public partial class MainWindow : Window
    {
        private KinectSensor kinect = null;
        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;

        public MainWindow()
        {
            kinect = KinectSensor.GetDefault();
            colorFrameReader = kinect.ColorFrameSource.OpenReader();

            colorFrameReader.FrameArrived += Reader_ColorFrameArrived;

            FrameDescription colorFrameDescription = kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            kinect.Open();

            DataContext = this;
            InitializeComponent();
        }

        public ImageSource ImageSource
        {
            get
            {
                return colorBitmap;
            }
        }

        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        colorBitmap.Lock();

                        if ((colorFrameDescription.Width == colorBitmap.PixelWidth) && (colorFrameDescription.Height == colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));
                        }

                        colorBitmap.Unlock();
                    }
                }
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (colorFrameReader != null)
            {
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }

            if (kinect != null)
            {
                kinect.Close();
                kinect = null;
            }
        }


        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            if (colorBitmap != null)
            {

                BitmapEncoder encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(colorBitmap));

                string photoLoc = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string time = DateTime.Now.ToString("d MMM yyyy hh - mm - ss");

                string path = Path.Combine(photoLoc, "Screenshot " + time + ".png");

                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                    //provide some confirmation to user that picture was taken
                }
                catch (IOException)
                {
                    //inform user that picture failed to save for whatever reason
                }

            }
        }
    }
}
