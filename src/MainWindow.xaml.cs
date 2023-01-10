using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;

namespace AudioKeepAlive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        record AudioDevice(int Id, string Name);
        Timer _timer = new Timer(); 
        AudioDevice? _selected = null;
        bool _isPlaying = false;
        CachedSound _sound = new CachedSound("Assets/beep.wav");

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _timer.Elapsed += Timer_Elapsed;
            ShowDevices();
        }

        private void ShowDevices()
        {
            var devices = new List<AudioDevice>();
            for (var n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                devices.Add(new AudioDevice(n, caps.ProductName));
            }

            lbDevices.ItemsSource = devices;
        }

        private void lbDevices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var device = lbDevices.SelectedItem as AudioDevice;

            if (device != null)
            {
                tbSelectedDevice.Text = device.Name;
                _selected = device;
            }
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (_selected != null)
            {
                if(_isPlaying)
                {
                    _isPlaying = false;
                    _timer.Stop();
                    btnStartStop.Content = "Start play";
                }
                else
                {
                    _isPlaying = true;
                    _timer.Interval = Convert.ToInt32(tbInterval.Text);
                    _timer.Start();
                    btnStartStop.Content = "Stop play";
                    Timer_Elapsed(this, null);
                }
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (_isPlaying && _selected != null)
            {
                AudioPlaybackEngine.Instance(_selected.Id).PlaySound(_sound);
            }
        }
    }
}
