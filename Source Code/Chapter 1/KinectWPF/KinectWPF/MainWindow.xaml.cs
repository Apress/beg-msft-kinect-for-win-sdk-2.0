using System.Windows;

using Microsoft.Kinect;
using System.ComponentModel;

namespace KinectWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private KinectSensor kinect = null;
        private string statusText = null;

        public MainWindow()
        {
            kinect = KinectSensor.GetDefault();
            kinect.IsAvailableChanged += Sensor_IsAvailableChanged;
            kinect.Open();
            StatusText = kinect.IsAvailable ? "Hello World!" : "Goodbye World!";
            DataContext = this;

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            StatusText = kinect.IsAvailable ? "Hello World!" : "Goodbye World!";
        }

        public string StatusText
        {
            get
            {
                return statusText;
            }

            set
            {
                if (statusText != value)
                {
                    statusText = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (kinect != null)
            {
                kinect.Close();
                kinect = null;
            }
        }
    }
}
