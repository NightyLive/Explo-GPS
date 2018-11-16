using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;


namespace Explo_GPS
{
    public partial class BingMaps : PhoneApplicationPage
    {
        // Declarations des variables
        GeoCoordinateWatcher Geo_Watcher = null;
        MapPolygon Poly_Circle = null;
        Pushpin Pin_Loc = null;
        ProgressBar Progress_Bar1 = new ProgressBar();
        ProgressBar Progress_Bar2 = new ProgressBar();
        private static readonly GeoCoordinate Geo_Default_Loc = new GeoCoordinate(47, 10);
        double geo_latitude;
        double geo_longitude;
        GeoCoordinate Geo_Latlon;
        bool pin_loc_set = false;
        //bool circle_loc_set = false;
        double pin_add_here_text = 1;
        bool mode_road = true;
        double geo_lat_wiki;
        double geo_lon_wiki;
        string geo_title_wiki;
        string geo_url_wiki;
        string geo_distance_wiki;
        double geo_distance_wiki_numeric;
        double geo_lat_google;
        double geo_lon_google;
        string geo_title_google;
        Color Col_Accent = (Color)Application.Current.Resources["PhoneAccentColor"];
        Brush Bru_Accent = (Brush)Application.Current.Resources["PhoneAccentBrush"];
        AppSettings App_Settings = new AppSettings();
        DispatcherTimer Check_Download_Timer = null;
        DispatcherTimer Circle_Timer = null;
        bool bing_dl_bar = false;
        List<double> circle_accuracy_list = new List<double>();
        double circle_accuracy_list_length = 8;
        double circle_accuracy; // = 1;
        double circle_accuracy_dest; // = 1;
        GeoCoordinate Accuracy_Pos; // = new GeoCoordinate(0,0);
        double circle_timer_ticks = 1;
        
        
        
        // Constructor
        public BingMaps()
        {
            InitializeComponent();
            //if (NetworkInterface.GetIsNetworkAvailable() != true)
            //{
            //    MessageBox.Show("Connexion de données non disponible !");
            //    NavigationService.GoBack();
            //    //NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            //}
            BingMapTimersStart();
            if (App_Settings.Switch_Underlock_Setting == true)
            {
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            if (App_Settings.Switch_Avoidlock_Setting == true)
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            } 
            if (App_Settings.Switch_Autostart_Setting == true)
            {
                GeoWatcherStart();
            }
            if (App_Settings.Switch_Roadmode_Setting == true)
            {
                map1.Mode = new RoadMode();
                mode_road = true;
            }
            else
            {
                map1.Mode = new AerialMode();
                mode_road = false;
            }
            //map1.ZoomLevel = 3;
            map1.ZoomLevel = 13;
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            AppReportingService.Flurry_Page_Init();
        }

        private void Click_FocusMe(object sender, EventArgs e)
        {
            if (Geo_Watcher == null)
            {
                GeoWatcherStart();
            }
            map1.Center = Geo_Latlon;
            map1.ZoomLevel = 17;
        }
        private void Click_Back(object sender, EventArgs e)
        {
            Check_Download_Timer.Stop();
            NavigationService.GoBack();
        }
        private void Click_Mode(object sender, EventArgs e)
        {
            if (mode_road == true)
            {
                map1.Mode = new AerialMode();
                mode_road = false;
            }
            else
            {
                map1.Mode = new RoadMode();
                mode_road = true;
            }
        }
        private void Click_AddPinHere(object sender, EventArgs e)
        {
            GeoCoordinate geo_latlon_add = new GeoCoordinate(geo_latitude, geo_longitude);
            Pushpin pin_add = new Pushpin();
            pin_add.Location = geo_latlon_add;
            pin_add.Content = pin_add_here_text;
            pin_add.Foreground = new SolidColorBrush(Colors.White);
            pin_add.Background = new SolidColorBrush(Colors.Purple);
            map1.Children.Add(pin_add);
            pin_add_here_text = pin_add_here_text + 1;
            map1.Center = geo_latlon_add;
            map1.ZoomLevel = 17;
        }
/*        private void Click_AddPin(object sender, EventArgs e)
        {
            double geo_add_man_lat = 0;
            double geo_add_man_lon = 0;
            string geo_add_man_lat_str = "lat_empty";
            string geo_add_man_lon_str = "lon_empty";
            decimal geo_number_lat;
            decimal geo_number_lon;
            Popup bm_popup = new Popup();
            bm_popup.VerticalOffset = 0;
            BingMapsPopup control = new BingMapsPopup();
            bm_popup.Child = control;
            bm_popup.IsOpen = true;
            control.PopTextBoxLat.Focus();
            control.btnOK.Click += (s, args) =>
            {
                bm_popup.IsOpen = false;
                geo_add_man_lat_str = control.PopTextBoxLat.Text;
                geo_add_man_lon_str = control.PopTextBoxLon.Text;
                NumberStyles style = NumberStyles.AllowDecimalPoint;
                CultureInfo culture = new CultureInfo("en-US");
                if (Decimal.TryParse(geo_add_man_lat_str, style, culture, out geo_number_lat) && Decimal.TryParse(geo_add_man_lon_str, style, culture, out geo_number_lon))
                {
                    geo_add_man_lat = (double)geo_number_lat;
                    geo_add_man_lon = (double)geo_number_lon;
                    GeoCoordinate geo_add_man_latlon = new GeoCoordinate(geo_add_man_lat, geo_add_man_lon);
                    Pushpin pin_add = new Pushpin();
                    pin_add.Location = geo_add_man_latlon;
                    pin_add.Content = "Ajout manuel";
                    pin_add.Foreground = new SolidColorBrush(Colors.White);
                    pin_add.Background = new SolidColorBrush(Colors.Orange);
                    map1.Children.Add(pin_add);
                    map1.Center = geo_add_man_latlon;
                    map1.ZoomLevel = 17;
                }
                else
                {
                    MessageBox.Show("Coordonnées invalides. \nFormat décimal exigé : 10.174568");
                };
            };
            control.btnCancel.Click += (s, args) =>
            {
                bm_popup.IsOpen = false;
            };
        }
*/
        private void Click_DelPins(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment effacer les marqueurs ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Supprime toutes les Pins ajoutées
                map1.Children.Clear();
                pin_add_here_text = 1;
                pin_loc_set = false;
                Poly_Circle = null;
                //Accuracy Circle
                if (Poly_Circle == null)
                {
                    Poly_Circle = new MapPolygon();
                    Poly_Circle.Opacity = 0.5;
                    Poly_Circle.Fill = new SolidColorBrush(Colors.Transparent); //.LightGray);
                    Poly_Circle.Stroke = new SolidColorBrush(Col_Accent);
                    Poly_Circle.StrokeThickness = 4; //4
                    Poly_Circle.Locations = CreateCircle(Accuracy_Pos, circle_accuracy_dest);
                    map1.Children.Add(Poly_Circle);
                }
                // Pushpin Me
                Geo_Latlon = new GeoCoordinate(geo_latitude, geo_longitude);
                if (pin_loc_set == false)
                {
                    Pin_Loc = new Pushpin();
                    Pin_Loc.Content = "Moi";
                    Pin_Loc.Foreground = new SolidColorBrush(Colors.White);
                    Pin_Loc.Background = new SolidColorBrush(Col_Accent);
                    map1.Children.Add(Pin_Loc);
                    pin_loc_set = true;
                }
                Pin_Loc.Location = Geo_Latlon;
            }
        }
        private void Click_GetWikipedia(object sender, EventArgs e)
        {
            Progress_Bar1.IsIndeterminate = true;
            Progress_Bar1.IsHitTestVisible = false;
            Progress_Bar1.Height = 40;
            Progress_Bar1.Foreground = Bru_Accent;
            if (Progress_Bar1.IsIndeterminate == false)
            {
                Progress_Bar1.Minimum = 0;
                Progress_Bar1.Maximum = 100;
                Progress_Bar1.Value = 1;
            }
            map1.Children.Add(Progress_Bar1);

            WebClient client = new WebClient();
            string url_lat = geo_latitude.ToString().Replace(",", ".");
            string url_lon = geo_longitude.ToString().Replace(",", ".");
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Add_Wikipedia);
            string url_wiki = "http://api.wikilocation.org/articles?lat=" + url_lat + "&lng=" + url_lon + "&limit=20&radius=10000&format=xml&locale=fr";
            //Debug.WriteLine("Url: " + url_wiki);
            //MessageBox.Show("URL : " + url_wiki);
            client.DownloadStringAsync(new Uri(url_wiki));
        }
        private void Add_Wikipedia(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Erreur de téléchargement de la base de données XML");
            }
            else
            {
                StringBuilder output = new StringBuilder();
                using (XmlReader reader = XmlReader.Create(new StringReader(e.Result)))
                {
                    try
                    {
                        while (reader.Read())
                        {
                            reader.ReadToFollowing("lat");
                            if ((reader.NodeType != XmlNodeType.None) & (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                            {
                                geo_lat_wiki = reader.ReadElementContentAsDouble();
                            }
                            if ((reader.NodeType != XmlNodeType.None) & (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                            {
                                geo_lon_wiki = reader.ReadElementContentAsDouble();
                            }
                            reader.ReadToFollowing("title");
                            if ((reader.NodeType != XmlNodeType.None) & (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                            {
                                geo_title_wiki = reader.ReadElementContentAsString();
                            }
                            reader.ReadToFollowing("mobileurl");
                            if ((reader.NodeType != XmlNodeType.None) & (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                            {
                                geo_url_wiki = reader.ReadElementContentAsString();
                            }
                            if ((reader.NodeType != XmlNodeType.None) & (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                            {
                                geo_distance_wiki = reader.ReadElementContentAsString().Replace("m", "");
                                geo_distance_wiki_numeric = Convert.ToDouble(geo_distance_wiki);
                                if (geo_distance_wiki_numeric < 100)
                                {
                                    geo_distance_wiki = geo_distance_wiki_numeric.ToString() + "m";
                                }
                                else if (geo_distance_wiki_numeric >= 100 && geo_distance_wiki_numeric < 10000)
                                {
                                    geo_distance_wiki_numeric = geo_distance_wiki_numeric / 1000;
                                    geo_distance_wiki = geo_distance_wiki_numeric.ToString().Remove(3) + "km";
                                }
                                else if (geo_distance_wiki_numeric >= 10000)
                                {
                                    geo_distance_wiki_numeric = geo_distance_wiki_numeric / 1000;
                                    geo_distance_wiki = geo_distance_wiki_numeric.ToString().Remove(2) + "km";
                                }
                            }
                            GeoCoordinate geo_latlon_wiki_add = new GeoCoordinate(geo_lat_wiki, geo_lon_wiki);
                            Pushpin pin_wiki_add = new Pushpin();
                            pin_wiki_add.Location = geo_latlon_wiki_add;
                            pin_wiki_add.Content = geo_title_wiki + " " + geo_distance_wiki;
                            pin_wiki_add.Foreground = new SolidColorBrush(Colors.White);
                            pin_wiki_add.Background = new SolidColorBrush(Colors.DarkGray);
                            map1.Children.Add(pin_wiki_add);
                        }
                    }
                    catch
                    {
                        //MessageBox.Show("Erreur dans la source XML");
                    }
                }
            }
            map1.Children.Remove(Progress_Bar1);
        }
//        private void LaunchWikiUrl(object sender, MouseButtonEventArgs e)
//        {
//            WebBrowserTask webBrowserTask = new WebBrowserTask();
//            MessageBox.Show("geo_url_wiki = " + geo_url_wiki + "\n url = " + e.ToString());
//            webBrowserTask.Uri = new Uri(geo_url_wiki, UriKind.Absolute); // utiliser sender ? e ? geo_url_wiki ?
//            //webBrowserTask.Uri = new Uri(url.ToString(), UriKind.Absolute);
//            webBrowserTask.Show();
//        }
        private void Click_GetGoogle(object sender, EventArgs e)
        {
            try
            {
                Progress_Bar1.IsIndeterminate = true;
                Progress_Bar1.IsHitTestVisible = false;
                Progress_Bar1.Height = 40;
                Progress_Bar1.Foreground = Bru_Accent;
                if (Progress_Bar1.IsIndeterminate == false)
                {
                    Progress_Bar1.Minimum = 0;
                    Progress_Bar1.Maximum = 100;
                    Progress_Bar1.Value = 1;
                }
                map1.Children.Add(Progress_Bar1);

                WebClient client = new WebClient();
                string url_lat = geo_latitude.ToString().Replace(",", ".");
                string url_lon = geo_longitude.ToString().Replace(",", ".");
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Add_Google);
                string url_google = "https://maps.googleapis.com/maps/api/place/search/xml?location=" + url_lat + "," + url_lon + "&radius=500&sensor=false&key=AIzaSyBszrf925x0e8HlM-uStGfv8efUMIYledg";
                client.DownloadStringAsync(new Uri(url_google));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue.");
                FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": BingMaps Click_GetGoogle: " + ex.Message.ToString(), ex);
                FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
            }
        }
        private void Add_Google(object sender, DownloadStringCompletedEventArgs e)
        {
            //MessageBox.Show("Etape 3 : XML : " + e.Result);
            if (e.Error != null)
            {
                MessageBox.Show("Erreur de téléchargement de la base de données XML");
            }
            else
            {
                StringBuilder output = new StringBuilder();
                using (XmlReader reader = XmlReader.Create(new StringReader(e.Result)))
                {
                    try
                    {
                        while (reader.Read())
                        {
                            reader.ReadToFollowing("name");
                            if ((reader.NodeType != XmlNodeType.None))// & (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                            {
                                geo_title_google = reader.ReadElementContentAsString();
                            }
                            reader.ReadToFollowing("lat");
                            if ((reader.NodeType != XmlNodeType.None)) //& (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                            {
                                geo_lat_google = reader.ReadElementContentAsDouble();
                            }
                            reader.ReadToFollowing("lng");
                            if ((reader.NodeType != XmlNodeType.None))// & (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                            {
                                geo_lon_google = reader.ReadElementContentAsDouble();
                            }
                            //MessageBox.Show(geo_lat_google + "\n" + geo_lon_google + "\n" + geo_title_google);
                            GeoCoordinate geo_latlon_google_add = new GeoCoordinate(geo_lat_google, geo_lon_google);
                            Pushpin pin_google_add = new Pushpin();
                            pin_google_add.Location = geo_latlon_google_add;
                            pin_google_add.Content = geo_title_google;
                            pin_google_add.Foreground = new SolidColorBrush(Colors.White);
                            pin_google_add.Background = new SolidColorBrush(Colors.DarkGray);
                            map1.Children.Add(pin_google_add);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Erreur dans la source XML");
                    }
                }
            }
            map1.Children.Remove(Progress_Bar1);
        }
//        private void LaunchGoogleUrl(object sender, MouseButtonEventArgs e)
//        {
//            WebBrowserTask webBrowserTask = new WebBrowserTask();
//            MessageBox.Show("geo_url_wiki = " + geo_url_wiki + "\n url = " + e.ToString());
//            webBrowserTask.Uri = new Uri(geo_url_wiki, UriKind.Absolute); // utiliser sender ? e ? geo_url_wiki ?
//            //webBrowserTask.Uri = new Uri(url.ToString(), UriKind.Absolute);
//            webBrowserTask.Show();
//        }
/*        private void Click_Buy(object sender, EventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.ContentIdentifier = "16751342-68ff-4c91-929d-9cc534cd9993";
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
            marketplaceDetailTask.Show();
        }
*/
        // Auto
        private void BingMapTimersStart()
        {
            if (Check_Download_Timer == null)
            {
                Check_Download_Timer = new DispatcherTimer();
                Check_Download_Timer.Interval = TimeSpan.FromMilliseconds(100);
                Check_Download_Timer.Tick += new EventHandler(BingMapCheckDownloading);
                Progress_Bar2.IsIndeterminate = true;
                Progress_Bar2.IsHitTestVisible = false;
                Progress_Bar2.Height = 40;
                Progress_Bar2.Foreground = Bru_Accent;
                Progress_Bar2.VerticalAlignment = VerticalAlignment.Bottom;
                Progress_Bar2.Margin = new Thickness(0, 0, 0, 50);
                //Check_Download_Timer.Start(); //desactiver la progressbar fait ramer bingmaps
            }
            if (Circle_Timer == null)
            {
                Circle_Timer = new DispatcherTimer();
                Circle_Timer.Interval = TimeSpan.FromMilliseconds(50);
                Circle_Timer.Tick += new EventHandler(BingMapCircleTimer);
                //Circle_Timer.Start(); // se lance tout seul des que la position change
            }
        }
        private void BingMapCheckDownloading(object sender, EventArgs e)
        {
            if (map1.IsDownloading == true)
            {
                if (bing_dl_bar == false)
                {
                    map1.Children.Add(Progress_Bar2);
                    bing_dl_bar = true;
                }
            }
            else
            {
                if (bing_dl_bar == true)
                {
                    map1.Children.Remove(Progress_Bar2);
                    bing_dl_bar = false;
                }
            }
        }
        private void BingMapCircleTimer(object sender, EventArgs e)
        {
            //circle_accuracy
            circle_accuracy_list.Add(circle_accuracy);
            if (circle_accuracy_list.Count < circle_accuracy_list_length)
            {
                if (circle_accuracy_list.Count >= 2)
                {
                    circle_accuracy_dest = circle_accuracy_list.Average();
                }
            }
            else
            {
                circle_accuracy_dest = circle_accuracy_list.Average();
                circle_accuracy_list.RemoveAt(0);
            }
            if (circle_timer_ticks <= 10 & Accuracy_Pos != null)
            {
                circle_timer_ticks = circle_timer_ticks + 1;
                Poly_Circle.Locations = CreateCircle(Accuracy_Pos, circle_accuracy_dest);
            }
            else
            {
                Circle_Timer.Stop();
                circle_timer_ticks = 1;
            }
        }
        private void GeoWatcherStart()
        {
            if (Geo_Watcher == null)
            {
                if (App_Settings.Switch_GPS_Sensibility_Setting == true)
                {
                    Geo_Watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                }
                else
                {
                    Geo_Watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
                }
                Geo_Watcher.MovementThreshold = Convert.ToInt32(App_Settings.GPS_Threshold); // 20 meters. 
                Geo_Watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(GeoWatcherStatusChanged);
                Geo_Watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(GeoWatcherPositionChanged);
            }
            Geo_Watcher.Start();
        }
        private void GeoWatcherPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => GeoMyPositionChanged(e));
        }
        private void GeoMyPositionChanged(GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //Accuracy Circle
            circle_accuracy = e.Position.Location.HorizontalAccuracy;
            if (circle_accuracy < e.Position.Location.VerticalAccuracy)
            {
                circle_accuracy = e.Position.Location.VerticalAccuracy;
            }
            if (Poly_Circle == null)
            {
                Poly_Circle = new MapPolygon();
                Poly_Circle.Opacity = 0.5;
                Poly_Circle.Fill = new SolidColorBrush(Colors.Transparent); //.LightGray);
                Poly_Circle.Stroke = new SolidColorBrush(Col_Accent);
                Poly_Circle.StrokeThickness = 4; //4
                map1.Children.Add(Poly_Circle);
            }
            // Desactiver polycircle et appeler fonction Compas_Display_Timer accuracycircle <-- lancée au boot
            //Poly_Circle.Locations = CreateCircle(e.Position.Location, circle_accuracy);
            Accuracy_Pos = e.Position.Location; // besoin de cette valeur pour le Compas_Display_Timer
            Circle_Timer.Start(); // lance Compas_Display_Timer manuel

            // Pushpin Me
            geo_latitude = e.Position.Location.Latitude;
            geo_longitude = e.Position.Location.Longitude;
            Geo_Latlon = new GeoCoordinate(geo_latitude, geo_longitude);
            if (pin_loc_set == false)
            {
                Pin_Loc = new Pushpin();
                Pin_Loc.Content = "Moi";
                Pin_Loc.Foreground = new SolidColorBrush(Colors.White);
                Pin_Loc.Background = new SolidColorBrush(Col_Accent);
                map1.Children.Add(Pin_Loc);
                pin_loc_set = true;
            }
            if (App_Settings.Switch_Follow_Setting == true)
            {
                map1.Center = Geo_Latlon;
            }
            Pin_Loc.Location = Geo_Latlon;
        }
        private void GeoWatcherStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => GeoMyStatusChanged(e));
        }
        private void GeoMyStatusChanged(GeoPositionStatusChangedEventArgs e)
        {
            //Debug.WriteLine("GeoMyStatusChanged()");
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    if (Geo_Watcher.Permission == GeoPositionPermission.Denied)
                    {
                        MessageBox.Show("Le service de localisation est désactivé sur cet appareil");
                        NavigationService.GoBack();
                    }
                    else
                    {
                        MessageBox.Show("Le service de localisation ne fonctionne pas sur cet appareil");
                        NavigationService.GoBack();
                    }
                    break;
                case GeoPositionStatus.NoData:
                    MessageBox.Show("Données de localisation indisponibles");
                    break;
                case GeoPositionStatus.Ready:
                    break;
            }
        }
        private static LocationCollection CreateCircle(GeoCoordinate center, double radius)
        {
            var earthRadius = 6367000; // radius in meters
            var lat = ToRadian(center.Latitude); //radians
            var lng = ToRadian(center.Longitude); //radians
            var d = radius / earthRadius; // d = angular distance covered on earth's surface
            var locations = new LocationCollection();
            for (var x = 0; x <= 360; x++)
            {
                var brng = ToRadian(x);
                var latRadians = Math.Asin(Math.Sin(lat) * Math.Cos(d) + Math.Cos(lat) * Math.Sin(d) * Math.Cos(brng));
                var lngRadians = lng + Math.Atan2(Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat), Math.Cos(d) - Math.Sin(lat) * Math.Sin(latRadians));
                locations.Add(new GeoCoordinate(ToDegrees(latRadians), ToDegrees(lngRadians)));
            }
            return locations;
        }
        // Calls
        private static double ToRadian(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
        private static double ToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }
    }
}