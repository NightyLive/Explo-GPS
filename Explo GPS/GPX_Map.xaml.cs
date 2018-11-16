using System;
using System.Device.Location;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;

namespace Explo_GPS
{
    public partial class GPX_Map : PhoneApplicationPage
    {
        IsolatedStorageFile appIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
        string currentFileNamePath;
        string currentFileName; 
        bool mode_road = true;
        LocationCollection locationsList = new LocationCollection();
        double xml_lat = 0;
        double xml_lon = 0;
        double xml_alt = 0;

        public GPX_Map()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            FlurryWP7SDK.Api.LogEvent("GPX_Map");
            FlurryWP7SDK.Api.LogPageView();
            base.OnNavigatedTo(e);
            currentFileName = NavigationContext.QueryString["FileName"];
            currentFileNamePath = "GPX\\" + NavigationContext.QueryString["FileName"];
            //this.actualFileName.Text = NavigationContext.QueryString["FileName"];
            this.actualFileName.Text = currentFileName;
            this.ReadFileData(currentFileNamePath);
        }

        private void Click_Back(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
        private void Click_FocusMe(object sender, EventArgs e)
        {
            try
            {
                map1.Center = new GeoCoordinate(xml_lat, xml_lon);
                map1.ZoomLevel = 14;
            }
            catch
            {
            }
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
        private void ReadFileData(string filePath)
        {
            if (appIsolatedStorage.FileExists(filePath))
            {
                try
                {
                    using (IsolatedStorageFileStream fileStream = appIsolatedStorage.OpenFile(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            var xml_content = reader.ReadToEnd();
                            using (XmlReader reader2 = XmlReader.Create(new StringReader(xml_content)))
                            {
                                // faudra essayer de coupler les 3 using ensemble...

                                while (reader2.Read())
                                {
                                    reader2.ReadToFollowing("trkpt");
                                    reader2.MoveToFirstAttribute();
                                    if ((reader2.NodeType != XmlNodeType.None))// & (reader.NodeType != XmlNodeType.EndElement) & (!reader.IsEmptyElement))
                                    {
                                        xml_lat = Convert.ToDouble(reader2.Value.Replace(".", ","));
                                        reader2.MoveToNextAttribute();
                                        xml_lon = Convert.ToDouble(reader2.Value.Replace(".", ","));
                                        locationsList.Add(new GeoCoordinate(xml_lat, xml_lon, xml_alt));
                                        //MessageBox.Show("Lat:" + xml_lat + " Lon:" + xml_lon);
                                    }
                                }
                            }
                            //MessageBox.Show("Generation polyline");
                            MapPolyline polyline = new MapPolyline();
                            polyline.Stroke = new SolidColorBrush(Colors.Red);
                            polyline.StrokeThickness = 5;
                            polyline.Opacity = 0.7;
                            polyline.Locations = locationsList; //recuperation de la liste
                            map1.Children.Add(polyline);
                            map1.Center = new GeoCoordinate(xml_lat, xml_lon);
                            map1.ZoomLevel = 14;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Erreur de lecture GPX !");
                }
            }
        }
    }
}