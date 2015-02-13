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
//using Microsoft.Office.Core;

namespace Real_Final_Project
{
    /// <summary>
    /// Interaction logic for PresentationSelection.xaml
    /// </summary>
    public partial class PresentationSelection : Page
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

        // Presentation
        //Microsoft.Office.Interop.PowerPoint.Application objPPT;
        //Microsoft.Office.Interop.PowerPoint.Presentations objPresentations;
        //Microsoft.Office.Interop.PowerPoint.Presentation objCurrentPresentation;
        //Microsoft.Office.Interop.PowerPoint.SlideShowView objSlideShowView;

        #endregion

        public PresentationSelection()
        {
            InitializeComponent();
            this.WindowHeight = (double)WindowState.Maximized;
            this.WindowWidth = (double)WindowState.Maximized;
            InitializeButtons();
            Generics.ResetHandPosition(kinectButton);
            kinectButton.Click += new RoutedEventHandler(kinectButton_Click);
        }
        //initialize buttons to be checked
        private void InitializeButtons()
        {
            buttons = new List<Button> {};
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
                    sensor.Start();

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

                kinectButton.Visibility = System.Windows.Visibility.Visible;

                DepthImagePoint point = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(hand.Position, DepthImageFormat.Resolution640x480Fps30);
                handX = (int)((point.X * LayoutRoot.ActualWidth / this.sensor.DepthStream.FrameWidth) -
                    (kinectButton.ActualWidth / 2.0));
                handY = (int)((point.Y * LayoutRoot.ActualHeight / this.sensor.DepthStream.FrameHeight) -
                    (kinectButton.ActualHeight / 2.0));
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

        void kinectButton_Click(object sender, RoutedEventArgs e)
        {
            selected.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, selected));
        }
    }
}
