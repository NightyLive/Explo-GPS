using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Devices;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;


namespace Explo_GPS
{

    public partial class Compas : PhoneApplicationPage
    {
        AppSettings App_Settings = new AppSettings();
        PhotoCamera Camera_Source = new PhotoCamera(CameraType.Primary);
        Compass Boussolle_Get_Timer;
        Accelerometer Accelerometre_Get_Timer;
        double cap_magnetique;
        double cap_reel;
        double cap_precision;
        bool donnees_valides;
        bool calibration = false;
        List<double> cap_reel_list = new List<double>();
        List<double> cap_magnetique_list = new List<double>();
        bool compas_running = false;
        bool camera_running = false;
        bool capture_running = false;
        bool camera_ison = false;
        int timer_get = 25; //multiples de 20 (sur le mien c'est 25) verif max avec : MessageBox.Show(Boussolle_Get_Timer.TimeBetweenUpdates.TotalMilliseconds + " ms");

        int cap_list_length = 8;
        int cap_angle_min = -45;
        int cap_angle_max = 45;
        double cap_reel_angle_step = 1;
        double cap_magnetique_angle_step = 1;

        //Constructor
        public Compas()
        {
            InitializeComponent();
            if (!Compass.IsSupported)
            {
                MessageBox.Show(" Ce téléphone ne dispose pas de bousolle !");
                //NavigationService.GoBack();   // A DESACTIVER POUR SIMULATEUR
            }
            else
            {
                if (App_Settings.Switch_AutostartCompas_Setting == true)
                {
                    Click_CompassOnOff(null, EventArgs.Empty);
                }
                if (App_Settings.Switch_AutostartCamera_Setting == true)
                {
                    Click_CameraOnOff(null, EventArgs.Empty);
                }
            }
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            AppReportingService.Flurry_Page_Init();
        }

        //Actions
        private void Click_Back(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
        private void Click_CompassOnOff(object sender, EventArgs e)
        {

            if (compas_running == true)
            {
                if (camera_running == true)
                {
                    try
                    {
                        Camera_Source.CaptureImageAvailable += new System.EventHandler<ContentReadyEventArgs>(Camera_Image_Available);
                        Camera_Source.FlashMode = FlashMode.Off;
                        Camera_Source.CaptureImage();
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show(" La caméra n'a pas eu le temps de s'initialiser.");
                    }
                }
                Boussolle_Get_Timer.Stop();
                Accelerometre_Get_Timer.Stop();
                etat_TextBlock.Text = " Pause";
                compas_running = false;
            }
            else
            {
                if (Boussolle_Get_Timer == null)
                {
                    Boussolle_Get_Timer = new Compass();
                    Boussolle_Get_Timer.TimeBetweenUpdates = TimeSpan.FromMilliseconds(timer_get);
                    Boussolle_Get_Timer.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<CompassReading>>(Boussolle_ValueChanged);
                    Boussolle_Get_Timer.Calibrate += new EventHandler<CalibrationEventArgs>(Calibration_Start);
                }
                try
                {
                    etat_TextBlock.Text = " Démarrage...";
                    Boussolle_Get_Timer.Start();
                    Accelerometre_Get_Timer = new Accelerometer();
                    Accelerometre_Get_Timer.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(Orientation_ValueChanged);
                    Accelerometre_Get_Timer.Start();
                    Accelerometre_Get_Timer = new Accelerometer();
                    Accelerometre_Get_Timer.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(Orientation_ValueChanged);
                    Accelerometre_Get_Timer.Start();
                    compas_running = true;
                    //MessageBox.Show("Reel : " + Boussolle_Get_Timer.TimeBetweenUpdates.TotalMilliseconds + "ms \nPrevu : " + timer_get + "ms \nDisplay : " + timer_display + "ms"); //timer affichage
                    if (camera_running == false && camera_ison == true)
                    {
                        Camera_Ellipse.Fill = Camera_Brush;
                        Camera_Source = new PhotoCamera(CameraType.Primary);
                        Camera_Brush.SetSource(Camera_Source);
                        camera_running = true;
                        Image_Ellipse.Opacity = 0;
                    }
                }
                catch (InvalidOperationException)
                {
                    etat_TextBlock.Text = " Erreur.";
                }
            }
        }
        private void Click_CameraOnOff(object sender, EventArgs e)
        {
            DispatcherTimer Compas_Circle_Timer = new DispatcherTimer();
            Compas_Circle_Timer.Interval = TimeSpan.FromMilliseconds(1000);
            Compas_Circle_Timer.Tick +=
                delegate(object s, EventArgs args)
                {
                    cross_int1.Opacity = 1;
                    cross_int2.Opacity = 1;
                    cercle_int_Circle.Opacity = 1;
                    Compas_Circle_Timer.Stop();
                };

            if (capture_running == true)
            {
                Image_Ellipse.Opacity = 0;
                capture_running = false;
            }
            if (camera_running == false)
            {
                Compas_Circle_Timer.Start();
                Camera_Ellipse.Fill = Camera_Brush;
                Camera_Source = new PhotoCamera(CameraType.Primary);
                Camera_Brush.SetSource(Camera_Source);
                camera_running = true;
                camera_ison = true;
            }
            else
            {
                Compas_Circle_Timer.Stop();
                Camera_Source.Dispose();
                //Camera_Ellipse.Fill = new SolidColorBrush(Colors.Black);
                cross_int1.Opacity = 0;
                cross_int2.Opacity = 0;
                cercle_int_Circle.Opacity = 0;
                camera_running = false;
                camera_ison = false;
            }


            //Camera_Source.AvailableResolutions
            //Camera_Source.Resolution.Height
            //Camera_Brush_Transform.ScaleX = 2;
            //Camera_Brush_Transform.ScaleY = 2;

            //private void OnButtonFullPress(object sender, EventArgs e)
            //{
            //    if (cam != null)
            //    {
            //        cam.CaptureImage();
            //    }
            //}

            //Camera_Brush.SetSource(cameraoff);

            // Release memory, ensure garbage collection.
            //Camera_Source.Initialized -= cam_Initialized;
            //Camera_Brush.SetSource(Camera_Source.CaptureImage);
            //Camera_Ellipse.Fill = Camera_Source.CaptureImage;
            // ImageBrush Camera_Brush_Image = Camera_Source.GetPreviewBufferArgb32;
            //Camera_Brush.SetSource(Camera_Source.GetPreviewBufferArgb32);
            // mettre a off Camera_Brush.SetSource(Camera_Source);
        }
        private void Click_Calibration_Start(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                calibration_StackPanel.Visibility = Visibility.Visible;
                cap_StackPanel.Visibility = Visibility.Collapsed;
            });
            calibration = true;
        }
        private void Click_Calibration_Ok(object sender, RoutedEventArgs e)
        {

            calibration_StackPanel.Visibility = Visibility.Collapsed;
            cap_StackPanel.Visibility = Visibility.Visible;
            calibration = false;
        }

        //Calls
        private void Calibration_Start(object sender, CalibrationEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                calibration_StackPanel.Visibility = Visibility.Visible;
                cap_StackPanel.Visibility = Visibility.Collapsed;
            });
            calibration = true;
        }
        private void Boussolle_ValueChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(()=>
            {
            if (!calibration)
            {
            }
            else
            {
                if (cap_precision <= 10)
                {
                    calibration_TextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    calibration_TextBlock.Text = " Ok !";
                }
                else
                {
                    calibration_TextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    calibration_TextBlock.Text = " +-" + (cap_precision / 2).ToString("0") + "°";
                }
            }
            //Init variables
            //cap_reel = e.SensorReading.TrueHeading;
            //cap_magnetique = e.SensorReading.MagneticHeading;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            cap_reel_list.Add(e.SensorReading.TrueHeading);
            if (cap_reel_list.Count < cap_list_length)
            {
                if (cap_reel_list.Count >= 2)
                {
                    cap_reel_angle_step = cap_reel_list.ElementAt(cap_reel_list.Count - 2) - cap_reel_list.ElementAt(cap_reel_list.Count - 1);
                    if (cap_reel_angle_step > cap_angle_max || cap_reel_angle_step < cap_angle_min)
                    {
                        cap_reel = e.SensorReading.TrueHeading;
                        cap_reel_list.RemoveRange(0, cap_reel_list.Count - 1);
                    }
                    else
                    {
                        cap_reel = cap_reel_list.Average();
                    }
                }
            }
            else
            {
                cap_reel_angle_step = cap_reel_list.ElementAt(cap_reel_list.Count - 2) - cap_reel_list.ElementAt(cap_reel_list.Count - 1);
                if (cap_reel_angle_step > cap_angle_max || cap_reel_angle_step < cap_angle_min)
                {
                    cap_reel = e.SensorReading.TrueHeading;
                    cap_reel_list.RemoveRange(0, cap_reel_list.Count - 1);
                }
                else
                {
                    cap_reel = cap_reel_list.Average();
                    cap_reel_list.RemoveAt(0);
                }
            }

            cap_magnetique_list.Add(e.SensorReading.MagneticHeading);
            if (cap_magnetique_list.Count < cap_list_length)
            {
                if (cap_magnetique_list.Count >= 2)
                {
                    cap_magnetique_angle_step = cap_magnetique_list.ElementAt(cap_magnetique_list.Count - 2) - cap_magnetique_list.ElementAt(cap_magnetique_list.Count - 1);
                    if (cap_magnetique_angle_step > cap_angle_max || cap_magnetique_angle_step < cap_angle_min)
                    {
                        cap_magnetique = e.SensorReading.MagneticHeading;
                        cap_magnetique_list.RemoveRange(0, cap_magnetique_list.Count - 1);
                    }
                    else
                    {
                        cap_magnetique = cap_magnetique_list.Average();
                    }
                }
            }
            else
            {
                cap_magnetique_angle_step = cap_magnetique_list.ElementAt(cap_magnetique_list.Count - 2) - cap_magnetique_list.ElementAt(cap_magnetique_list.Count - 1);
                if (cap_magnetique_angle_step > cap_angle_max || cap_magnetique_angle_step < cap_angle_min)
                {
                    cap_magnetique = e.SensorReading.MagneticHeading;
                    cap_magnetique_list.RemoveRange(0, cap_magnetique_list.Count - 1);
                }
                else
                {
                    cap_magnetique = cap_magnetique_list.Average();
                    cap_magnetique_list.RemoveAt(0);
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            donnees_valides = Boussolle_Get_Timer.IsDataValid;
            cap_precision = Math.Abs(e.SensorReading.HeadingAccuracy);

            precision_TextBlock.Text = " +-" + (cap_precision / 2).ToString("0") + "°";
            reel_Line_Transform.Angle = 360 - cap_reel;
            magnetique_Line_Transform.Angle = 360 - cap_magnetique;
            
            if (donnees_valides)
            {
                etat_TextBlock.Text = " Réception";
            }
            if (((337 <= cap_magnetique) && (cap_magnetique < 360)) || ((0 <= cap_magnetique) && (cap_magnetique < 22)))
            {
                magnetique_TextBlock.Text = " " + cap_magnetique.ToString("0") + "° N";
                reel_TextBlock.Text = " " + cap_reel.ToString("0") + "° N";
            }
            else if ((22 <= cap_magnetique) && (cap_magnetique < 67))
            {
                magnetique_TextBlock.Text = " " + cap_magnetique.ToString("0") + "° NE";
                reel_TextBlock.Text = " " + cap_reel.ToString("0") + "° NE";
            }
            else if ((67 <= cap_magnetique) && (cap_magnetique < 112))
            {
                magnetique_TextBlock.Text = " " + cap_magnetique.ToString("0") + "° E";
                reel_TextBlock.Text = " " + cap_reel.ToString("0") + "° E";
            }
            else if ((112 <= cap_magnetique) && (cap_magnetique < 152))
            {
                magnetique_TextBlock.Text = " " + cap_magnetique.ToString("0") + "° SE";
                reel_TextBlock.Text = " " + cap_reel.ToString("0") + "° SE";
            }
            else if ((152 <= cap_magnetique) && (cap_magnetique < 202))
            {
                magnetique_TextBlock.Text = " " + cap_magnetique.ToString("0") + "° S";
                reel_TextBlock.Text = " " + cap_reel.ToString("0") + "° S";
            }
            else if ((202 <= cap_magnetique) && (cap_magnetique < 247))
            {
                magnetique_TextBlock.Text = " " + cap_magnetique.ToString("0") + "° SO";
                reel_TextBlock.Text = " " + cap_reel.ToString("0") + "° SO";
            }
            else if ((247 <= cap_magnetique) && (cap_magnetique < 292))
            {
                magnetique_TextBlock.Text = " " + cap_magnetique.ToString("0") + "° O";
                reel_TextBlock.Text = " " + cap_reel.ToString("0") + "° O";
            }
            else if ((292 <= cap_magnetique) && (cap_magnetique < 337))
            {
                magnetique_TextBlock.Text = " " + cap_magnetique.ToString("0") + "° NO";
                reel_TextBlock.Text = " " + cap_reel.ToString("0") + "° NO";
            }

        });

        }
        
        void Orientation_ValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            Vector3 v = e.SensorReading.Acceleration;
            bool isCompassUsingNegativeZAxis = false;
            if (Math.Abs(v.Z) < Math.Cos(Math.PI / 4) && (v.Y < Math.Sin(7 * Math.PI / 4)))
            {
                isCompassUsingNegativeZAxis = true;
            }
            Dispatcher.BeginInvoke(() => { orientation_TextBlock.Text = (isCompassUsingNegativeZAxis) ? " Portrait" : " Couché"; });
        }
        void Camera_Image_Available(object sender, ContentReadyEventArgs e)
        {
            Dispatcher.BeginInvoke(() => Camera_Image_Capture(e));
        }
        void Camera_Image_Capture(ContentReadyEventArgs e)
        {
            // Ameliorer vitesse capture trop lent
            // Camera_Source.Resolution = new Size(400, 800);
            if (e.ImageStream.CanRead == true)
            {
                BitmapImage image_capture = new BitmapImage();
                image_capture.SetSource(e.ImageStream);
                ImageBrush still = new ImageBrush();
                still.ImageSource = image_capture;
                Image_Ellipse.Fill = still;
                Image_Ellipse.Fill.RelativeTransform = new CompositeTransform() { CenterX = .5, CenterY = .5, Rotation = 90, ScaleX = 3.3, ScaleY = 2.5 };
                Image_Ellipse.Opacity = 1;
                Camera_Source.Dispose();
                camera_running = false;
                capture_running = true;
                e.ImageStream.Close();
            }
            else
            {
                //MessageBox.Show("La capture n'a pas eu le temps de s'initialiser !");
            }
        }
    }
}