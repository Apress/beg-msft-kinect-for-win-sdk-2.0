using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;

namespace ColorManipulation
{

    public sealed partial class MainPage : Page
    {

        private KinectSensor kinect = null;
        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;

        private readonly uint bytesPerPixel;
        private byte[] colorPixels = null;


        public MainPage()
        {
            kinect = KinectSensor.GetDefault();

            colorFrameReader = kinect.ColorFrameSource.OpenReader();
            colorFrameReader.FrameArrived += Reader_ColorFrameArrived;

            FrameDescription colorFrameDescription = kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);

            bytesPerPixel = colorFrameDescription.BytesPerPixel;
            colorPixels = new byte[colorFrameDescription.Width * colorFrameDescription.Height * bytesPerPixel];

            colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height);

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
            var colorFrameProcessed = false;
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    if ((colorFrameDescription.Width == colorBitmap.PixelWidth) && (colorFrameDescription.Height == colorBitmap.PixelHeight))
                    {

                        if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                        {

                            colorFrame.CopyRawFrameDataToArray(colorPixels);
                        }
                        else
                        {
                            colorFrame.CopyConvertedFrameDataToArray(colorPixels, ColorImageFormat.Bgra);
                        }

                        for (uint i = 0; i < colorPixels.Length; i += bytesPerPixel)
                        {
                            colorPixels[i] = 0x00;
                            colorPixels[i + 1] = 0x00;
                        }

                        Stream pixelStream = colorBitmap.PixelBuffer.AsStream();
                        pixelStream.Seek(0, SeekOrigin.Begin);
                        pixelStream.Write(colorPixels, 0, colorPixels.Length);

                        colorFrameProcessed = true;
                    }

                }
            }
            if (colorFrameProcessed)
            {
                colorBitmap.Invalidate();
            }
        }

        private async void Screenshot_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder picFolder = KnownFolders.PicturesLibrary;

            StorageFile picFile = await picFolder.CreateFileAsync("Kinect Screenshot.png", CreationCollisionOption.GenerateUniqueName);

            using (IRandomAccessStream stream = await picFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                Stream pixelStream = colorBitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    (uint)colorBitmap.PixelWidth,
                    (uint)colorBitmap.PixelHeight,
                    96.0, 96.0,
                    pixels);

                await encoder.FlushAsync();
            }
        }
    }
}
