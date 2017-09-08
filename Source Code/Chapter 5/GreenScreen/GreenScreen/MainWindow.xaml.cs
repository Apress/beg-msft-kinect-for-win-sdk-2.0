using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace GreenScreen
{
    public partial class MainWindow : Window
    {
        private KinectSensor kinect;
        private MultiSourceFrameReader multiSourceFrameReader;
        private FrameDescription colorFrameDescription;
        private FrameDescription depthFrameDescription;
        private CoordinateMapper coordinateMapper;

        private WriteableBitmap colorBitmap;

        private ushort[] depthPixels;
        private ColorSpacePoint[] colorSpacePoints;
        private byte[] bodyIndexPixels;
        private byte[] colorPixels;
        private byte[] bitmapPixels;

        public MainWindow()
        {
            kinect = KinectSensor.GetDefault();
            multiSourceFrameReader = kinect.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.BodyIndex | FrameSourceTypes.Depth);
            colorFrameDescription = kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            depthFrameDescription = kinect.DepthFrameSource.FrameDescription;

            colorBitmap = new WriteableBitmap(depthFrameDescription.Width, depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

            depthPixels = new ushort[kinect.DepthFrameSource.FrameDescription.LengthInPixels];
            colorSpacePoints = new ColorSpacePoint[depthPixels.Length];
            bodyIndexPixels = new byte[kinect.BodyIndexFrameSource.FrameDescription.LengthInPixels];
            colorPixels = new byte[colorFrameDescription.LengthInPixels * colorFrameDescription.BytesPerPixel];
            bitmapPixels = new byte[colorPixels.Length];

            multiSourceFrameReader.MultiSourceFrameArrived += MultiSourceFrameReader_MultiSourceFrameArrived;

            coordinateMapper = kinect.CoordinateMapper;

            kinect.Open();

            DataContext = this;

            InitializeComponent();
        }

        private void MultiSourceFrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            Array.Clear(bitmapPixels, 0, bitmapPixels.Length);
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            using (DepthFrame depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    depthFrame.CopyFrameDataToArray(depthPixels);
                    coordinateMapper.MapDepthFrameToColorSpace(depthPixels, colorSpacePoints);
                }
            }

            using (BodyIndexFrame bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame())
            {
                if (bodyIndexFrame != null)
                {
                    bodyIndexFrame.CopyFrameDataToArray(bodyIndexPixels);
                }
            }

            using (ColorFrame colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    colorFrame.CopyConvertedFrameDataToArray(colorPixels, ColorImageFormat.Bgra);
                    for (int y = 0; y < depthFrameDescription.Height; y++)
                    {
                        for (int x = 0; x < depthFrameDescription.Width; x++)
                        {
                            int depthIndex = (y * depthFrameDescription.Width) + x;

                            byte player = bodyIndexPixels[depthIndex];

                            if (player != 255)
                            {
                                ColorSpacePoint colorPoint = colorSpacePoints[depthIndex];

                                int colorX = (int)System.Math.Floor(colorPoint.X);
                                int colorY = (int)System.Math.Floor(colorPoint.Y);

                                if ((colorX >= 0) && (colorX < 1920) && (colorY >= 0) && (colorY < 1080))
                                {
                                    int colorIndex = ((colorY * 1920) + colorX) * 4;
                                    int displayIndex = depthIndex * 4;

                                    bitmapPixels[displayIndex + 0] = colorPixels[colorIndex];
                                    bitmapPixels[displayIndex + 1] = colorPixels[colorIndex + 1];
                                    bitmapPixels[displayIndex + 2] = colorPixels[colorIndex + 2];
                                    bitmapPixels[displayIndex + 3] = 255;
                                }
                            }
                        }
                    }
                    colorBitmap.WritePixels(new Int32Rect(0, 0, depthFrameDescription.Width,
                        depthFrameDescription.Height),
                        bitmapPixels, depthFrameDescription.Width * 4, 0);
                }
            }
        }

        public ImageSource ImageSource
        {
            get
            {
                return colorBitmap;
            }
        }
    }
}