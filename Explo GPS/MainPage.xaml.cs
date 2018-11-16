using System;
using System.Device.Location;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Text;
//using trafikappen.Utils;

namespace Explo_GPS
{
    public partial class MainPage : PhoneApplicationPage
    {
        //
        // Declarations de variables
        EmailAddressChooserTask Email_Chooser_Task;
        PhoneNumberChooserTask Phone_Number_Chooser_Task;
        GeoCoordinateWatcher Geo_Watcher = null;
        WebBrowserTask Web_Browser_Task = new WebBrowserTask();
        AppSettings App_Settings = new AppSettings();
        static readonly GeoCoordinate Geo_Default_Loc = new GeoCoordinate(47, 10);
        string coord_url_lat;
        string coord_url_lon;
        double coord_xml_count = 0;
        double coord_brut_lat;
        double coord_brut_lon;
        double coord_brut_acc_hor;
        double coord_brut_acc_ver;
        double coord_brut_alt;
        double coord_brut_speed;
        double coord_brut_cap;
        IsolatedStorageFile My_Isolated_Storage = IsolatedStorageFile.GetUserStoreForApplication();
        XmlWriterSettings Xml_Writer_Settings = new XmlWriterSettings();
        XmlReaderSettings Xml_Reader_Settings = new XmlReaderSettings();
        
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            /*ShellTile PrimaryTile = ShellTile.ActiveTiles.First();
            if (PrimaryTile != null)
            {
                StandardTileData tile = new StandardTileData();
                tile.Title = "";
                PrimaryTile.Update(tile);
            }*/
            if (App_Settings.Switch_Underlock_Setting == true)
            {
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            if (App_Settings.Switch_Avoidlock_Setting == true)
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            if (App_Settings.Switch_GPX_Setting == true)
            {
                if (My_Isolated_Storage.DirectoryExists("GPX") == false)
                {
                    My_Isolated_Storage.CreateDirectory("GPX");
                }
                else if (My_Isolated_Storage.FileExists("GPX\\Session_en_cours.gpx") == true && App_Settings.Switch_GPX_Reset_Setting == true)
                {
                    My_Isolated_Storage.DeleteFile("GPX\\Session_en_cours.gpx");
                }
                // trouver autre solution pour ne pas bloquer le demarrage si l'user attend trop longtemp
                else if (My_Isolated_Storage.FileExists("GPX\\Session_en_cours.gpx") == true && App_Settings.Switch_GPX_Reset_Setting == false)
                {
                    if (MessageBox.Show("La session en cours n'a pas été réinitialisée, voulez-vous le faire maintenant ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        My_Isolated_Storage.DeleteFile("GPX\\Session_en_cours.gpx");
                    }
                }
            }
            if (App_Settings.Switch_Autostart_Setting == true)
            {
                GeoWatcherStart();
                Switch_Start.IsChecked = true;
                Switch_Start.Content = "Activée";
            }
            textBlock2.MouseLeftButtonDown += new MouseButtonEventHandler(Click_GPX_Now);
            //if (My_Isolated_Storage.FileExists("Session_en_cours.gpx"))
            //{
            //    My_Isolated_Storage.MoveFile("Session_en_cours.gpx", "GPX_" + DateTime.Today.Year + "-" + DateTime.Today.Month + "-" + DateTime.Today.Day);
            //}
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            AppReportingService.Flurry_Page_Init();
            AppReportingService.Email_Check_Previous_Excepetion();
        }

        private void Click_Start(object sender, RoutedEventArgs e)
        {
            GeoWatcherStart();
            Switch_Start.Content = "Activée";
        }
        private void Click_Stop(object sender, RoutedEventArgs e)
        {
            Geo_Watcher.Stop();
            textBox1.Text = "";
            Switch_Start.Content = "Désactivée";
        }
        private void Click_Email(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                Email_Chooser_Task = new EmailAddressChooserTask();
                Email_Chooser_Task.Completed += new EventHandler<EmailResult>(TaskChooserEmailCompleted);
                try
                {
                    Email_Chooser_Task.Show();
                }
                catch (System.InvalidOperationException)
                {
                    MessageBox.Show("Une erreur est survenue.");
                }
            }
            else
            {
                MessageBox.Show("La localisation n'est pas démarrée !");
            }
        }
        private void Click_SMS(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                Phone_Number_Chooser_Task = new PhoneNumberChooserTask();
                Phone_Number_Chooser_Task.Completed += new EventHandler<PhoneNumberResult>(TaskChooserPhoneCompleted);
                try
                {
                    Phone_Number_Chooser_Task.Show();
                }
                catch (System.InvalidOperationException)
                {
                    MessageBox.Show("Une erreur est survenue.");
                }
            }
            else 
            { 
                MessageBox.Show("La localisation n'est pas démarrée !"); 
            }
        }
        private void Click_Map(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable() == true)
            {
                NavigationService.Navigate(new Uri("/BingMaps.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show("Connexion de données non disponible !");
            }
        }
        private void Click_Settings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPivot.xaml", UriKind.Relative));
        }
/*        public static string GetProductId()
        {
            System.Xml.Linq.XElement xml = System.Xml.Linq.XElement.Load("WMAppManifest.xml");
            var appElement = (from manifest in xml.Descendants("App") select manifest).SingleOrDefault();
            if (appElement != null)
            {
                return appElement.Attribute("ProductID").Value;
            }
            return string.Empty;
        }
        private void Click_Buy(object sender, EventArgs e)
        {
            //var product_id = GetProductId();
            //MessageBox.Show(product_id);
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            //marketplaceDetailTask.ContentIdentifier = "16751342-68ff-4c91-929d-9cc534cd9993";
            //marketplaceDetailTask.ContentIdentifier = product_id;
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
            marketplaceDetailTask.Show();

        }
*/
        private void Click_About(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }
        private void Click_Marketplace_Review(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }
        private void Click_Compas(object sender, EventArgs e)
        {
            if (!Compass.IsSupported)
            {
                MessageBox.Show(" Ce téléphone ne dispose pas de bousolle !");
                //NavigationService.GoBack();   // A DESACTIVER POUR SIMULATEUR
            }
            else
            {
                NavigationService.Navigate(new Uri("/Compas.xaml", UriKind.Relative));
            }
        }
        private void Click_GPX(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/GPX.xaml", UriKind.Relative));
        }
        private void Click_GPX_Now(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/GPX_View.xaml?FileName=Session_en_cours.gpx", UriKind.Relative));
        }
        private void Click_Skydrive(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable() == true)
            {
                NavigationService.Navigate(new Uri("/Skydrive.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show("Connexion de données non disponible !");
            }
        }
        // Timers et watchers
        private void GeoWatcherStart()
        {
            if (App_Settings.First_Launch == true)
            {
                MessageBox.Show("Explo GPS utilise votre position pour localiser votre appareil automatiquement.\nLes données de localisation sont stockées dans votre téléphone seulement si vous décidez d'enregistrer vos parcours de manière permanente.\nCes informations ne sont pas utilisées pour vous identifier et ne contiennent aucunes données personnelles.\nDans tous les cas, ces données ne seront pas tranmises à des fins commerciales et à des tiers sans votre autorisation.\nVous pouvez toujours, si vous le souhaitez, désactiver la fonction d'enregistrement et de localisation dans les paramètres.\n\nVous pouvez retrouver ces informations sur la page \"A propos\" à l'onglet \"Vie privée\"");
                App_Settings.First_Launch = false;
            }
            //Debug.WriteLine("GeoWatcherSart()");
            try
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
            catch (Exception e)
            {
                //Debug.WriteLine("Geo_Watcher Exception: " + e.Message);
                textBox1.Text = e.Message;
            }
        }
        private void GeoWatcherPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => GeoMyPositionChanged(e));
        }
        private void GeoMyPositionChanged(GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            try
            {
                FlurryWP7SDK.Api.SetLocation(e.Position.Location.Latitude, e.Position.Location.Longitude, Convert.ToSingle(e.Position.Location.HorizontalAccuracy));
                //MessageBox.Show(e.Position.Location.Latitude.ToString() + " / " + e.Position.Location.Longitude.ToString() + " / " + Convert.ToSingle(e.Position.Location.HorizontalAccuracy).ToString());
                //Voir si message first start doit avertir utilisation de localisation pour la pub ?
                //adControl1.Latitude = e.Position.Location.Latitude;
                //adControl1.Longitude = e.Position.Location.Longitude;
                coord_brut_lat = e.Position.Location.Latitude;
                coord_brut_lon = e.Position.Location.Longitude;
                coord_brut_acc_hor = e.Position.Location.HorizontalAccuracy;
                coord_brut_acc_ver = e.Position.Location.VerticalAccuracy;
                coord_brut_alt = e.Position.Location.Altitude;
                coord_brut_speed = e.Position.Location.Speed;
                coord_brut_cap = e.Position.Location.Course;

                /*if (coord_brut_acc_hor == null)
                {
                    MessageBox.Show("null");
                }
                else
                {
                    //FlurryWP7SDK.Api.SetLocation(coord_brut_alt, coord_brut_lon, (float)coord_brut_acc_ver);
                    MessageBox.Show("null");
                }*/
                //MessageBox.Show(coord_brut_lat.ToString() + " / " + coord_brut_lon.ToString() + " / " + coord_brut_acc_hor.ToString());
                //MessageBox.Show(Convert.ToSingle(e.Position.Location.HorizontalAccuracy).ToString());
                //FlurryWP7SDK.Api.SetLocation(coord_brut_alt, coord_brut_lon, (float)coord_brut_acc_hor);

                if (App_Settings.Switch_Decimal_Setting == false)
                {
                    double coord_dms_lat_degres = coord_brut_lat;
                    double coord_dms_lat_minutes = (coord_dms_lat_degres - (int)coord_dms_lat_degres) * 60;
                    double coord_dms_lat_secondes = (coord_dms_lat_minutes - (int)coord_dms_lat_minutes) * 60;
                    //textBox1.Text = "Latitude : " + (int)coord_dms_lat_degres + "° " + (int)coord_dms_lat_minutes + "m " + (int)coord_dms_lat_secondes + "s N\n";
                    textBox1.Text = "Latitude : " + (int)coord_dms_lat_degres + "°" + (int)coord_dms_lat_minutes + "'" + coord_dms_lat_secondes.ToString("00") +"\" N\n";
                    double coord_dms_lon_degres = e.Position.Location.Longitude;
                    double coord_dms_lon_minutes = (coord_dms_lon_degres - (int)coord_dms_lon_degres) * 60;
                    double coord_dms_lon_secondes = (coord_dms_lon_minutes - (int)coord_dms_lon_minutes) * 60;
                    //textBox1.Text += "Longitude : " + (int)coord_dms_lon_degres + "° " + (int)coord_dms_lon_minutes + "m " + (int)coord_dms_lon_secondes + "s E\n";
                    textBox1.Text += "Longitude : " + (int)coord_dms_lon_degres + "°" + (int)coord_dms_lon_minutes + "'" + coord_dms_lon_secondes.ToString("00") + "\" E\n";
                }
                else
                {
                    textBox1.Text = "Latitude : " + coord_brut_lat.ToString("0.000000") + "° N\n";
                    textBox1.Text += "Longitude : " + coord_brut_lon.ToString("0.000000") + "° E\n";
                }
                coord_url_lat = coord_brut_lat.ToString("0.000000"); // Replace pour lancement des urls
                coord_url_lat = coord_url_lat.Replace(",", ".");
                coord_url_lat = coord_url_lat.Replace("-", ""); //peut etre pas besoin de retirer le - , verifier avec toutes les urls
                coord_url_lon = coord_brut_lon.ToString("0.000000");
                coord_url_lon = coord_url_lon.Replace(",", ".");
                coord_url_lon = coord_url_lon.Replace("-", "");

                if (App_Settings.Switch_Metric_Setting == false)
                {
                    double coord_us_hor_acc = e.Position.Location.HorizontalAccuracy;
                    coord_us_hor_acc = coord_us_hor_acc * 3.28;
                    textBox1.Text += "Précision horizontale : " + (int)coord_us_hor_acc + " ft\n";

                    double coord_us_alt = e.Position.Location.Altitude;
                    coord_us_alt = coord_us_alt * 3.28;
                    if (e.Position.Location.Altitude.ToString("0") == "Non Numérique") textBox1.Text += "Altitude : Non numérique\n";
                    else textBox1.Text += "Altitude : " + (int)coord_us_alt + " ft\n";

                    double coord_us_ver_acc = e.Position.Location.VerticalAccuracy;
                    coord_us_ver_acc = coord_us_ver_acc * 0.328;
                    if (e.Position.Location.VerticalAccuracy.ToString("0") == "Non Numérique") textBox1.Text += "Précision verticale : Non numérique\n";
                    else textBox1.Text += "Précision verticale : " + coord_us_ver_acc + " fr\n";

                    double coord_us_speed = e.Position.Location.Speed;
                    coord_us_speed = coord_us_speed * 3.6 * 0.621;
                    if (e.Position.Location.Speed.ToString("0") == "Non Numérique") textBox1.Text += "Vitesse : Non numérique\n";
                    else textBox1.Text += "Vitesse : " + coord_us_speed + " mi/h\n";
                }
                else
                {
                    double speed_kmh = e.Position.Location.Speed * 3.6;
                    textBox1.Text += "Précision horizontale : " + e.Position.Location.HorizontalAccuracy.ToString("0") + " m\n";
                    if (e.Position.Location.Altitude.ToString("0") == "Non Numérique") textBox1.Text += "Altitude : Non numérique\n";
                    else textBox1.Text += "Altitude : " + e.Position.Location.Altitude.ToString("0") + " m\n";
                    if (e.Position.Location.VerticalAccuracy.ToString("0") == "Non Numérique") textBox1.Text += "Précision verticale : Non numérique\n";
                    else textBox1.Text += "Précision verticale : " + e.Position.Location.VerticalAccuracy.ToString("0") + " m\n";
                    if (e.Position.Location.Speed.ToString("0") == "Non Numérique") textBox1.Text += "Vitesse : Non numérique\n";
                    else textBox1.Text += "Vitesse : " + speed_kmh.ToString("0") + " km/h\n";
                }
                // ICI FAUT IL CONVERTIR EN DMS SUIVANT SETTINGS ?
                if (e.Position.Location.Course.ToString("0.00") == "Non Numérique") textBox1.Text += "Cap : Non numérique\n \n";
                else textBox1.Text += "Cap : " + e.Position.Location.Course.ToString("0") + " ° N\n \n";
                textBox1.Text += "Heure du relevé : " + DateTime.Now;

                if (App_Settings.Switch_GPX_Setting == true)
                {
                    //Ici mettre les enregistrement dans la liste puis dans le xml
                    GPS_XML_Update();
                    //textBox1.Text = "";
                    //ScrollBox1.Opacity = 0;
                    //MessageBox.Show(ScrollBox1.Content.ToString());
                    //RootListBox.Background.Opacity = 0;
                }

                //Debug.WriteLine("GeoMyPositionChanged() --> End");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue.");
                FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": MainPage GeoMyPositionChanged: " + ex.Message.ToString(), ex);
                FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
            }
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
                        textBox1.Text += "Le service de localisation est désactivé sur cet appareil\n";
                    }
                    else
                    {
                        textBox1.Text += "Le service de localisation ne fonctionne pas sur cet appareil\n";
                    }
                    break;
                case GeoPositionStatus.Initializing:
                    textBox1.Text += "Localisation en cours...\n";
                    //Button_Start.IsEnabled = false;
                    break;
                case GeoPositionStatus.NoData:
                    textBox1.Text += "Données de localisation indisponibles.\n";
                    //Button_Start.IsEnabled = true;
                    break;
                case GeoPositionStatus.Ready:
                    //textBox1.Text += "Données de localisation disponibles.\n";
                    //Button_Start.IsEnabled = true;
                    break;
            }
        }
        // Appels
        private void GPS_XML_Update()
        {
            try
            {
                //if (App_Settings.Switch_GPX_Setting == true)
                //bool gpx_settings = true;
                //if (gpx_settings == true)
                //{

                //if (coord_xml_count == 1)
                if (My_Isolated_Storage.FileExists("GPX\\Session_en_cours.gpx") == false)
                {
                    IsolatedStorageFileStream xml_stream = new IsolatedStorageFileStream("GPX\\Session_en_cours.gpx", FileMode.Create, My_Isolated_Storage);
                    //File introduction :
                    XDocument xml_xdoc = new XDocument(
                        new XDeclaration("1.0", "utf-8", "yes"),
                        new XElement("gpx",
                            new XAttribute("version", "1.0"),
                            new XAttribute("creator", "ExploGPS"),
                            new XElement("trk",
                                new XElement("name", "ExploGPSTrack"),
                                new XElement("desc", "Track description"),
                                    new XElement("trkseg",
                        //Points :
                                        new XElement("trkpt",
                                            new XAttribute("lat", coord_brut_lat),
                                            new XAttribute("lon", coord_brut_lon),
                                            new XElement("ele", coord_brut_alt),
                                            new XElement("time", DateTime.Today.Year.ToString("0000") + "-" + DateTime.Today.Month.ToString("00") + "-" + DateTime.Today.Day.ToString("00") + "T" + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00") + "Z")
                                            )))));
                    xml_xdoc.Save(xml_stream);
                    xml_stream.Flush();
                    xml_stream.Dispose();
                    xml_stream.Close();
                    coord_xml_count = 1;
                    textBlock2.Text = "Session actuelle: " + coord_xml_count + " points";
                    //this.ScrollBox1.Content = xml_xdoc;
                }
                else
                {
                    IsolatedStorageFileStream xml_stream = new IsolatedStorageFileStream("GPX\\Session_en_cours.gpx", FileMode.Open, My_Isolated_Storage);
                    XDocument xml_xdoc = XDocument.Load(xml_stream);
                    xml_stream.Position = 0;
                    XDocument xml_xdoc2 = XDocument.Parse(xml_xdoc.ToString(SaveOptions.DisableFormatting));
                    try
                    {
                        xml_xdoc.Element("gpx").Element("trk").Element("trkseg").Add(new XElement("trkpt",
                                new XAttribute("lat", coord_brut_lat),
                                new XAttribute("lon", coord_brut_lon),
                                new XElement("ele", coord_brut_alt),
                                new XElement("time", DateTime.Today.Year.ToString("0000") + "-" + DateTime.Today.Month.ToString("00") + "-" + DateTime.Today.Day.ToString("00") + "T" + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00") + "Z")
                                ));
                        //this.ScrollBox1.Content = xml_xdoc;
                        xml_xdoc.Save(xml_stream);
                    }
                    catch (NullReferenceException)
                    {
                        MessageBox.Show("Null");
                    }
                    xml_stream.Flush();
                    xml_stream.Dispose();
                    xml_stream.Close();
                    coord_xml_count = coord_xml_count + 1;
                    textBlock2.Text = "Session actuelle: " + coord_xml_count + " points";
                }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue.");
                FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": MainPage GPS_XML_Update: " + ex.Message.ToString(), ex);
                FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
            }
        }
        private void TaskChooserEmailCompleted(object sender, EmailResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                try
                {
                    EmailComposeTask MyEmailComposer = new EmailComposeTask();
                    MyEmailComposer.To = e.Email;
                    MyEmailComposer.Subject = "Explo GPS - Données";
                    MyEmailComposer.Body = textBox1.Text;

                    ////Partie d'export xml
                    //IsolatedStorageFileStream xml_stream = new IsolatedStorageFileStream("GPX\\Session_en_cours.gpx", FileMode.Open, My_Isolated_Storage);
                    ////My_Isolated_Storage.CopyFile("Coords_Data.gpx", "Explo_GPS.settings_gpx");
                    //using (StreamReader reader = new StreamReader(xml_stream))
                    //{
                    //    //this.ScrollBox1.Content = reader.ReadToEnd();
                    //    MyEmailComposer.Body = reader.ReadToEnd();
                    //}
                    //xml_stream.Dispose();
                    //xml_stream.Close();
                    MyEmailComposer.Show();
                }
                catch 
                {
                    MessageBox.Show("Exception pendant la génération de l'email");
                }
            }
        }
        private void TaskChooserPhoneCompleted(object sender, PhoneNumberResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                SmsComposeTask MySmsComposer = new SmsComposeTask();
                MySmsComposer.To = e.PhoneNumber;
                MySmsComposer.Body = textBox1.Text;
                MySmsComposer.Show();
            }
        }
        // Future
/**        private void Click_Url_BMap(object sender, EventArgs e)
        {
            string url_bmap;
            url_bmap = "http://www.bing.com/maps/?v=2&cp=" + coord_url_lat + "~" + coord_url_lon + "&lvl=17&dir=0&sty=r&form=LMLTCC";
            MessageBox.Show(url_bmap);
            Web_Browser_Task.Uri = new Uri(url_bmap, UriKind.RelativeOrAbsolute);
            Web_Browser_Task.Show();
        }
        private void Click_Url_GMap(object sender, EventArgs e)
        {
            string url_gmap;
            url_gmap = "http://maps.google.com/maps?ll=" + coord_url_lat + "," + coord_url_lon;
            MessageBox.Show(url_gmap);
            Web_Browser_Task.Uri = new Uri(url_gmap, UriKind.RelativeOrAbsolute);
            Web_Browser_Task.Show();
        }
        private void Click_Url_OSMap(object sender, EventArgs e)
        {
            string url_osmap;
            url_osmap = "http://www.openstreetmap.org/?lat=" + coord_url_lat + "&lon=" + coord_url_lon + "&zoom=17&layers=M";
            MessageBox.Show(url_osmap);
            Web_Browser_Task.Uri = new Uri(url_osmap, UriKind.RelativeOrAbsolute);
            Web_Browser_Task.Show();
        }
 **/


        /*private void ConvertLatLongToUTM()
        {
            //Read the inputs
            double sLong = 7.146245;
            double sLat = 46.030260;

            GeoUTMConverter UTM_Converter = new GeoUTMConverter();
            UTM_Converter.ToUTM(sLat, sLong);
        }*/

    }
}