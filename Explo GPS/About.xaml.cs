using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;
using System.Text.RegularExpressions;
using System.Device.Location;


namespace Explo_GPS
{
    public partial class About : PhoneApplicationPage
    {
        public class RSSClass
        {
            public string Title { get; set; }
            public string PubDate { get; set; }
            public string Content { get; set; }
            public string Author { get; set; }
            public string Link { get; set; }
        }

        public About()
        {
            InitializeComponent();
            Get_FB_RSS();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            AppReportingService.Flurry_Page_Init();
        }

        public void Click_Back(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
/*        public void Click_Buy(object sender, EventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.ContentIdentifier = "16751342-68ff-4c91-929d-9cc534cd9993";
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
            marketplaceDetailTask.Show();
        }
*/
        public void Click_Email(object sender, EventArgs e)
        {
            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = "my@email.com";
            emailcomposer.Subject = "Contact depuis Explo GPS";
            emailcomposer.Body = "";
            emailcomposer.Show();
        }
        private void Click_Site(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webbrowser = new WebBrowserTask();
            webbrowser.Uri = new Uri("http://www.site.com", UriKind.Absolute);
            webbrowser.Show();
        }
        private void Click_Facebook(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webbrowser = new WebBrowserTask();
            webbrowser.Uri = new Uri("http://www.facebook.com/ExploGPS", UriKind.Absolute);
            webbrowser.Show();
        }
        private void Click_Google(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webbrowser = new WebBrowserTask();
            webbrowser.Uri = new Uri("http://plus.google.com/u/0/b/12452586439442491202/124522864092476421602", UriKind.Absolute);
            webbrowser.Show();
        }
        public void Click_Marketplace(object sender, EventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            // marketplaceDetailTask.ContentIdentifier = "16751342-68ff-4c91-929d-9cc534cd9993";
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
            marketplaceDetailTask.Show();
        }
        public void Click_Marketplace_Review(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }
        private void Click_Debug(object sender, EventArgs e)
        {
                try
                {
                    string deviceManufacturer = DeviceStatus.DeviceManufacturer;
                    string deviceName = DeviceStatus.DeviceName;
                    string deviceHardwareVersion = DeviceStatus.DeviceHardwareVersion;
                    string deviceFirmwareVersion = DeviceStatus.DeviceFirmwareVersion;
                    string OSVersion = Environment.OSVersion.Version.ToString();
                    long deviceTotalMemory = DeviceStatus.DeviceTotalMemory;
                    long applicationCurrentMemoryUsage = DeviceStatus.ApplicationCurrentMemoryUsage;
                    long applicationPeakMemoryUsage = DeviceStatus.ApplicationPeakMemoryUsage;
                    //bool isKeyboardDeployed = DeviceStatus.IsKeyboardDeployed; 
                    //bool isKeyboardPresent = DeviceStatus.IsKeyboardPresent; 
                    //PowerSource powerSource = DeviceStatus.PowerSource;
                    //MessageBox.Show("Fabricant: " + deviceManufacturer + "\n\rNom: " + deviceName + "\n\rHVersion:" + deviceHardwareVersion + "\n\rFVersion: " + deviceFirmwareVersion + "\n\rOS: " + OSVersion + "\n\rTotal Mem: " + (deviceTotalMemory / 1048576) + "Mo\rApp Mem: " + (applicationCurrentMemoryUsage / 1048576) + "Mo\nPeak Mem: " + (applicationPeakMemoryUsage / 1048576) + "Mo"); 
                    EmailComposeTask MyEmailComposer = new EmailComposeTask();
                    MyEmailComposer.To = "my@email.com";
                    MyEmailComposer.Subject = "Explo GPS - Debug informations";
                    MyEmailComposer.Body = "Constructeur: " + deviceManufacturer + "\nModèle: " + deviceName + "\nH/W version: " + deviceHardwareVersion + "\nF/W Version: " + deviceFirmwareVersion + "\nOS: Windows Phone " + OSVersion + "\nTotal Mem: " + (deviceTotalMemory / 1048576) + "Mo\nApp Mem: " + (applicationCurrentMemoryUsage / 1048576) + "Mo\nPeak Mem: " + (applicationPeakMemoryUsage / 1048576) + "Mo";
                    MyEmailComposer.Show();
                }
                catch
                {
                    MessageBox.Show("Exception pendant la génération de l'email");
                }
        }
        private void Click_MS_WL(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webbrowser = new WebBrowserTask();
            webbrowser.Uri = new Uri("http://windows.microsoft.com/fr-FR/windows-live/microsoft-service-agreement", UriKind.Absolute);
            webbrowser.Show();
        }
        private void Click_MS_WP(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webbrowser = new WebBrowserTask();
            webbrowser.Uri = new Uri("http://www.microsoft.com/windowsphone/fr-FR/privacy.aspx", UriKind.Absolute);
            webbrowser.Show();
        }
        private void Get_FB_RSS()
        {
            if (NetworkInterface.GetIsNetworkAvailable() == true)
            {
                try
                {
                    WebClient client = new WebClient();
                    client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Get_FB_RSS_Completed);
                    client.DownloadStringAsync(new Uri(@"http://www.facebook.com/feeds/page.php?id=281818225250015&format=rss20"));
                    //client.DownloadStringAsync(new Uri(@"http://www.facebook.com/feeds/page.php?id=281818225250015&format=atom10"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue.");
                    FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": About Get_FB_RSS: " + ex.Message.ToString(), ex);
                    FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
                }
            }
            else
            {
                TextBlock_Error.Visibility = Visibility.Visible;
                TextBlock_Error.Text = "Erreur lors du téléchargement des données: Aucune connexion n'est disponible.";
            }
        }
        private void Get_FB_RSS_Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    var RSS_Data = from RSS in XElement.Parse(e.Result).Descendants("item")
                                   select new RSSClass
                                   {
                                       Title = RSS.Element("title").Value,
                                       PubDate = RSS.Element("pubDate").Value,
                                       //Content = RSS.Element("description").Value,
                                       //Content = HttpUtility.HtmlDecode(Regex.Replace(RSS.Element("description").Value.ToString(), "<[^>]+>", string.Empty).Replace("\r", "&#10;").Replace("\n", "&#10;")),
                                       //Content = HttpUtility.HtmlDecode(Regex.Replace(RSS.Element("description").Value.ToString(), "<[^>]+>", string.Empty).Replace("\r", "&#xD;").Replace("\n", "&#xD;")),
                                       //Content = HttpUtility.HtmlDecode(Regex.Replace(RSS.Element("description").Value.ToString(), "<[^>]+>", string.Empty).Replace("\r", "&#x0a;").Replace("\n", "&#x0a;")),
                                       //Content = HttpUtility.HtmlDecode(Regex.Replace(RSS.Element("description").Value.ToString(), "<[^>]+>", string.Empty).Replace("br", "&#xD;").Replace("\n", "&#x0a;")),
                                       Content = HttpUtility.HtmlDecode(RSS.Element("description").Value.ToString().Replace("<br />", "&#xD;")),
                                       Author = RSS.Element("author").Value,
                                       Link = RSS.Element("link").Value.ToString().Replace("www", "m")
                                   };
                    TextBlock_Error.Visibility = Visibility.Collapsed;
                    ListBox_RSS.ItemsSource = RSS_Data;
                }
                else
                {
                    TextBlock_Error.Text = "Erreur lors du téléchargement des données: " + e.Error.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue.");
                FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": About Get_FB_RSS_Completed: " + ex.Message.ToString(), ex);
                FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
            }
        }
        private void ListBox_RSS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != sender && sender is ListBox)
            {
                ListBox lv = sender as ListBox;
                lv.SelectedIndex = -1;
            } 
        }
    /*
    // Remove HTML tags and newline characters from the text, and decode HTML encoded characters. 
    // This is a basic method. Additional code would be needed to more thoroughly  
    // remove certain elements, such as embedded Javascript. 

    // Remove HTML tags. 
    fixedString = Regex.Replace(value.ToString(), "<[^>]+>", string.Empty);

    // Remove newline characters.
    fixedString = fixedString.Replace("\r", "").Replace("\n", "");

    // Remove encoded HTML characters.
    fixedString = HttpUtility.HtmlDecode(fixedString);

    strLength = fixedString.ToString().Length;
    */
    }
}