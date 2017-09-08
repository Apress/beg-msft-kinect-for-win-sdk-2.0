using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace BodyIndex
{

    public partial class MainWindow : Window
    {
        KinectSensor kinect = null;
        BodyIndexFrameReader bodyIndexFrameReader = null;

        WriteableBitmap bodyIndexBitmap = null;
        byte[] bodyIndexPixels = null;
        byte[] bitmapPixels = null;
        Color[] bodyIndexColors = null;


        public MainWindow()
        {
            kinect = KinectSensor.GetDefault();
            bodyIndexFrameReader = kinect.BodyIndexFrameSource.OpenReader();

            bodyIndexFrameReader.FrameArrived += Reader_BodyIndexFrameArrived;

            FrameDescription bodyIndexFrameDescription = kinect.BodyIndexFrameSource.FrameDescription;

            bodyIndexBitmap = new WriteableBitmap(bodyIndexFrameDescription.Width, bodyIndexFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            bodyIndexPixels = new byte[bodyIndexFrameDescription.LengthInPixels];
            bitmapPixels = new byte[bodyIndexFrameDescription.Width * bodyIndexFrameDescription.Height * 4];

            Color[] bodyIndexColors = {
                Colors.Red,
                Colors.Blue,
                Colors.Green,
                Colors.Yellow,
                Colors.Purple,
                Colors.Orange
            };

            kinect.Open();

            DataContext = this;
        }

        private void Reader_BodyIndexFrameArrived(object sender, BodyIndexFrameArrivedEventArgs e)
        {
            using (BodyIndexFrame bodyIndexFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyIndexFrame != null)
                {
                    bodyIndexFrame.CopyFrameDataToArray(bodyIndexPixels);
                }

                for (int i = 0; i < bodyIndexPixels.Length; ++i)
                {
                    if (bodyIndexPixels[i] != 255)
                    {
                        var color = bodyIndexColors[bodyIndexPixels[i]];
                        bitmapPixels[i * 4 + 0] = color.B;
                        bitmapPixels[i * 4 + 1] = color.G;
                        bitmapPixels[i * 4 + 2] = color.R;
                        bitmapPixels[i * 4 + 3] = 255;
                    }
                    else
                    {
                        bitmapPixels[i * 4 + 0] = 0;
                        bitmapPixels[i * 4 + 1] = 0;
                        bitmapPixels[i * 4 + 2] = 0;
                        bitmapPixels[i * 4 + 3] = 255;
                    }
                }

                bodyIndexBitmap.WritePixels(new Int32Rect(0, 0,
                    bodyIndexBitmap.PixelWidth,
                    bodyIndexBitmap.PixelHeight),
                    bitmapPixels,
                    bodyIndexBitmap.PixelWidth * 4, 0);
            }
        }
    }
}
