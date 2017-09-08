using Microsoft.Kinect;
using Microsoft.Kinect.Face;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;

namespace Face
{
    public partial class MainWindow : Window
    {
        KinectSensor kinect;

        MultiSourceFrameReader multiSourceFrameReader;
        FrameDescription frameDescription;
        Body[] bodies;

        FaceFrameSource[] faceFrameSources;
        FaceFrameReader[] faceFrameReaders;
        FaceFrameResult[] faceFrameResults;

        private List<Brush> faceBrush;

        WriteableBitmap infraredBitmap;
        ushort[] infraredPixels;
        DrawingGroup drawingGroup;
        DrawingImage drawingImageSource;

        public MainWindow()
        {
            kinect = KinectSensor.GetDefault();

            InfraredFrameSource infraredFrameSource = kinect.InfraredFrameSource;

            multiSourceFrameReader = kinect.OpenMultiSourceFrameReader(FrameSourceTypes.Infrared | FrameSourceTypes.Body);
            multiSourceFrameReader.MultiSourceFrameArrived += MultiSource_FrameArrived;

            frameDescription = infraredFrameSource.FrameDescription;

            bodies = new Body[kinect.BodyFrameSource.BodyCount];

            FaceFrameFeatures faceFrameFeatures =
                FaceFrameFeatures.BoundingBoxInInfraredSpace
                | FaceFrameFeatures.PointsInInfraredSpace
                | FaceFrameFeatures.RotationOrientation
                | FaceFrameFeatures.FaceEngagement
                | FaceFrameFeatures.Glasses
                | FaceFrameFeatures.Happy
                | FaceFrameFeatures.LeftEyeClosed
                | FaceFrameFeatures.RightEyeClosed
                | FaceFrameFeatures.LookingAway
                | FaceFrameFeatures.MouthMoved
                | FaceFrameFeatures.MouthOpen;

            faceFrameSources = new FaceFrameSource[6];
            faceFrameReaders = new FaceFrameReader[6];
            faceFrameResults = new FaceFrameResult[6];

            for (int i = 0; i < 6; i++)
            {
                faceFrameSources[i] = new FaceFrameSource(kinect, 0, faceFrameFeatures);
                faceFrameReaders[i] = faceFrameSources[i].OpenReader();
                faceFrameReaders[i].FrameArrived += Face_FrameArrived;
            }

            faceBrush = new List<Brush>()
            {
                Brushes.Pink,
                Brushes.Orange,
                Brushes.Yellow,
                Brushes.Purple,
                Brushes.Red,
                Brushes.Blue

            };

            infraredBitmap = new WriteableBitmap(frameDescription.Width,
                 frameDescription.Height,
                 96.0, 96.0,
                 PixelFormats.Gray32Float,
                 null);
            infraredPixels = new ushort[frameDescription.LengthInPixels];
            drawingGroup = new DrawingGroup();
            drawingImageSource = new DrawingImage(drawingGroup);

            kinect.Open();

            DataContext = this;

            InitializeComponent();
        }

        public ImageSource ImageSource
        {
            get
            {
                return drawingImageSource;
            }
        }

        private void Face_FrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            using (FaceFrame faceFrame = e.FrameReference.AcquireFrame())
            {
                if (faceFrame != null)
                {
                    int index = GetFaceSourceIndex(faceFrame.FaceFrameSource);

                    faceFrameResults[index] = faceFrame.FaceFrameResult;
                }
            }
        }

        private void MultiSource_FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            using (InfraredFrame infraredFrame = multiSourceFrame.InfraredFrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    using (KinectBuffer infraredBuffer = infraredFrame.LockImageBuffer())
                    {
                        ProcessInfraredFrameData(infraredBuffer.UnderlyingBuffer, infraredBuffer.Size);
                    }
                }
            }

            using (BodyFrame bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(bodies);

                    using (DrawingContext dc = drawingGroup.Open())
                    {
                        dc.DrawImage(infraredBitmap, new Rect(0, 0,
                            infraredBitmap.Width,
                            infraredBitmap.Height));

                        for (int i = 0; i < 6; i++)
                        {
                            if (faceFrameSources[i].IsTrackingIdValid)
                            {
                                if (faceFrameResults[i] != null)
                                {
                                    DrawFace(i, faceFrameResults[i], dc);
                                }
                            }
                            else
                            {
                                if (bodies[i].IsTracked)
                                {
                                    faceFrameSources[i].TrackingId = bodies[i].TrackingId;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawFace(int index, FaceFrameResult faceFrameResult, DrawingContext drawingContext)
        {
            Brush drawingBrush = faceBrush[0];
            if (index < 6)
            {
                drawingBrush = faceBrush[index];
            }

            Pen drawingPen = new Pen(drawingBrush, 4);

            var faceBoxSource = faceFrameResult.FaceBoundingBoxInInfraredSpace;
            Rect faceBox = new Rect(faceBoxSource.Left, faceBoxSource.Top, faceBoxSource.Right - faceBoxSource.Left, faceBoxSource.Bottom - faceBoxSource.Top);
            drawingContext.DrawRectangle(null, drawingPen, faceBox);

            if (faceFrameResult.FaceBoundingBoxInInfraredSpace != null)
            {
                foreach (PointF pointF in faceFrameResult.FacePointsInInfraredSpace.Values)
                {
                    drawingContext.DrawEllipse(null, drawingPen, new Point(pointF.X, pointF.Y), 0.4, 0.4);
                }
            }

            string faceText = string.Empty;
            if (faceFrameResult.FaceProperties != null)
            {
                if (faceFrameResult.FaceProperties[FaceProperty.Happy] == DetectionResult.Yes)
                {
                    Point nosePoint = new Point(faceFrameResult.FacePointsInInfraredSpace[FacePointType.Nose].X,
                        faceFrameResult.FacePointsInInfraredSpace[FacePointType.Nose].Y);
                    drawingContext.DrawText(new FormattedText(
                        "☺",
                        System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.RightToLeft,
                        new Typeface("Segoe UI"),
                        68,
                        drawingBrush),
                        nosePoint);
                }
            }

            if (faceFrameResult.FaceProperties != null)
            {
                if (faceFrameResult.FaceProperties[FaceProperty.Happy] == DetectionResult.Yes)
                {
                    Point nosePoint = new Point(faceFrameResult.FacePointsInInfraredSpace[FacePointType.Nose].X,
                                    faceFrameResult.FacePointsInInfraredSpace[FacePointType.Nose].Y);
                    drawingContext.DrawText(new FormattedText(
                        "☺",
                        System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.RightToLeft,
                        new Typeface("Segoe UI"),
                        68,
                        drawingBrush),
                        nosePoint);
                }
            }
        }
        private static void ExtractFaceRotationInDegrees(Vector4 rotQuaternion, out int pitch, out int yaw, out int roll)
        {
            double x = rotQuaternion.X;
            double y = rotQuaternion.Y;
            double z = rotQuaternion.Z;
            double w = rotQuaternion.W;

            double yawD, pitchD, rollD;
            pitchD = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
            yawD = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
            rollD = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;

            double increment = 5.0;
            pitch = (int)(Math.Floor((pitchD + ((increment / 2.0) * (pitchD > 0 ? 1.0 : -1.0))) / increment) * increment);
            yaw = (int)(Math.Floor((yawD + ((increment / 2.0) * (yawD > 0 ? 1.0 : -1.0))) / increment) * increment);
            roll = (int)(Math.Floor((rollD + ((increment / 2.0) * (rollD > 0 ? 1.0 : -1.0))) / increment) * increment);
        }


        private unsafe void ProcessInfraredFrameData(IntPtr underlyingBuffer, uint size)
        {
            ushort* frameData = (ushort*)underlyingBuffer;

            infraredBitmap.Lock();
            float* backBuffer = (float*)infraredBitmap.BackBuffer;

            for (int i = 0; i < (int)(size / frameDescription.BytesPerPixel); ++i)
            {
                backBuffer[i] = Math.Min(1f, (((float)frameData[i] / (float)ushort.MaxValue * 0.85f) * (0.9f - 0.01f)) + 0.01f);
            }

            infraredBitmap.AddDirtyRect(new Int32Rect(0, 0, infraredBitmap.PixelWidth, infraredBitmap.PixelHeight));
            infraredBitmap.Unlock();
        }

        private int GetFaceSourceIndex(FaceFrameSource faceFrameSource)
        {
            int index = -1;

            for (int i = 0; i < 6; i++)
            {
                if (faceFrameSources[i] == faceFrameSource)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
        private bool ValidateFaceBoxAndPoints(FaceFrameResult faceFrameResult)
        {
            bool isFaceValid = faceFrameResult != null;

            if (isFaceValid)
            {
                RectI boundingBox = faceFrameResult.FaceBoundingBoxInInfraredSpace;
                if (boundingBox != null)
                {
                    isFaceValid = (boundingBox.Right - boundingBox.Left) > 0 &&
                                  (boundingBox.Bottom - boundingBox.Top) > 0 &&
                                  boundingBox.Right <= frameDescription.Width &&
                                  boundingBox.Bottom <= frameDescription.Height;

                    if (isFaceValid)
                    {
                        var facePoints = faceFrameResult.FacePointsInInfraredSpace;
                        if (facePoints != null)
                        {
                            foreach (PointF pointF in facePoints.Values)
                            {
                                bool isFacePointValid = pointF.X > 0.0f &&
                                                        pointF.Y > 0.0f &&
                                                        pointF.X < frameDescription.Width &&
                                                        pointF.Y < frameDescription.Height;

                                if (!isFacePointValid)
                                {
                                    isFaceValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return isFaceValid;
        }
    }
}
