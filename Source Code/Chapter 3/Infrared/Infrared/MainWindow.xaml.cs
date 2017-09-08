using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.ComponentModel;

namespace Infrared
{
    public partial class MainWindow : Window
    {
        private const float InfrDataScale = 0.75f;
        private const float InfrMinVal = 0.01f;
        private const float InfrMaxVal = 1.0f;

        private KinectSensor kinect = null;
        private InfraredFrameReader infraredFrameReader = null;
        private WriteableBitmap infrBitmap = null;

        public MainWindow()
        {

            kinect = KinectSensor.GetDefault();
            infraredFrameReader = kinect.InfraredFrameSource.OpenReader();

            infraredFrameReader.FrameArrived += Reader_InfraredFrameArrived;

            FrameDescription infraredFrameDescription = kinect.InfraredFrameSource.FrameDescription;

            infrBitmap = new WriteableBitmap(infraredFrameDescription.Width, infraredFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray32Float, null);

            kinect.Open();

            DataContext = this;
            InitializeComponent();
        }

        public ImageSource ImageSource
        {
            get
            {
                return infrBitmap;
            }
        }

        private void Reader_InfraredFrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            using (InfraredFrame infrFrame = e.FrameReference.AcquireFrame())
            {
                if (infrFrame != null)
                {
                    using (KinectBuffer infrBuffer = infrFrame.LockImageBuffer())
                    {

                        if ((infrFrame.FrameDescription.Width * infrFrame.FrameDescription.Height) == (infrBuffer.Size / infrFrame.FrameDescription.BytesPerPixel))
                        {
                            this.ProcessInfraredFrameData(infrBuffer.UnderlyingBuffer, infrBuffer.Size, infrFrame.FrameDescription.BytesPerPixel);
                        }
                    }
                }
            }
        }

        private unsafe void ProcessInfraredFrameData(IntPtr infrFrameData, uint infrFrameDataSize, uint bytesPerPix)
        {
            ushort* frameData = (ushort*)infrFrameData;

            infrBitmap.Lock();

            float* backBuffer = (float*)infrBitmap.BackBuffer;

            for (int i = 0; i < (int)(infrFrameDataSize / bytesPerPix); ++i)
            {
                backBuffer[i] = Math.Min(InfrMaxVal, (float)frameData[i] / ushort.MaxValue * InfrDataScale * (1.0f - InfrMinVal) + InfrMinVal);
            }

            infrBitmap.AddDirtyRect(new Int32Rect(0, 0, infrBitmap.PixelWidth, infrBitmap.PixelHeight));

            infrBitmap.Unlock();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (infraredFrameReader != null)
            {
                infraredFrameReader.Dispose();
                infraredFrameReader = null;
            }

            if (kinect != null)
            {
                kinect.Close();
                kinect = null;
            }
        }
    }
}
