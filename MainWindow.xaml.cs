using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using Zeitmessung.Models;
using System.Windows.Threading;
using System.Management;
using System.Windows.Media;
using System.Reflection;
using System.Media;
using System.Threading;
using System.Collections.ObjectModel;

namespace Zeitmessung
{
   

    public partial class MainWindow : Window
    {
        public static MainWindow app;

        Stopwatch stopwatch = new();
        List<Run> runs = new();
        SerialPort activePort = new();
        public enum Status { Idle, AudioPlaying, StopTime, DisplayLastTime };
        Status activeStatus;
        public enum Focus { Stopuhr, Ziehen, Analyse, Audio, SingleMode };
        Focus activeFocus;

        public event Action OnPressStart;
        public event Action OnPressStop;
        public event Action OnPressNext;

        private MediaPlayer mediaPlayer = new MediaPlayer();

        //Settings
        public bool IsAudioEnabled { get; set; }
        public bool IsSingleMode { get; set; }

        public bool IsZiehenEdit { get; set; }

        //Save Settings
        public string ComPort { get; set; }

        public MainWindow()
        {
            app = this;
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(2);
            tbVersion.Text = $"Version {version}";
            activeStatus = Status.Idle;

            ComPort = Properties.Settings.Default.ComPort;

            //Set last Focus
            OnPressStart += MainWindow_OnPressStart;
            OnPressStop += MainWindow_OnPressStop;
            OnPressNext += MainWindow_OnPressNext;
            //Media
            mediaPlayer.Open(new Uri("Assets/Angriffsbefehl_LFLB.mp3", UriKind.Relative));
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;


            List<string> list = new();
            list = SerialPort.GetPortNames().ToList();
            cbComPorts.ItemsSource = list;

            activePort.BaudRate = 9600;
            activePort.ReadTimeout = 500;
            activePort.WriteTimeout = 500;
            activePort.DataReceived += MyPort_DataReceived;
            activePort.ErrorReceived += MyPort_ErrorReceived;


            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        private void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            StartStopwatch();
        }

        private void MainWindow_OnPressStart()
        {
            switch (activeFocus)
            {
                case Focus.Stopuhr:
                    if (activeStatus == Status.StopTime && IsSingleMode)
                    {
                        activeStatus = Status.DisplayLastTime;
                        stopwatch.Stop();
                        return;
                    }
                    if (activeStatus == Status.Idle && IsAudioEnabled)
                    {
                        PlayAudio();
                    }
                    if (activeStatus == Status.Idle && !IsAudioEnabled)
                    {
                        StartStopwatch();
                    }
                    if (activeStatus == Status.DisplayLastTime)
                    {
                        runs.Add(new Run { Time = stopwatch.Elapsed, StartTime = DateTime.Now });
                        lvRuns.ItemsSource = runs.OrderByDescending(x => x.StartTime).ToList();
                        stopwatch.Reset();
                        activeStatus = Status.Idle;
                    }
                    
                    break;
                case Focus.Ziehen:
                    ZiehenPage.instance.GetPosition();
                    break;

                case Focus.Analyse:
                    break;
                case Focus.Audio:
                    //cbAudio.IsChecked = !IsAudioEnabled;
                    break;
                case Focus.SingleMode:
                    //cbSingleMode.IsChecked = !IsSingleMode;
                    break;
                default:
                    break;
            }
            
        }
        private void MainWindow_OnPressStop()
        {
            if (activeStatus == Status.AudioPlaying)
            {
                mediaPlayer.Stop();
                activeStatus = Status.Idle;
                return;
            }
            if (activeStatus == Status.StopTime)
            {
                activeStatus = Status.DisplayLastTime;
                stopwatch.Stop();
                
            }

            
        }
        private void MainWindow_OnPressNext()
        {
            NextFocus();
        }

        private void PlayAudio()
        {
            activeStatus = Status.AudioPlaying;
            mediaPlayer.Stop();
            mediaPlayer.Play();
        }

        private void StartStopwatch()
        {
            activeStatus = Status.StopTime;
            if (stopwatch.IsRunning) stopwatch.Stop();
            if (stopwatch.Elapsed.TotalSeconds > 0)
            {
                runs.Add(new Run { Time = stopwatch.Elapsed });
                lvRuns.Items.Refresh();
            }

            stopwatch.Reset();
            stopwatch.Start();
        }

       
      
        private void timer_Tick(object sender, EventArgs e)
        {
            TimeSpan timeTaken = stopwatch.Elapsed;
            string foo = timeTaken.ToString(@"m\:ss\.ff");
            lbMainTime.Text = foo;
 
            //Audio
            if (mediaPlayer.Source!= null && activeStatus == Status.AudioPlaying)
            {
                tbAudioTime.Text = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                pbAudioTime.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                pbAudioTime.Value = mediaPlayer.Position.TotalMilliseconds;
            }
            else
            {
                tbAudioTime.Text = "";
                pbAudioTime.Value = 0;
            } 
        }

        private void MyPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(50);
            var port = sender as SerialPort;
            var inputString = port.ReadLine().Replace("\r\n", "").Replace("\r", "").Replace("\n", ""); ;

            Debug.WriteLine(inputString);
            switch (inputString)
            {
                case "START":
                    Dispatcher.Invoke(() => OnPressStart?.Invoke());
                    break;
                case "STOP":
                    Dispatcher.Invoke(() => OnPressStop?.Invoke());
                    break;
                case "NEXT":
                    Dispatcher.Invoke(() => OnPressNext?.Invoke());
                    break;
                default:
                    break;
            }
        }
        


        private void cbComPorts_DropDownClosed(object sender, EventArgs e)
        { 
        
            var selectedItem = (sender as ComboBox)?.SelectedItem;
            
            if (selectedItem == null) return;
            lbStatus.Text = "";
            if (CheckComPort(selectedItem.ToString()))
            {
                lbStatus.Text = "verbunden";
                Properties.Settings.Default.ComPort = selectedItem.ToString();
                Properties.Settings.Default.Save();
            }
            else
            {
                lbStatus.Text = "fehler";
            }
            

        }

        private void MyPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private bool CheckComPort(string? portName)
        {
            
            if (portName == null) return false;
            try
            {
                if (activePort.IsOpen) activePort.Close();
                activePort.PortName = portName;
                activePort.Open();               
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OnPressStart -= MainWindow_OnPressStart;
            OnPressStop -= MainWindow_OnPressStop;
            OnPressNext -= MainWindow_OnPressNext;

            if (activePort.IsOpen) 
            {
                activePort.DataReceived -= MyPort_DataReceived;
                activePort.ErrorReceived -= MyPort_ErrorReceived;
                activePort.Close();
            } 
        }


        //Debug Buttons
        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            OnPressStart?.Invoke();

        }
        private void btStop_Click(object sender, RoutedEventArgs e)
        {

            OnPressStop?.Invoke();
        }
        private void btModusWechsel_Click(object sender, RoutedEventArgs e)
        {
            OnPressNext?.Invoke();
        }
        private void cbTon_Checked(object sender, RoutedEventArgs e)
        {
            IsAudioEnabled = true;
        }
        private void cbTon_Unchecked(object sender, RoutedEventArgs e)
        {
            IsAudioEnabled = false;
        }
        private void cbSingleMode_Checked(object sender, RoutedEventArgs e)
        {
            IsSingleMode = true;
        }

        private void cbSingleMode_Unchecked(object sender, RoutedEventArgs e)
        {
            IsSingleMode = false;
        }
        private void NextFocus()
        {
            TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
            var elementWithFocus = Keyboard.FocusedElement as UIElement;
            if (elementWithFocus != null)
            {
                elementWithFocus.MoveFocus(tRequest);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(btStopuhr);
            var comPort = Properties.Settings.Default.ComPort;
            if (!string.IsNullOrEmpty(comPort))
            {
                if (CheckComPort(comPort))
                {
                    lbStatus.Text = "verbunden";
                }
                else
                {
                    lbStatus.Text = "fehler";
                }
                cbComPorts.SelectedItem = comPort;
            }


        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var button = e.NewFocus as Button;
            var checkbox = e.NewFocus as CheckBox;

            if (button != null) SetFocus(button.Name);
            if (checkbox != null) SetFocus(checkbox.Name);
        }


        public void SetFocus(string elementName)
        {
            //var b = new BrushConverter().ConvertFromString("#EF5D6B");
            //var color = b as SolidColorBrush;

            //bAudio.Background = Brushes.Transparent;
            //bSingleMode.Background = Brushes.Transparent;

            switch (elementName)
            {
                case "btStopuhr":
                    activeFocus = Focus.Stopuhr;
                    SubFrame.Navigate(new Uri("ZiehenPage.xaml", UriKind.Relative));
                    break;
                case "btZiehen":
                    activeFocus = Focus.Ziehen;
                    SubFrame.Navigate(new Uri("ZiehenPage.xaml", UriKind.Relative));
                    break;
                case "btAnalyse":
                    activeFocus = Focus.Analyse;
                    SubFrame.Navigate(new Uri("AnalysePage.xaml", UriKind.Relative));
                    break;
                //case "cbAudio":
                //    activeFocus = Focus.Audio;
                //    bAudio.Background = color;
                //    break;
                //case "cbSingleMode":
                //    activeFocus = Focus.SingleMode;
                //    bSingleMode.Background = color;
                //    break;

            }

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.F5)
            {
                if (debugPannel.Visibility == Visibility.Visible) { debugPannel.Visibility = Visibility.Hidden; return; }
                if (debugPannel.Visibility == Visibility.Hidden) debugPannel.Visibility = Visibility.Visible;
            }
        }

        private void cbAudioShort_Checked(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Close();
            mediaPlayer.Open(new Uri("Assets/Angriffsbefehl_LFLB_kurz.mp3", UriKind.Relative));
        }

        private void cbAudioShort_Unchecked(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Close();
            mediaPlayer.Open(new Uri("Assets/Angriffsbefehl_LFLB.mp3", UriKind.Relative));
        }
    }

   

}
