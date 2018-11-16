using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;


namespace Explo_GPS
{
    public partial class SettingsPivot : PhoneApplicationPage
    {
        private AppSettings settings = new AppSettings();

        public SettingsPivot()
        {
            InitializeComponent();
            Switch_Autostart.Click += new EventHandler<RoutedEventArgs>(Switch_Autostart_Click);
            Switch_Follow.Click += new EventHandler<RoutedEventArgs>(Switch_Follow_Click);
            Switch_Roadmode.Click += new EventHandler<RoutedEventArgs>(Switch_Roadmode_Click);
            Switch_AutostartMap.Click += new EventHandler<RoutedEventArgs>(Switch_AutostartMap_Click);
            Switch_AutostartCompas.Click += new EventHandler<RoutedEventArgs>(Switch_AutostartCompas_Click);
            Switch_AutostartCamera.Click += new EventHandler<RoutedEventArgs>(Switch_AutostartCamera_Click);
            Switch_Decimal.Click += new EventHandler<RoutedEventArgs>(Switch_Decimal_Click);
            Switch_Metric.Click += new EventHandler<RoutedEventArgs>(Switch_Metric_Click);
            Switch_GPX.Click += new EventHandler<RoutedEventArgs>(Switch_GPX_Click);
            Switch_GPX_Reset.Click += new EventHandler<RoutedEventArgs>(Switch_GPX_Reset_Click);
            Switch_Underlock.Click += new EventHandler<RoutedEventArgs>(Switch_Underlock_Click);
            Switch_Avoidlock.Click += new EventHandler<RoutedEventArgs>(Switch_Avoidlock_Click);
            Switch_GPS_Sensibility.Click += new EventHandler<RoutedEventArgs>(Switch_GPS_Sensibility_Click);
            //GPS_Treshold_List.ItemsSource = new List<string>() { "1", "5", "10", "25", "50", "100", "250", "500" };
            GPS_Treshold_List.ItemsSource = new List<string>() { "5", "10", "25", "50", "100" };
            //limite acceptable gpx de 1mo donc environ 14000points -> 5m ca fait 70km de relevés, 100m ca fait 1400km de relevés -> 1m : 14km
            GPS_Treshold_List.SelectedItem = settings.GPS_Threshold;
            GPS_Treshold_List.SelectionChanged += new SelectionChangedEventHandler(GPS_Treshold_List_SelectionChanged);

            if (settings.Switch_Autostart_Setting == true)
            {
                Switch_Autostart.Content = "Activée";
            }
            if (settings.Switch_Follow_Setting == true)
            {
                Switch_Follow.Content = "Activé";
            }
            if (settings.Switch_Roadmode_Setting == true)
            {
                Switch_Roadmode.Content = "Route";
            }
            if (settings.Switch_AutostartMap_Setting == true)
            {
                Switch_AutostartMap.Content = "Activée";
            }
            if (settings.Switch_AutostartCompas_Setting == true)
            {
                Switch_AutostartCompas.Content = "Oui";
            }
            if (settings.Switch_AutostartCamera_Setting == true)
            {
                Switch_AutostartCamera.Content = "Oui";
            }
            if (settings.Switch_Decimal_Setting == true)
            {
                Switch_Decimal.Content = "Décimal";
            }
            if (settings.Switch_Metric_Setting == true)
            {
                Switch_Metric.Content = "Métrique";
            }
            if (settings.Switch_GPX_Setting == true)
            {
                Switch_GPX.Content = "Activé";
            }
            if (settings.Switch_GPX_Reset_Setting == true)
            {
                Switch_GPX_Reset.Content = "Activé";
            }
            if (settings.Switch_Underlock_Setting == true)
            {
                Switch_Underlock.Content = "Activé";
            }
            if (settings.Switch_Avoidlock_Setting == true)
            {
                Switch_Avoidlock.Content = "Activé";
            }
            if (settings.Switch_GPS_Sensibility_Setting == true)
            {
                Switch_GPS_Sensibility.Content = "Haute";
            }
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            AppReportingService.Flurry_Page_Init();
        }

        void Switch_Autostart_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_Autostart_Setting == true)
            {
                Switch_Autostart.Content = "Activée";
            }
            else
            {
                Switch_Autostart.Content = "Désactivée";
            }
        }
        void Switch_Follow_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_Follow_Setting == true)
            {
                Switch_Follow.Content = "Activé";
            }
            else
            {
                Switch_Follow.Content = "Désactivé";
            }
        }
        void Switch_Roadmode_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_Roadmode_Setting == true)
            {
                Switch_Roadmode.Content = "Route";
            }
            else
            {
                Switch_Roadmode.Content = "Satellite";
            }
        }
        void Switch_AutostartMap_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_AutostartMap_Setting == true)
            {
                Switch_AutostartMap.Content = "Activée";
            }
            else
            {
                Switch_AutostartMap.Content = "Désactivée";
            }
        }
        void Switch_AutostartCompas_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_AutostartCompas_Setting == true)
            {
                Switch_AutostartCompas.Content = "Oui";
            }
            else
            {
                Switch_AutostartCompas.Content = "Non";
            }
        }
        void Switch_AutostartCamera_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_AutostartCamera_Setting == true)
            {
                Switch_AutostartCamera.Content = "Oui";
            }
            else
            {
                Switch_AutostartCamera.Content = "Non";
            }
        }
        void Switch_Decimal_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_Decimal_Setting == true)
            {
                Switch_Decimal.Content = "Décimal";
            }
            else
            {
                Switch_Decimal.Content = "DMS";
            }
        }
        void Switch_Metric_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_Metric_Setting == true)
            {
                Switch_Metric.Content = "Métrique";
            }
            else
            {
                Switch_Metric.Content = "US";
            }
        }
        void Switch_GPX_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_GPX_Setting == true)
            {
                Switch_GPX.Content = "Activé";
            }
            else
            {
                Switch_GPX.Content = "Désactivé";
            }
        }
        void Switch_GPX_Reset_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_GPX_Reset_Setting == true)
            {
                Switch_GPX_Reset.Content = "Activé";
            }
            else
            {
                Switch_GPX_Reset.Content = "Désactivé";
            }
        }
        void Switch_Underlock_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_Underlock_Setting == true)
            {
                Switch_Underlock.Content = "Activé";
            }
            else
            {
                Switch_Underlock.Content = "Désactivé";
            }
            MessageBox.Show("L'application doit être redémarrée pour appliquer ce paramètre");
        }
        void Switch_Avoidlock_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_Avoidlock_Setting == true)
            {
                Switch_Avoidlock.Content = "Activé";
            }
            else
            {
                Switch_Avoidlock.Content = "Désactivé";
            }
            MessageBox.Show("L'application doit être redémarrée pour appliquer ce paramètre");
        }
        void Switch_GPS_Sensibility_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Switch_GPS_Sensibility_Setting == true)
            {
                Switch_GPS_Sensibility.Content = "Haute";
            }
            else
            {
                Switch_GPS_Sensibility.Content = "Basse";
            }
        }
        void GPS_Treshold_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.GPS_Threshold = GPS_Treshold_List.SelectedItem.ToString();
            //MessageBox.Show("e : " + e.ToString());
            //MessageBox.Show("selecteditem : " + GPS_Treshold_List.SelectedItem.ToString());
        }
        //void Button_Reset_Click(object sender, RoutedEventArgs e)
        void Button_Reset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment remettre les paramètres par défaut ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
            settings.GPS_Threshold = "25";
            settings.Switch_Autostart_Setting = true;
            settings.Switch_Follow_Setting = true;
            settings.Switch_Roadmode_Setting = true;
            settings.Switch_AutostartMap_Setting = true;
            settings.Switch_AutostartCompas_Setting = true;
            settings.Switch_AutostartCamera_Setting = false;
            settings.Switch_Decimal_Setting = true;
            settings.Switch_Metric_Setting = true;
            settings.Switch_GPX_Setting = true;
            settings.Switch_GPX_Reset_Setting = true;
            settings.Switch_Underlock_Setting = true;
            settings.Switch_Avoidlock_Setting = false;
            settings.Switch_GPS_Sensibility_Setting = true;
            settings.First_Launch = true;

            MessageBox.Show("Paramètres par défaut appliqués !");
            NavigationService.GoBack();
            }
        }
        void Click_Back(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}