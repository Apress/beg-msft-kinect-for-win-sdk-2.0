using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Microsoft.Kinect;
using System.Windows.Controls;

namespace PixelDistance
{
    public partial class MainWindow : Window
    {
        private KinectSensor kinect = null;
        private DepthFrameReader depthFrameReader = null;
        private WriteableBitmap depthBitmap = null;
        FrameDescription depthFrameDescription = null;

        private const int MapDepthToByte = 8000 / 256;
        private byte[] depthPixels = null;
        private int[] depthData = null;

        public MainWindow()
        {

            kinect = KinectSensor.GetDefault();
            depthFrameReader = kinect.DepthFrameSource.OpenReader();

            depthFrameReader.FrameArrived += Reader_DepthFrameArrived;

            depthFrameDescription = kinect.DepthFrameSource.FrameDescription;

            depthPixels = new byte[depthFrameDescription.Width * depthFrameDescription.Height *4];
            depthBitmap = new WriteableBitmap(depthFrameDescription.Width, depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);
            depthData = new int[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

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

            int colorByteIndex = 0;

            Color color = new Color();

            for (int i = 0; i < (int)(depthFrameDataSize / depthFrameDescription.BytesPerPixel); ++i)
            {

                ushort depth = frameData[i];
                depthData[i] = depth;
                float depthPercentage = (depth / 8000f);

                if (depthPercentage <= 0)
                {
                    color.B = 0;
                    color.G = 0;
                    color.R = 0;
                }
                if (depthPercentage > 0f & depthPercentage <= 0.2f)
                {
                    color.B = 255;
                    color.G = 0;
                    color.R = 0;
                }
                if (depthPercentage > 0.2f & depthPercentage <= 0.4f)
                {
                    color.B = 0;
                    color.G = 255;
                    color.R = 0;
                }
                if (depthPercentage > 0.4f & depthPercentage <= 0.6f)
                {
                    color.B = 0;
                    color.G = 0;
                    color.R = 255;
                }
                if (depthPercentage >= 0.6f)
                {
                    color.B = 0;
                    color.G = 255;
                    color.R = 255;
                }

                depthPixels[colorByteIndex++] = color.B;
                depthPixels[colorByteIndex++] = color.G;
                depthPixels[colorByteIndex++] = color.R;
                depthPixels[colorByteIndex++] = 255;

            }

        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point p = e.GetPosition(DepthImage);

            int index = ((int)p.X + ((int)p.Y * depthBitmap.PixelWidth));
            int depth = depthData[index];

            ToolTip toolTip = new ToolTip { Content = depth + "mm", FontSize = 36, IsOpen = true, StaysOpen = false };
            DepthImage.ToolTip = toolTip;

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
