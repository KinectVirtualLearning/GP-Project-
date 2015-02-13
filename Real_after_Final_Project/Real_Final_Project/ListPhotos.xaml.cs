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
namespace Real_Final_Project
{
    /// <summary>
    /// Interaction logic for ListPhotos.xaml
    /// </summary>
    public partial class ListPhotos : Page
    {
        KinectSensor sensor;
        Skeleton[] TotalSkeletons = new Skeleton[6];
        Skeleton mySkeleton;

        List<Button> buttons;     
        static Button selected;

        float handX;
        float handY;
        public static int Index = 0;  
        public static List<BitmapImage> Photos;
 
        public ListPhotos()
        {
            InitializeComponent();
            InitializeButtons(); 
            Generics.ResetHandPosition(kinectButton);
            kinectButton.Click += new RoutedEventHandler(kinectButton_Click);
            Photos = new List<BitmapImage>();
          
            int i=0;

            while (i < MainPresentation.PhotoList.Count)
            {
                Photos.Add(MainPresentation.PhotoList[i]);
                i++;
            }
        }

        void kinectButton_Click(object sender, RoutedEventArgs e)
        {
            selected.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, selected));
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
            
                mySkeleton = firstSkeleton;
                Joint primaryHand = GetPrimaryHand(firstSkeleton);
                TrackHand(primaryHand);
            }
        }
        private void InitializeButtons()
        {
            buttons = new List<Button> {right,left};
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.sensor = KinectSensor.KinectSensors[0];

                if (sensor.Status == KinectStatus.Connected)
                {
                    sensor.SkeletonStream.Enable();
                    sensor.SkeletonFrameReady += this.Skeleton_Frame_Ready;
                    sensor.Start();
                }
            }
        }

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

        private void right_btn(object sender, RoutedEventArgs e)
        {
           Index++;
           if (Index >= Photos.Count || Index < 0)
           {
               Index = 0;
           }
            this.Image_Background.Source = Photos[Index];
            this.L1.Content = Index.ToString() + " / " + Photos.Count.ToString();
         }

        private void left_btn(object sender, RoutedEventArgs e)
        {
            Index--;
            if (Index >= Photos.Count || Index < 0)
            {
                Index = Photos.Count - 1;
            }

            this.Image_Background.Source = Photos[Index];
            this.L1.Content = Index.ToString() + " / " + (Photos.Count-1).ToString();

        }    
    }
}