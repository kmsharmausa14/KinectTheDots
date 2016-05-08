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
using HandTracking.Others;
using System.Drawing;
using System.IO;
using System.Windows.Media.Media3D;
using Nui = Microsoft.Kinect;
using System.Windows.Threading;
using System.Timers;

namespace KinectTheDots
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Member Variables
        DateTime dt = DateTime.Now;
        private KinectSensor _KinectDevice;
        //private readonly Brush[] _SkeletonBrushes;
        private Skeleton[] _FrameSkeletons;
        private DotPuzzle _Puzzle;
        private int _PuzzleDotIndex;
        double temp = 0.0;
        Grid dotContainer = new Grid();
        Grid dotContainer1 = new Grid();
        //string path = @"D:\Yourfile.txt";
        string path = @"D:\Yourfile.txt";
        private int counter = 0;
        private Boolean timebox = true;
        private int largeindex = 0;
        private Boolean timeboxlarge = true;
        DispatcherTimer _timer;
        TimeSpan _time;
        Boolean timerbox = true;

        /// <summary>
        /// ////////
        /// </summary>
        #endregion Member Variables


        #region Constructor
        public MainWindow()
        {
            
            //File.Create(path);
            if (!File.Exists(path))
            {
                File.Create(path);
                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine(Convert.ToString(DateTime.Now.ToShortDateString()));
                tw.Close();
            }

            else if (File.Exists(path))
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine(Convert.ToString(DateTime.Now.ToShortDateString()));
                    w.Close();
                }
            }

            



            InitializeComponent();
            /*
            _time = TimeSpan.FromSeconds(100);

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                lblTime.Content = _time.ToString("c");
                if (_time == TimeSpan.Zero) _timer.Stop();
                //_time = _time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);
            _timer.Start();
            */


            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors
                                             .FirstOrDefault(x => x.Status == KinectStatus.Connected);
            //Sample puzzle
            this._Puzzle = new DotPuzzle();
            this._Puzzle.Dots.Add(new Point(200, 300));
            /*
            this._Puzzle.Dots.Add(new Point(1600, 300));
            this._Puzzle.Dots.Add(new Point(1650, 400));
            this._Puzzle.Dots.Add(new Point(1600, 500));
            this._Puzzle.Dots.Add(new Point(1000, 500));
            this._Puzzle.Dots.Add(new Point(1000, 600));
            this._Puzzle.Dots.Add(new Point(1200, 700));
            this._Puzzle.Dots.Add(new Point(1150, 800));
            this._Puzzle.Dots.Add(new Point(750, 800));
            this._Puzzle.Dots.Add(new Point(700, 700));
            this._Puzzle.Dots.Add(new Point(900, 600));
            this._Puzzle.Dots.Add(new Point(900, 500));
            this._Puzzle.Dots.Add(new Point(200, 500));
            this._Puzzle.Dots.Add(new Point(150, 400));
            */
            this._PuzzleDotIndex = -1;

            this.Loaded += MainWindow_Loaded;

        }
        #endregion Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        /////////////TIMER METHOD
       
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status ==
                                                                          KinectStatus.Connected);

            DrawPuzzle(this._Puzzle);
        }
        private void DrawPuzzle(DotPuzzle puzzle)
        {
            PuzzleBoardElement.Children.Clear();

            if (puzzle != null)
            {
                for (int i = 0; i < _Puzzle.Dots.Count; i++)
                {

                    dotContainer.Width = Constants.DOTCONTAINER_WIDTH;
                    dotContainer.Height = Constants.DOTCONTAINER_HEIGHT;
                    dotContainer.Children.Add(new Ellipse() { Fill = Brushes.Gray });
                    
                    //Position the UI element centered on the dot point
                    Canvas.SetTop(dotContainer, puzzle.Dots[i].Y - (dotContainer.Height / 2));
                    Canvas.SetLeft(dotContainer, puzzle.Dots[i].X - (dotContainer.Width / 2));
                    PuzzleBoardElement.Children.Add(dotContainer);
                    
                }
            }
        }
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                    this.KinectDevice = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    //TODO: Give the user feedback to plug-in a Kinect device.    
                    this.KinectDevice = null;
                    break;
                default:
                    //TODO: Show an error state
                    break;
            }
        }
        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    frame.CopySkeletonDataTo(this._FrameSkeletons);
                    Skeleton skeleton = GetPrimarySkeleton(this._FrameSkeletons);

                    if (skeleton == null)
                    {
                        //HandCursorElement.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //HandCursorElement.Visibility = Visibility.Visible;
                        Joint primaryHand = GetPrimaryHand(skeleton);
                        TrackHand(primaryHand);
                        TrackPuzzle(primaryHand.Position);
                    }
                }
            }

        }

        private void TrackPuzzle(SkeletonPoint position)
        {

                largeindex++;

                if ((largeindex > 1000) && (timeboxlarge == true))
                {
                    //MessageBox.Show("You have run out of time?");
                    label.Visibility = Visibility.Visible;
                    _timer.Stop();
                    timeboxlarge = false;
                }
            

            Point dot;
                //to track the current dot viz. dot
                // dotindex is the current dot index which is -1
                //count is the total no. of dots
                if (this._PuzzleDotIndex + 1 < this._Puzzle.Dots.Count)
                {
                    dot = this._Puzzle.Dots[this._PuzzleDotIndex + 1];
                }
                else
                {                
                    dot = this._Puzzle.Dots[0];
                }
                
                //Hand cordinates
                DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(position, DepthImageFormat.Resolution640x480Fps30);
                point.X = (int)(point.X * LayoutRoot.ActualWidth / this.KinectDevice.DepthStream.FrameWidth);
                point.Y = (int)(point.Y * LayoutRoot.ActualHeight / this.KinectDevice.DepthStream.FrameHeight);
                Point handPoint = new Point(point.X, point.Y);
                //Graphics.DrawImage(this.picture, this.pictureLocation);

                //Calculate the length between the two points. This can be done manually 
                //as shown here or by using the System.Windows.Vector object to get the length. 
                //System.Windows.Media.Media3D.Vector3D is available for 3D vector math.
                Point dotDiff = new Point(dot.X - handPoint.X, dot.Y - handPoint.Y);
                double length = Math.Sqrt(dotDiff.X * dotDiff.X + dotDiff.Y * dotDiff.Y);

                //helps in drawing the line
                int lastPoint = (this.CrayonElement.Points.Count) - 1;



                if (length < Constants.DISTANCE_HAND_DOT)
                {

                
                //Cursor is within the hit zone
                if ((counter > 5) &&(_PuzzleDotIndex>1) && (timebox==true))
                    {
                    _timer.Stop();
                    label.Visibility = Visibility.Visible;

                    timebox = false;
                        
                    }

                if (timerbox == true)
                {
                    _time = TimeSpan.FromSeconds(100);

                    _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                    {
                        lblTime.Content = _time.ToString("c");
                        if (_time == TimeSpan.Zero) _timer.Stop();
                        _time = _time.Add(TimeSpan.FromSeconds(-1));
                    }, Application.Current.Dispatcher);
                    _timer.Start();
                    timerbox = false;
                }

                if (lastPoint > 0)
                    {
                        //Remove the working end point
                        this.CrayonElement.Points.RemoveAt(lastPoint);
                    }

                    //Set line end point
                    this.CrayonElement.Points.Add(new Point(dot.X, dot.Y));

                    //Set new line start point
                    this.CrayonElement.Points.Add(new Point(dot.X, dot.Y));

                    //Move to the next dot
                    //this._PuzzleDotIndex++;
                 
                    counter++;
                }
                else
                {
                    counter = 0;
                    if (lastPoint > 0)
                    {
                        //To refresh the Polyline visual you must remove the last point,
                        //update and add it back.
                       
                        // Hand trajectory
                        Point lineEndpoint = this.CrayonElement.Points[lastPoint];
                        lineEndpoint.X = handPoint.X;
                        lineEndpoint.Y = handPoint.Y;
                        this.CrayonElement.Points.Add(lineEndpoint);

                                 //removes the current rotating dot and adds the new dot position               
                                PuzzleBoardElement.Children.Remove(dotContainer);
                                dotContainer = dotContainer1;
                                PuzzleBoardElement.Children.Remove(dotContainer1);
                    this._PuzzleDotIndex++;
                    ///////////////Global addition////////////////////////////////                               
                    // (rotation speed) calculation from the one point to another point
                    temp =temp+.01;

                            /////////////////////////////////////point addition LOCAL
                            dotContainer1.Width = Constants.DOTCONTAINER_WIDTH;
                            dotContainer1.Height = Constants.DOTCONTAINER_HEIGHT;
                            dotContainer1.Children.Add(new Ellipse() { Fill = Brushes.Gray });

                            //Position the UI element centered on the dot point                            
                            double x = (500-10* temp) *Math.Sin(temp)+800;
                            double y = 500 * Math.Cos(temp) + 800;
                            Canvas.SetTop(dotContainer1, y - (dotContainer1.Height / 2));
                            Canvas.SetLeft(dotContainer1, x - (dotContainer1.Width / 2));
                            PuzzleBoardElement.Children.Add(dotContainer1);
                            this._Puzzle.Dots.RemoveAt(0);
                            this._Puzzle.Dots.Add(new Point(x, y));
                        
                    }

                }
            
        }

        private static Joint GetPrimaryHand(Skeleton skeleton)
        {
            Joint righHand = new Joint();

            if (skeleton != null)
            {
                //primaryHand = skeleton.Joints[JointType.HandLeft];
                righHand = skeleton.Joints[JointType.HandRight];
                /*
                if (righHand.TrackingState != JointTrackingState.NotTracked)
                {
                    if (primaryHand.TrackingState == JointTrackingState.NotTracked)
                    {
                        primaryHand = righHand;
                    }
                    else
                    {
                        if (primaryHand.Position.Z > righHand.Position.Z)
                        {
                            primaryHand = righHand;
                        }
                    }
                }
                */
            }
            return righHand;
        }
        private void TrackHand(Joint hand)
        {
            if (hand.TrackingState == JointTrackingState.NotTracked)
            {
                //HandCursorElement.Visibility = System.Windows.Visibility.Visible;
            }

            else
            {
                //HandCursorElement.Visibility = System.Windows.Visibility.Visible;
                //HandCursorElement.Height = 200;
                //HandCursorElement.Width = 200;

                DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(hand.Position, DepthImageFormat.Resolution640x480Fps30);

                point.X = (int)((point.X * LayoutRoot.ActualWidth /
                                   this.KinectDevice.DepthStream.FrameWidth)); //-  (HandCursorElement.ActualWidth / 2.0));

                point.Y = (int)((point.Y * LayoutRoot.ActualHeight) / (this.KinectDevice.DepthStream.FrameHeight)); //-
                // (HandCursorElement.ActualHeight / 2.0));
                

                TextWriter tw = new StreamWriter(path, true);
                String points = Convert.ToString(point.X) + " " + Convert.ToString(point.Y);
                tw.WriteLine(points);
                tw.Close();

                System.Windows.Shapes.Ellipse Ellipse = EllipseLeft;
                Ellipse.Width = Constants.HAND_WIDTH;
                Ellipse.Height = Constants.HAND_HEIGTH;
                Canvas.SetLeft(Ellipse, point.X - (Ellipse.Width / 2));
                Canvas.SetTop(Ellipse, point.Y - (Ellipse.Height / 2));
                
            }
        }
        private static Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;

            if (skeletons != null)
            {
                //Find the closest skeleton       
                for (int i = 0; i < skeletons.Length; i++)
                {
                    if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }
                        else
                        {
                            if (skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }
                }
            }

            return skeleton;
        }
        /*
        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();
            figure.StrokeThickness = Constants.LINE_THICKNESS;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }


            return figure;
        }
         */

        /*
        private Point GetJointPoint(Joint joint)
        {
            DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(joint.Position,this.KinectDevice.DepthStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / this.KinectDevice.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / this.KinectDevice.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }
         */

        #region Properties
        public KinectSensor KinectDevice
        {
            get { return this._KinectDevice; }
            set
            {
                if (this._KinectDevice != value)
                {
                    //Uninitialize
                    if (this._KinectDevice != null)
                    {
                        this._KinectDevice.Stop();
                        this._KinectDevice.SkeletonFrameReady -= KinectDevice_SkeletonFrameReady;
                        this._KinectDevice.SkeletonStream.Disable();
                        this._FrameSkeletons = null;
                    }

                    this._KinectDevice = value;

                    //Initialize
                    if (this._KinectDevice != null)
                    {
                        if (this._KinectDevice.Status == KinectStatus.Connected)
                        {
                            this._KinectDevice.SkeletonStream.Enable();
                            this._FrameSkeletons = new
        Skeleton[this._KinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            this.KinectDevice.SkeletonFrameReady +=
        KinectDevice_SkeletonFrameReady;
                            this._KinectDevice.Start();
                        }
                    }
                }
            }
        }
        #endregion Properties
    }
}






