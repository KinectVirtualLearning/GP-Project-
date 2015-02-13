using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using GestureRecognizer;
using System.Timers;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Threading;
using System.Windows.Threading;
using System.IO;
//using Microsoft.Office.Core;

namespace Real_Final_Project
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        #region "Variable Initialization"
        
        GestureRecognitionEngine recognitionEngine;
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"D:/Gerture_Main_Form/Clapping_Project/clap.wav");

        /// Bitmap that will hold color information
        private WriteableBitmap colorBitmap;
        /// Intermediate storage for the color data received from the camera
        private byte[] colorPixels;


    //    private const float RenderWidth = 466.0f;
     //   private const float RenderHeight = 417.0f;


    //    private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
     //   private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);


    //    private const double JointThickness = 3;

     //   private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
     //   private readonly Brush inferredJointBrush = Brushes.Yellow;


        // Screen to draw on it 

        // here to create image and Draw on it ;) 
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;

        // variables
        KinectSensor sensor;
        Skeleton[] TotalSkeletons = new Skeleton[6];
        List<Button> buttons;
        static Button selected;

        float handX;
        float handY;
        
        
        // positon variables
       // Timer positionTimer;
        Skeleton mySkeleton;
       // float oldRFoot = 0.0f;

        // waking fast variables
       // Timer speedTimer;
        //int counter = 0;
        //float oldRFootSpeed = 0.0f;

        //leaning timer
        //Timer leaningTimer;
        //private const float GAP = 0.05f;

        // Gesture Repetition
        //Timer gestureRepetition;
        //Timer EvSec;
        //List<double> list = new List<double>();
        //double oldRX = 0.0, oldLX = 0.0, oldRY = 0.0, oldLY = 0.0;

        bool flag = false; // to enable the timers
          #endregion
        private int counter_sec = 0;
        private int filler_counter = 0;
        private void OnTickFiller(object source, ElapsedEventArgs e)
        {
           

            counter_sec++;
            if (counter_sec == 10)
            {
               // MessageBox.Show(filler_counter.ToString());
                counter_sec = 0;
                if (filler_counter >= 3)
                {
                    MessageBox.Show("kda keteeeeeeeeeeeeeeeeeeer");
                    filler_counter = 0;
                }
            }

        }


        public MainMenu()
        {
            
            InitializeComponent();
            
            this.WindowHeight = (double)WindowState.Maximized;
            this.WindowWidth = (double)WindowState.Maximized;
            InitializeButtons();
            Generics.ResetHandPosition(kinectButton);
            kinectButton.Click += new RoutedEventHandler(kinectButton_Click);
            // Timer To check Position
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
           // dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();

            
        }
      
        //initialize buttons to be checked
        private void InitializeButtons()
        {
            buttons = new List<Button> {start,tips,quit,settings};
        }


        private void Main_Loaded(object sender, RoutedEventArgs e)
        {

            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);
            //Image.Source = this.imageSource;

            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.sensor = KinectSensor.KinectSensors[0];

                if (sensor.Status == KinectStatus.Connected)
                {

                    // Turn on the color stream to receive color frames
                    sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                    // Allocate space to put the pixels we'll receive
                    colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                    // This is the bitmap we'll display on-screen
                    colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.Image2.Source = this.colorBitmap;

                    // Set the image we display to point to the bitmap where we'll put the image data
           
                    this.sensor.ColorFrameReady += this.SensorColorFrameReady;


                    sensor.SkeletonStream.Enable();
                    sensor.SkeletonFrameReady += Skeleton_Frame_Ready;
                    this.speechRecognizer = this.CreateSpeechRecognizer();                      //speeeeech
                    sensor.Start();

                    if (this.speechRecognizer != null && sensor != null)
                    {
                        // NOTE: Need to wait 4 seconds for device to be ready to stream audio right after initialization
                        this.readyTimer = new DispatcherTimer();
                        this.readyTimer.Tick += this.ReadyTimerTick;
                        this.readyTimer.Interval = new TimeSpan(0, 0, 4);
                        this.readyTimer.Start();

                        this.ReportSpeechStatus("Initializing audio stream...");
                        this.UpdateInstructionsText(string.Empty);

                        //  this.Closing += this.MainWindowClosing;

                    }
                    Filler_timer = new System.Timers.Timer(1 * 1000);
                    Filler_timer.Elapsed += OnTickFiller;


                }
            }
            // here 
            recognitionEngine = new GestureRecognitionEngine();
            recognitionEngine.GestureRecognized += new EventHandler<GestureEventArgs>(recognitionEngine_GestureRecognized);
        }
        void recognitionEngine_GestureRecognized(object sender, GestureEventArgs e)
        {
          
        }

        private void UnregisterEvents()
        {
            
            
            this.sensor.SkeletonFrameReady -= Skeleton_Frame_Ready;
            this.sensor.ColorFrameReady -= SensorColorFrameReady;
            
        }


        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }
    


        void Skeleton_Frame_Ready(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {

                if (skeletonFrame == null)
                    return;

                skeletonFrame.CopySkeletonDataTo(TotalSkeletons);
                Skeleton firstSkeleton = (from trackskeleton in TotalSkeletons
                                          where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                          select trackskeleton).FirstOrDefault();


                if (firstSkeleton == null)
                {
                    kinectButton.Visibility = Visibility.Collapsed;
                    return;
                }
                // here
                recognitionEngine.Skeleton = firstSkeleton;
                mySkeleton = firstSkeleton;
                recognitionEngine.StartRecognize();
                Joint primaryHand = GetPrimaryHand(firstSkeleton);
                TrackHand(primaryHand);
                
            }
            if (!flag)
            {
                flag = true;
                //   speedTimer.Start();
                //    positionTimer.Start();
                //leaningTimer.Start();
                //    gestureRepetition.Start();
                //    EvSec.Start();
                //     OpenSlide();
                Filler_timer.Start();                   //speeeeeech
            }
              


        }

        //track and display hand
        private void TrackHand(Joint hand)
        {
            if (hand.TrackingState == JointTrackingState.NotTracked)
            {
                kinectButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                /*
                Screen.PrimaryScreen.Bounds.Width;
                Screen.PrimaryScreen.Bounds.Height;
                Screen.PrimaryScreen.Bounds.Size;
                */

                kinectButton.Visibility = System.Windows.Visibility.Visible;

                DepthImagePoint point = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(mySkeleton.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint Spoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(mySkeleton.Joints[JointType.ShoulderRight].Position, DepthImageFormat.Resolution640x480Fps30);

                double diffX = (double)(point.X - Spoint.X) / 640;
                double diffY = (double)(point.Y - Spoint.Y) / 480;

                handX = (int)(2 * this.WindowWidth * diffX + this.WindowWidth / 2);
                handY = (int)(2 * this.WindowHeight * diffY + this.WindowHeight / 2);

                
                Canvas.SetLeft(kinectButton, handX);
                Canvas.SetTop(kinectButton, handY);

                if (isHandOver(kinectButton, buttons)) kinectButton.Hovering();
                else kinectButton.Release();
                if (hand.JointType == JointType.HandRight)
                {
                    kinectButton.ImageSource = "/Images/myhand.png";
                }
                else
                {
                    kinectButton.ImageSource = "/Images/myhand.png";
                }
            }
        }

        
        //get the hand closest to the Kinect sensor
        private static Joint GetPrimaryHand(Skeleton skeleton)
        {
            Joint primaryHand = new Joint();
            if (skeleton != null)
            {
                primaryHand = skeleton.Joints[JointType.HandRight];
                Joint leftHand = skeleton.Joints[JointType.HandLeft];
                if (leftHand.TrackingState != JointTrackingState.NotTracked)
                {
                    if (primaryHand.TrackingState == JointTrackingState.NotTracked)
                    {
                        primaryHand = leftHand;
                    }
                    else
                    {
                        if (primaryHand.Position.Z > leftHand.Position.Z)
                        {
                            primaryHand = leftHand;
                        }
                    }
                }
            }
            return primaryHand;
        }
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
        //detect if hand is overlapping over any button
        private bool isHandOver(FrameworkElement hand, List<Button> buttonslist)
        {
            var handTopLeft = new Point(Canvas.GetLeft(hand), Canvas.GetTop(hand));
            var handX = handTopLeft.X + hand.ActualWidth / 2;
            var handY = handTopLeft.Y + hand.ActualHeight / 2;

            foreach (Button target in buttonslist)
            {

                if (target != null)
                {
                    Point targetTopLeft = new Point(Canvas.GetLeft(target), Canvas.GetTop(target));
                    if (handX > targetTopLeft.X &&
                        handX < targetTopLeft.X + target.Width &&
                        handY > targetTopLeft.Y &&
                        handY < targetTopLeft.Y + target.Height)
                    {
                        selected = target;
                        return true;
                    }
                }
            }
            return false;
        }


        void kinectButton_Click(object sender, RoutedEventArgs e)
        {
            selected.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, selected));
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            UnregisterEvents();
            (Application.Current.MainWindow.FindName("mainFrame") as Frame).Source = new Uri("MainPresentation.xaml", UriKind.Relative);
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this.WindowHeight.ToString() + "    " + this.WindowWidth.ToString());
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        //speech functions--------------------------------------------------------------------------------------------------------
        private DispatcherTimer readyTimer;
        private SpeechRecognitionEngine speechRecognizer;
        private EnergyCalculatingPassThroughStream stream;
        System.Timers.Timer Filler_timer;
        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private void ReadyTimerTick(object sender, EventArgs e)
        {
            this.Start();
            // this.ReportSpeechStatus("Ready to recognize speech!");
            //this.UpdateInstructionsText("Say: 'red', 'green' or 'blue'");
            this.readyTimer.Stop();
            this.readyTimer = null;
        }
        private void Start()
        {
            var audioSource = this.sensor.AudioSource;
            audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
            var kinectStream = audioSource.Start();
            this.stream = new EnergyCalculatingPassThroughStream(kinectStream);

            this.speechRecognizer.SetInputToAudioStream(
                this.stream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

            this.speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
            var t = new Thread(this.PollSoundSourceLocalization);
            t.Start();
        }
        private bool running = true;
        private const double AngleChangeSmoothingFactor = 0.35;
        private double angle;
        private const int WaveImageWidth = 500;
        private const int WaveImageHeight = 100;
        private readonly double[] energyBuffer = new double[WaveImageWidth];
        private readonly WriteableBitmap bitmapWave;
        private readonly byte[] blackPixels = new byte[WaveImageWidth * WaveImageHeight];
        private readonly Int32Rect fullImageRect = new Int32Rect(0, 0, WaveImageWidth, WaveImageHeight);
        private readonly byte[] pixels;

        private void PollSoundSourceLocalization()
        {
            while (this.running)
            {
                var audioSource = this.sensor.AudioSource;
                if (audioSource.SoundSourceAngleConfidence > 0.5)
                {
                    // Smooth the change in angle
                    double a = AngleChangeSmoothingFactor * audioSource.SoundSourceAngleConfidence;
                    this.angle = ((1 - a) * this.angle) + (a * audioSource.SoundSourceAngle);

                    // Dispatcher.BeginInvoke(new Action(() => { rotTx.Angle = -angle; }), DispatcherPriority.Normal);
                }

                Dispatcher.BeginInvoke(
                    new Action(
                        () =>
                        {
                            //clipConf.Rect = new Rect(
                            //    0, 0, 100 + (600 * audioSource.SoundSourceAngleConfidence), 50);
                            //string sConf = string.Format("Conf: {0:0.00}", audioSource.SoundSourceAngleConfidence);
                            //tbConf.Text = sConf;

                            stream.GetEnergy(energyBuffer);
                            //this.bitmapWave.WritePixels(fullImageRect, blackPixels, WaveImageWidth, 0);

                            for (int i = 1; i < energyBuffer.Length; i++)
                            {
                                int energy = (int)(energyBuffer[i] * 5);
                                Int32Rect r = new Int32Rect(i, (WaveImageHeight / 2) - energy, 1, 2 * energy);
                                //  this.bitmapWave.WritePixels(r, pixels, 1, 0);
                            }
                        }),
                    DispatcherPriority.Normal);

                Thread.Sleep(50);
            }
        }
        private class EnergyCalculatingPassThroughStream : Stream
        {
            private const int SamplesPerPixel = 10;

            private readonly double[] energy = new double[WaveImageWidth];
            private readonly object syncRoot = new object();
            private readonly Stream baseStream;

            private int index;
            private int sampleCount;
            private double avgSample;

            public EnergyCalculatingPassThroughStream(Stream stream)
            {
                this.baseStream = stream;
            }

            public override long Length
            {
                get { return this.baseStream.Length; }
            }

            public override long Position
            {
                get { return this.baseStream.Position; }
                set { this.baseStream.Position = value; }
            }

            public override bool CanRead
            {
                get { return this.baseStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return this.baseStream.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return this.baseStream.CanWrite; }
            }

            public override void Flush()
            {
                this.baseStream.Flush();
            }

            public void GetEnergy(double[] energyBuffer)
            {
                lock (this.syncRoot)
                {
                    int energyIndex = this.index;
                    for (int i = 0; i < this.energy.Length; i++)
                    {
                        energyBuffer[i] = this.energy[energyIndex];
                        energyIndex++;
                        if (energyIndex >= this.energy.Length)
                        {
                            energyIndex = 0;
                        }
                    }
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int retVal = this.baseStream.Read(buffer, offset, count);
                const double A = 0.3;
                lock (this.syncRoot)
                {
                    for (int i = 0; i < retVal; i += 2)
                    {
                        short sample = BitConverter.ToInt16(buffer, i + offset);
                        this.avgSample += sample * sample;
                        this.sampleCount++;

                        if (this.sampleCount == SamplesPerPixel)
                        {
                            this.avgSample /= SamplesPerPixel;

                            this.energy[this.index] = .2 + ((this.avgSample * 11) / (int.MaxValue / 2));
                            this.energy[this.index] = this.energy[this.index] > 10 ? 10 : this.energy[this.index];

                            if (this.index > 0)
                            {
                                this.energy[this.index] = (this.energy[this.index] * A) + ((1 - A) * this.energy[this.index - 1]);
                            }

                            this.index++;
                            if (this.index >= this.energy.Length)
                            {
                                this.index = 0;
                            }

                            this.avgSample = 0;
                            this.sampleCount = 0;
                        }
                    }
                }

                return retVal;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.baseStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                this.baseStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this.baseStream.Write(buffer, offset, count);
            }
        }


        private void UninitializeKinect()
        {
            var sensor = this.sensor;
            this.running = false;
            if (this.speechRecognizer != null && sensor != null)
            {
                sensor.AudioSource.Stop();
                sensor.Stop();
                this.speechRecognizer.RecognizeAsyncCancel();
                this.speechRecognizer.RecognizeAsyncStop();
            }

            if (this.readyTimer != null)
            {
                this.readyTimer.Stop();
                this.readyTimer = null;
            }
        }

        private SpeechRecognitionEngine CreateSpeechRecognizer()
        {





            RecognizerInfo ri = GetKinectRecognizer();


            if (ri == null)
            {
                MessageBox.Show(
                    @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed.",
                    "Failed to load Speech SDK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                //     this.Close();
                return null;
            }

            SpeechRecognitionEngine sre;
            try
            {
                sre = new SpeechRecognitionEngine(ri.Id);
            }
            catch
            {
                MessageBox.Show(
                    @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed and configured.",
                    "Failed to load Speech SDK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                //    this.Close();
                return null;
            }

            var grammar = new Choices();
            grammar.Add("ok");
            grammar.Add("start");


            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(grammar);

            // Create the actual Grammar instance, and then load it into the speech recognizer.
            var g = new Grammar(gb);

            sre.LoadGrammar(g);
            sre.SpeechRecognized += this.SreSpeechRecognized;
            sre.SpeechHypothesized += this.SreSpeechHypothesized;
            sre.SpeechRecognitionRejected += this.SreSpeechRecognitionRejected;

            return sre;
        }
        private void RejectSpeech(Microsoft.Speech.Recognition.RecognitionResult result)
        {
            string status = "Rejected: " + (result == null ? string.Empty : result.Text + " " + result.Confidence);
            this.ReportSpeechStatus(status);

            //Dispatcher.BeginInvoke(new Action(() => { tbColor.Background = blackBrush; }), DispatcherPriority.Normal);
        }

        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            this.RejectSpeech(e.Result);
        }

        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            this.ReportSpeechStatus("Hypothesized: " + e.Result.Text + " " + e.Result.Confidence);
        }

        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if ((e.Result.Text.ToLower() == "ok") && e.Result.Confidence >= 0.5)
            {
                filler_counter++;
                MessageBox.Show("ok");

            }
            if ((e.Result.Text.ToLower() == "start") && e.Result.Confidence >= 0.5)
            {
                filler_counter++;
                MessageBox.Show("start");
                //UnregisterEvents();
               // (Application.Current.MainWindow.FindName("mainFrame") as Frame).Source = new Uri("MainPresentation.xaml", UriKind.Relative);

            }
            string status = "Recognized: " + e.Result.Text + " " + e.Result.Confidence;
            this.ReportSpeechStatus(status);

            //Dispatcher.BeginInvoke(new Action(() => { tbColor.Background = brush; }), DispatcherPriority.Normal);
        }

        private void ReportSpeechStatus(string status)
        {
            // Dispatcher.BeginInvoke(new Action(() => { tbSpeechStatus.Text = status; }), DispatcherPriority.Normal);
        }
        private void UpdateInstructionsText(string instructions)
        {
            //Dispatcher.BeginInvoke(new Action(() => { tbColor.Text = instructions; }), DispatcherPriority.Normal);
        }
        //speechend -----------------------------------------------------------------------------------------------------------

    }
}
