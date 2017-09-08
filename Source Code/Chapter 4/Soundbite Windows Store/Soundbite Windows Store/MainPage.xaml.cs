using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowsPreview.Kinect;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Soundbite_Windows_Store
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private KinectSensor kinect = null;
        private AudioBeamFrameReader audioBeamFrameReader = null;
        private readonly byte[] audioBuffer = null;
        private bool isRecording = false;
        private int size = 0;
        private StorageFile storageFile;
        Stream stream;
        public MainPage()
        {
            this.kinect = KinectSensor.GetDefault();
            this.kinect.Open();

            AudioSource audioSource = this.kinect.AudioSource;

            this.audioBuffer = new byte[audioSource.SubFrameLengthInBytes];

            this.audioBeamFrameReader = audioSource.OpenReader();
            this.audioBeamFrameReader.FrameArrived += Reader_FrameArrived;

            this.audioBeamFrameReader.IsPaused = true;
            this.InitializeComponent();
        }

        private void Reader_FrameArrived(AudioBeamFrameReader sender, AudioBeamFrameArrivedEventArgs e)
        {
            using (var audioFrame = e.FrameReference.AcquireBeamFrames() as AudioBeamFrameList)
            {
                if (audioFrame == null)
                {
                    return;
                }

                for (int i = 0; i < audioFrame.Count; i++) {
                    using (var frame = audioFrame[i])
                    {
                        for (int j = 0; j < frame.SubFrames.Count; j++)
                        {
                            using (var subFrame = frame.SubFrames[j])
                            {
                                subFrame.CopyFrameDataToArray(this.audioBuffer);
                                stream.Write(audioBuffer, 0, audioBuffer.Length);
                                size += audioBuffer.Length;
                            }
                        }
                    }
                } 
            }
            //AudioBeamFrameList frameList = (AudioBeamFrameList)e.FrameReference.AcquireBeamFrames();

            //if (frameList != null)
            //{
            //    //using(frameList)
            //   // {
            //        IReadOnlyList<AudioBeamSubFrame> subFrameList = frameList[0].SubFrames;

            //        // Loop over all sub frames, extract audio buffer and beam informationIReadOnlyList<AudioBeamFrame> 
            //        foreach (AudioBeamSubFrame subFrame in subFrameList)
            //        {
            //                subFrame.CopyFrameDataToArray(this.audioBuffer);

            //                stream.Write(audioBuffer, 0, audioBuffer.Length);
            //                size += audioBuffer.Length;
            //        subFrame.Dispose();
            //            }
            //    frameList.Dispose();
                
            //   // }


            //}
            
        }

        private static void WriteWavHeader(Stream stream, int size)
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
                    memStream.WriteTo(stream);
                }
            }
        }
        private static void WriteString(Stream stream, string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s); stream.Write(bytes, 0, bytes.Length);
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
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            if (isRecording == true)
            {
                button.IsEnabled = false;
                button.Content = "Record";

                audioBeamFrameReader.IsPaused = true;
                this.isRecording = false;
                // long prePos = fileStream.Position;
                stream.Seek(0, SeekOrigin.Begin);
                WriteWavHeader(stream, size);
                stream.Seek(0, SeekOrigin.End);
                await stream.FlushAsync();
                stream.Dispose();
                size = 0;
                button.IsEnabled = true;


            }

            else if (isRecording == false)
            {
                button.IsEnabled = false;
                this.isRecording = true;

                StorageFolder musicFolder = KnownFolders.MusicLibrary;

                storageFile = await musicFolder.CreateFileAsync("Kinect Audio.wav", CreationCollisionOption.GenerateUniqueName);//FileStream(fileName, FileMode.Create);
                stream = await storageFile.OpenStreamForWriteAsync();

                WriteWavHeader(stream, size);
                audioBeamFrameReader.IsPaused = false;

                button.Content = "Stop";
                button.IsEnabled = true;

            }
        }
    }
}
