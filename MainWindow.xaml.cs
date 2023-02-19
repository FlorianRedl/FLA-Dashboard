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
using FLAD.Models;
using System.Windows.Threading;
using System.Management;
using System.Windows.Media;
using System.Reflection;
using System.Threading;
using WindowsInput.Native;
using WindowsInput;
using System.Windows.Media.Animation;

namespace FLAD;


public enum FocusPoint { Stopuhr, Ziehen, Analyse, Audio, SingleMode };

public partial class MainWindow : Window
{
    public static MainWindow instance;

    Stopwatch stopwatch = new();
    List<Run> runs = new();
    SerialPort activePort;
    InputSimulator keySim = new();
    public enum Status { Idle, AudioPlaying, StopTime, DisplayLastTime };
    Status activeStatus;

    FocusPoint activeFocus;

    public event Action OnPressStart;
    public event Action OnPressStop;
    public event Action OnPressNext;
    public event Action OnDeviceCheckOk;
    private bool isDeviceCheckOk = false;

    private MediaPlayer mediaPlayer = new MediaPlayer();

    //Settings
    public bool IsAudioEnabled { get; set; }
    public bool IsSingleMode { get; set; }
    public bool IsVideoEnabled { get; set; }
    public bool IsZiehenEdit { get; set; }

    //Save Settings
    public string ComPort { get; set; }

    public MainWindow()
    {
        instance = this;
        InitializeComponent();
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(2);
        tbVersion.Text = $"Version {version}";
        activeStatus = Status.Idle;

        ComPort = Properties.Settings.Default.ComPort;

        //Set last Focus
        OnPressStart += MainWindow_OnPressStart;
        OnPressStop += MainWindow_OnPressStop;
        OnPressNext += MainWindow_OnPressNext;
        OnDeviceCheckOk += MainWindow_OnDeviceCheckOk;
        //Media
        mediaPlayer.Open(new Uri("Assets/Angriffsbefehl_LFLB.mp3", UriKind.Relative));
        mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;


        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
        timer.Interval = TimeSpan.FromMilliseconds(25);
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
            case FocusPoint.Stopuhr:
                if (activeStatus == Status.StopTime && IsSingleMode)
                {
                    StopStopwatch();
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
                    //DateTime last = DateTime.Today.AddDays(- 6);
                    //var t = runs.Where(s => s.StartTime >= last).OrderByDescending(x => x.StartTime).ToList();
                    dgRuns.ItemsSource = runs.OrderByDescending(x => x.StartTime).ToList();
                    dgRuns.ScrollIntoView(runs.LastOrDefault());
                    stopwatch.Reset();
                    activeStatus = Status.Idle;
                }

                break;
            case FocusPoint.Ziehen:
                ZiehenPage.instance.GetPosition();
                break;

            case FocusPoint.Analyse:
                break;
            case FocusPoint.Audio:
                //cbAudio.IsChecked = !IsAudioEnabled;
                break;
            case FocusPoint.SingleMode:
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
            if (IsVideoEnabled)
            {
                keySim.Keyboard.KeyDown(VirtualKeyCode.VK_B).Sleep(25).KeyUp(VirtualKeyCode.VK_B);
            }
            mediaPlayer.Stop();
            activeStatus = Status.Idle;
            return;
        }
        if (activeStatus == Status.StopTime)
        {
            StopStopwatch();

        }


    }
    private void MainWindow_OnPressNext()
    {
        NextFocus();
    }
    private void MainWindow_OnDeviceCheckOk()
    {
        //Debug.WriteLine("MainWindow_OnDeviceCheckOk: " + Thread.CurrentThread.ManagedThreadId);
        isDeviceCheckOk = true;
        Dispatcher.Invoke(() => { lbStatus.Text = "verbunden"; });
    }


    private void PlayAudio()
    {
        if (IsVideoEnabled)
        {
            keySim.Keyboard.KeyDown(VirtualKeyCode.VK_A).Sleep(25).KeyUp(VirtualKeyCode.VK_A);
        }
        activeStatus = Status.AudioPlaying;
        mediaPlayer.Stop();
        mediaPlayer.Play();
    }

    private void StartStopwatch()
    {
        activeStatus = Status.StopTime;
        if (stopwatch.IsRunning) stopwatch.Stop();
        stopwatch.Reset();
        stopwatch.Start();
    }
    private void StopStopwatch()
    {
        activeStatus = Status.DisplayLastTime;
        stopwatch.Stop();
        if (IsVideoEnabled)
        {
            keySim.Keyboard.KeyDown(VirtualKeyCode.VK_B).Sleep(25).KeyUp(VirtualKeyCode.VK_B);
        }
    }


    private void timer_Tick(object sender, EventArgs e)
    {
        TimeSpan timeTaken = stopwatch.Elapsed;
        string foo = timeTaken.ToString(@"m\:ss\.ff");
        lbMainTime.Text = foo;

        //Audio
        if (mediaPlayer.Source != null && activeStatus == Status.AudioPlaying)
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
        
        Thread.Sleep(25);
        var port = sender as SerialPort;
        var inputString = port.ReadLine().Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

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
            case "CheckOk":
                //Debug.WriteLine("MyPort_DataReceived: " + Thread.CurrentThread.ManagedThreadId);
                OnDeviceCheckOk?.Invoke();
                break;
            default:
                break;
        }
    }
    private void MyPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }


    private void cbComPorts_DropDownClosed(object sender, EventArgs e)
    {

        var selectedItem = (sender as ComboBox)?.SelectedItem;

        if (selectedItem == null) return;

        ConnectComPort(selectedItem.ToString());
        Properties.Settings.Default.ComPort = selectedItem.ToString();
        Properties.Settings.Default.Save();


    }

    private void SetupComboboxComPorts()
    {
        cbComPorts.ItemsSource = SerialPort.GetPortNames().ToList();
        var lastComPort = Properties.Settings.Default.ComPort;
        if (string.IsNullOrEmpty(lastComPort)) return;
        cbComPorts.SelectedItem = lastComPort;
        ConnectComPort(lastComPort);

    }
    private async void ConnectComPort(string? portName)
    {
        Debug.WriteLine("ConnectComPort: " + Thread.CurrentThread.ManagedThreadId);
        if (portName == null) return;
        isDeviceCheckOk = false;
        lbStatus.Text = "";
        if (activePort == null)
        {
            activePort = new SerialPort();
            activePort.BaudRate = 9600;
            activePort.ReadTimeout = 500;
            activePort.WriteTimeout = 500;
            activePort.DataReceived += MyPort_DataReceived;
            activePort.ErrorReceived += MyPort_ErrorReceived;
            activePort.PinChanged += (object sender, SerialPinChangedEventArgs e)=> { };
            activePort.Disposed += (object? sender, EventArgs e)=> { };
        }
        try
        {
            lbStatus.Text = "...";
            if (activePort.IsOpen) activePort.Close();
            activePort.PortName = portName;
           
            activePort.Open();
            activePort.WriteLine("CheckDevice");
            
            await CheckConnection();
            return;
        }
        catch (Exception ex)
        {
            lbStatus.Text = "fehler!";
            lbStatus.ToolTip = ex.Message;
            return;
        }


    }

   

    private async Task CheckConnection()
    {
        await Task.Delay(2000);
        //Debug.WriteLine("TASK: " + Thread.CurrentThread.ManagedThreadId);
        if (isDeviceCheckOk) return;
        
        lbStatus.Text = "fehler!";
        lbStatus.ToolTip = "falsches Gerät";

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
        cbVideo.IsEnabled = true;
    }
    private void cbTon_Unchecked(object sender, RoutedEventArgs e)
    {
        IsAudioEnabled = false;
        cbVideo.IsEnabled = false;
        cbVideo.IsChecked = false;
    }
    private void cbSingleMode_Checked(object sender, RoutedEventArgs e)
    {
        IsSingleMode = true;
        
    }

    private void cbSingleMode_Unchecked(object sender, RoutedEventArgs e)
    {
        IsSingleMode = false;
    }
    private void cbVideo_Checked(object sender, RoutedEventArgs e)
    {
        IsVideoEnabled = true;
    }

    private void cbVideo_Unchecked(object sender, RoutedEventArgs e)
    {
        IsVideoEnabled = false;
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
        btStopuhr.Focus();
        SetupComboboxComPorts();
        DataLoader.LoadAllRuns(ref runs);
        dgRuns.ItemsSource = runs.OrderByDescending(x => x.StartTime).ToList();

    }
    private void Window_Closed(object sender, EventArgs e)
    {
        DataLoader.SaveAllRuns(ref runs);

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
    private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        var button = e.NewFocus as Button;
        var checkbox = e.NewFocus as CheckBox;
        var compareListe = new Dictionary<string, FocusPoint>
        {
            { "btStopuhr", FocusPoint.Stopuhr },
            { "btZiehen", FocusPoint.Ziehen },
            { "btAnalyse", FocusPoint.Analyse },
            { "cbAudio", FocusPoint.Audio },
            { "cbSingleMode", FocusPoint.SingleMode }
        };
        if (button != null) SetFocus(compareListe.Where(s => s.Key == button.Name).FirstOrDefault().Value);
        if (checkbox != null) SetFocus(compareListe.Where(s => s.Key == checkbox.Name).FirstOrDefault().Value);
    }


    public void SetFocus(FocusPoint focus)
    {
        activeFocus = focus;
        SetSubFrame(focus);
      

    }

    private void SetSubFrame(FocusPoint focus)
    {
        switch (focus)
        {
            case FocusPoint.Stopuhr:
                SubFrame.Navigate(new Uri("ZiehenPage.xaml", UriKind.Relative));
                break;
            case FocusPoint.Ziehen:
                SubFrame.Navigate(new Uri("ZiehenPage.xaml", UriKind.Relative));
                break;
            case FocusPoint.Analyse:
                SubFrame.Navigate(new Uri("AnalysePage.xaml", UriKind.Relative));
                break;
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

    
}


