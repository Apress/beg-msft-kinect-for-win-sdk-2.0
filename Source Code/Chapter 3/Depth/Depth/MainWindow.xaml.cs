using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.ComponentModel;

namespace Depth
{
    public partial class MainWindow : Window
    {
        private KinectSensor kinect = null;
        private DepthFrameReader depthFrameReader = null;
        private WriteableBitmap depthBitmap = null;
        FrameDescription depthFrameDescription = null;

        private const int MapDepthToByte = 8000 / 256;
        private byte[] depthPixels = null;

        public MainWindow()
        {

            kinect = KinectSensor.GetDefault();
            depthFrameReader = kinect.DepthFrameSource.OpenReader();

            depthFrameReader.FrameArrived += Reader_DepthFrameArrived;

            depthFrameDescription = kinect.DepthFrameSource.FrameDescription;

            depthPixels = new byte[depthFrameDescription.Width * depthFrameDescription.Height];
            depthBitmap = new WriteableBitmap(depthFrameDescription.Width, depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

            kinect.Open();

            DataContext = this;
            InitializeComponent();
        }

        public ImageSource ImageSource
        {
            get
            {
                return depthBitmap;
            }
        }

        private void Reader_DepthFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    using (KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        if ((depthFrameDescription.Width * depthFrameDescription.Height) == (depthBuffer.Size / depthFrameDescription.BytesPerPixel))
                        {

                            ushort maxDepth = ushort.MaxValue;

                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {

                depthBitmap.WritePixels(
                    new Int32Rect(0, 0, depthBitmap.PixelWidth, depthBitmap.PixelHeight),
                    depthPixels,
                    depthBitmap.PixelWidth,
                    0);
            }
        }

        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {

            ushort* frameData = (ushort*)depthFrameData;

            for (int i = 0; i < (int)(depthFrameDataSize / depthFrameDescription.BytesPerPixel); ++i)
            {

                ushort depth = frameData[i];

                depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (depthFrameReader != null)
            {
                depthFrameReader.Dispose();
                depthFrameReader = null;
            }

            if (kinect != null)
            {
                kinect.Close();
                kinect = null;
            }
        }
    }
}
