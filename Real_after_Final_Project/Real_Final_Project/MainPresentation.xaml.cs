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
using System.IO;
using System.Speech.Synthesis;
using System.Globalization;
using Microsoft.Office.Core;
using System.Diagnostics;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Threading;
using System.Windows.Threading;

namespace Real_Final_Project
{
    /// <summary>
    /// Interaction logic for MainPresentation.xaml
    /// </summary>
    public partial class MainPresentation : Page
    {
        #region
        int index = -1;
         GestureRecognitionEngine recognitionEngine;
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"D:/Gerture_Main_Form/Clapping_Project/clap.wav");
        Skeleton mySkeleton;

        // variables
        KinectSensor sensor;
        Skeleton[] TotalSkeletons = new Skeleton[6];

        //bitmap list
        List<WriteableBitmap> PhotoList;
        
        // color stream
        private WriteableBitmap colorBitmap;
        private byte[] colorPixels;

        // positon variables
         System.Timers.Timer positionTimer;
         float oldRFoot = 0.0f;

        // waking fast variables
         System.Timers.Timer speedTimer;
        int counter = 0;
        float oldRFootSpeed = 0.0f;

        
        private const float GAP = 0.05f;

        // Gesture Repetition
        System.Timers.Timer gestureRepetition;
        System.Timers.Timer EvSec;
        
        List<double> list = new List<double>();
        double oldRX = 0.0, oldLX = 0.0, oldRY = 0.0, oldLY = 0.0;

        bool flag = false; // to enable the timers
          #endregion
        private int counter_sec = 0;
        private int filler_counter = 0;
        private void OnTickFiller(object source, ElapsedEventArgs e)
        {

            counter_sec++;
            if (counter_sec == 10)
            {
                //MessageBox.Show(filler_counter.ToString());
                counter_sec = 0;
                if (filler_counter >= 3)
                {
                    MessageBox.Show("filler word");
                    filler_counter = 0;
                }
            }

        }

        public MainPresentation()
        {
            InitializeComponent();
          PhotoList  = new List<WriteableBitmap>();

         // Timer To check Position
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();

        }
        
        private void OnTick(object source, ElapsedEventArgs e)
        {
            if (samePosition())
                MessageBox.Show("Good Positioning");
            else
                MessageBox.Show("You Shoud Keep Moving");
        }

        private void OnTickSpeed(object source, ElapsedEventArgs e)
        {

            if (speedWaking())
                counter++;

            if (counter > 10)
            {
                counter = 0;
                MessageBox.Show("You are Waking very fast, plz slow down");

                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                synthesizer.Volume = 100;  // 0...100
                synthesizer.Rate = -2;     // -10...10
                synthesizer.SpeakAsync("You are Waking very fast, plz slow down");
            }
        }
        
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            if (samePosition())
                MessageBox.Show("Good Positioning");
            else
                MessageBox.Show("You Shoud Keep Moving");
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10
            synthesizer.SpeakAsync("You Shoud Keep Moving");
        }
        
        private bool samePosition()
        {
            float newRFoot = mySkeleton.Joints[JointType.FootRight].Position.X;

            if ((oldRFoot >= 0 && newRFoot >= 0) && ((newRFoot - oldRFoot) >= 0.4f))
            {
                oldRFoot = newRFoot;
                return true;
            }
                
            if ((oldRFoot < 0 && newRFoot < 0) && (((-1 * newRFoot) - (-1 * oldRFoot)) >= 0.4f))
            {
                oldRFoot = newRFoot;
                return true;
            }
                
            if ((oldRFoot >= 0 && newRFoot < 0) || (oldRFoot < 0 && newRFoot >= 0))
            {
                oldRFoot = newRFoot;
                return true;
            }

            oldRFoot = newRFoot;
            return false;
        }

        private bool speedWaking()
        {
            float newRFootSpeed = mySkeleton.Joints[JointType.FootRight].Position.X;

            if ((oldRFootSpeed >= 0 && newRFootSpeed >= 0) && ((newRFootSpeed - oldRFootSpeed) >= 0.4f))
            {
                oldRFootSpeed = newRFootSpeed;
                return true;
            }

            if ((oldRFootSpeed < 0 && newRFootSpeed < 0) && (((-1 * newRFootSpeed) - (-1 * oldRFootSpeed)) >= 0.4f))
            {
                oldRFootSpeed = newRFootSpeed;
                return true;
            }

            if ((oldRFootSpeed >= 0 && newRFootSpeed < 0) || (oldRFootSpeed < 0 && newRFootSpeed >= 0))
            {
                oldRFootSpeed = newRFootSpeed;
                return true;
            }

            oldRFootSpeed = newRFootSpeed;
            return false;
        }


        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
      
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

                    this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                    // Timer To check Position
                    positionTimer = new System.Timers.Timer(5 * 60 * 1000); // every 5 min
                    positionTimer.Elapsed += OnTick;
                    
                    // Timer to check the speed
                    speedTimer = new System.Timers.Timer(2 * 1000); // every 2 sec
                    speedTimer.Elapsed += OnTickSpeed;
                    
                    sensor.SkeletonStream.Enable();
                    sensor.SkeletonFrameReady += Skeleton_Frame_Ready;
                    this.speechRecognizer = this.CreateSpeechRecognizer();
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
            recognitionEngine = new GestureRecognitionEngine();
            recognitionEngine.GestureRecognized += new EventHandler<GestureEventArgs>(recognitionEngine_GestureRecognized);
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

        //
                // Presentation Variables
        Microsoft.Office.Interop.PowerPoint.Application objPPT;
        Microsoft.Office.Interop.PowerPoint.Presentations objPresentations;
        Microsoft.Office.Interop.PowerPoint.Presentation objCurrentPresentation;
        Microsoft.Office.Interop.PowerPoint.SlideShowView objSlideShowView;


// function to open slide
private void OpenSlide()
        {
            objPPT = new Microsoft.Office.Interop.PowerPoint.Application();
            objPPT.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;

            //Open the presentation
            objPresentations = objPPT.Presentations;
            objCurrentPresentation = objPresentations.Open(@"D:/p1.pptx", MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
            //Hide the Presenter View
            objCurrentPresentation.SlideShowSettings.ShowPresenterView = MsoTriState.msoFalse;
            //Run the presentation
            objCurrentPresentation.SlideShowSettings.Run();
            //Hold a reference to the SlideShowWindow
            objSlideShowView = objCurrentPresentation.SlideShowWindow.View;

            //Unless running on a timer you have to activate the SlideShowWindow before showing the next slide
            objSlideShowView.Application.SlideShowWindows[1].Activate();
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
                    return;
                }
             
                // here
                recognitionEngine.Skeleton = firstSkeleton;
                mySkeleton = firstSkeleton;
                recognitionEngine.StartRecognize();
            }

            if (!flag)
            {
                flag = true;
                    speedTimer.Start();
                    positionTimer.Start();
               //         gestureRepetition.Start();
              //          EvSec.Start();
                    OpenSlide();
                    Filler_timer.Start();
            }



        }

      private void recognitionEngine_GestureRecognized(object sender, GestureEventArgs e)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            // create frame from the writable bitmap and add to encoder
            PhotoList.Add(this.colorBitmap);
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));
            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);
            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = System.IO.Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");
           
            int SlCount=objCurrentPresentation.Slides.Count;
            
          
            if (e.GestureType.ToString() == "SwipHandToright")
            {
                    objSlideShowView.Next(); // next
                    index++;
            }

            else if (e.GestureType.ToString() == "SwipHandToleft")
            {
                objSlideShowView.Previous(); // previous
                index--;

            }
          
            if (SlCount == index)
            {
                //MessageBox.Show("d5lt");
              
                //Process[] V = Process.GetProcessesByName("POWERPNT.EXE");
                //V[0].Kill();
               // MainWindow.PPP.Show();
                (Application.Current.MainWindow.FindName("mainFrame") as Frame).Source = new Uri("ListPhotos.xaml", UriKind.Relative);
                
            }

            // write the new file to disk
          try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Error in Screenshot");
            }

            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10
            synthesizer.SpeakAsync(e.GestureType.ToString());
     

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
          grammar.Add("OK");


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
          if ((e.Result.Text.ToLower() == "OK") && e.Result.Confidence >= 0.5)
          {
              filler_counter++;
              MessageBox.Show("OK");

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

