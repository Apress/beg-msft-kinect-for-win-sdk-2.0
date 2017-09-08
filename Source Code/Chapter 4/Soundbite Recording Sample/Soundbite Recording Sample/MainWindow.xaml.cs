using System.Windows;
using System.IO;
using System.Collections.Generic;

using Microsoft.Kinect;
using System;
using System.Text;

namespace Soundbite_Recording_Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor kinect = null;
        private AudioBeamFrameReader audioBeamFrameReader = null;
        private readonly byte[] audioBuffer = null;
        private bool isRecording = false;
        private int size = 0;
        private FileStream fileStream;
        public MainWindow()
        {

            this.kinect = KinectSensor.GetDefault();
            this.kinect.Open();

            AudioSource audioSource = this.kinect.AudioSource;

            this.audioBuffer = new byte[audioSource.SubFrameLengthInBytes];

            this.audioBeamFrameReader = audioSource.OpenReader();
            this.audioBeamFrameReader.FrameArrived += Reader_FrameArrived;

            this.audioBeamFrameReader.IsPaused = true;

            audioSource.AudioBeams[0].AudioBeamMode = AudioBeamMode.Manual;
            audioSource.AudioBeams[0].BeamAngle = 0.80f;

            InitializeComponent();
        }

        private static void WriteWavHeader(FileStream fileStream, int size)
        {
            using (MemoryStream memStream = new MemoryStream(64))
            {
                int cbFormat = 18;
                WAVEFORMATEX format = new WAVEFORMATEX()
                {
                    wFormatTag = 3,
                    nChannels = 1,
                    nSamplesPerSec = 16000,
                    nAvgBytesPerSec = 64000,
                    nBlockAlign = 4,
                    wBitsPerSample = 32,
                    cbSize = 0
                };
                using (var bw = new BinaryWriter(memStream))
                {
                    WriteString(memStream, "RIFF");
                    bw.Write(size + cbFormat + 4);
                    WriteString(memStream, "WAVE");
                    WriteString(memStream, "fmt "); bw.Write(cbFormat);
                    bw.Write(format.wFormatTag);
                    bw.Write(format.nChannels);
                    bw.Write(format.nSamplesPerSec);
                    bw.Write(format.nAvgBytesPerSec);
                    bw.Write(format.nBlockAlign);
                    bw.Write(format.wBitsPerSample);
                    bw.Write(format.cbSize);
                    WriteString(memStream, "data");
                    bw.Write(size);
                    memStream.WriteTo(fileStream);
                }
            }
        }
        private static void WriteString(Stream stream, string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s); stream.Write(bytes, 0, bytes.Length);
        }
        private void Reader_FrameArrived(object sender, AudioBeamFrameArrivedEventArgs e)
        {

            AudioBeamFrameReference frameReference = e.FrameReference;
            AudioBeamFrameList frameList = frameReference.AcquireBeamFrames();

            if (frameList != null)
            {

                using (frameList)
                {

                    IReadOnlyList<AudioBeamSubFrame> subFrameList = frameList[0].SubFrames;

                    // Loop over all sub frames, extract audio buffer and beam information
                    foreach (AudioBeamSubFrame subFrame in subFrameList)
                    {
                        subFrame.CopyFrameDataToArray(this.audioBuffer);
                        if (fileStream.CanWrite == true)
                        {
                            fileStream.Write(audioBuffer, 0, audioBuffer.Length);
                            size += audioBuffer.Length;
                        }
                    }
                }
            }
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (isRecording == true)
            {
                button.IsEnabled = false;
                button.Content = "Record";

                audioBeamFrameReader.IsPaused = true;
                this.isRecording = false;
               // long prePos = fileStream.Position;
                fileStream.Seek(0, SeekOrigin.Begin);
                WriteWavHeader(fileStream, size);
                fileStream.Seek(0, SeekOrigin.End);
                fileStream.Flush();
                fileStream.Dispose();
                size = 0;
                button.IsEnabled = true;


            }

            else if (isRecording == false)
            {
                button.IsEnabled = false;

                this.isRecording = true;
                audioBeamFrameReader.IsPaused = false;

                string time = DateTime.Now.ToString("d MMM yyyy hh-mm-ss");
                string myMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                string fileName = Path.Combine(myMusic, "Kinect Audio-" + time + ".wav");
                fileStream = new FileStream(fileName, FileMode.Create);

                WriteWavHeader(fileStream, size);
                button.Content = "Stop";
                button.IsEnabled = true;

            }
        }
        struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;

        }

    }
}

