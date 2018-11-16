using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;


namespace Explo_GPS
{
    public partial class GPX : PhoneApplicationPage
    {
        IsolatedStorageFile My_Isolated_Storage = IsolatedStorageFile.GetUserStoreForApplication();
        string[] directoryFiles;

        public GPX()
        {
            InitializeComponent();
            Loaded += Content_Refresh;
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            AppReportingService.Flurry_Page_Init();
        }

        private void Click_Back(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
        private void Click_Delete(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment effacer tous les fichiers ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    directoryFiles = My_Isolated_Storage.GetFileNames("GPX\\*.gpx");
                    foreach (string file in directoryFiles)
                    {
                        My_Isolated_Storage.DeleteFile("GPX\\" + file);
                    }
                    directoryFiles = My_Isolated_Storage.GetFileNames("GPX\\*.gpx");
                    this.RootListBox.ItemsSource = directoryFiles;
                    //MessageBox.Show("Tout les fichiers on été effacés");
                }
                catch
                {
                    MessageBox.Show("Le répertoire n'a pas pu être vidé !");
                }
            }
        }
        private void Click_Delete_Current(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment effacer la session en cours ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    My_Isolated_Storage.DeleteFile("GPX\\Session_en_cours.gpx");
                    directoryFiles = My_Isolated_Storage.GetFileNames("GPX\\*.gpx");
                    this.RootListBox.ItemsSource = directoryFiles;
                    //MessageBox.Show("La session en cours à été effacée");
                }
                catch
                {
                    MessageBox.Show("La session en cours n'a pas pu être effacé !");
                }
            }
        }
        private void Content_Refresh(object sender, EventArgs e)
        {
            directoryFiles = My_Isolated_Storage.GetFileNames("GPX\\*.gpx");
            this.RootListBox.ItemsSource = directoryFiles;
            textBlock2.Text = "Espace restant: " + (My_Isolated_Storage.AvailableFreeSpace / 1073741824).ToString("0.00") + "Go";
        }
        private void RootListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.RootListBox.SelectedItem != null)
            {
                string selectedFileName = this.RootListBox.SelectedItem.ToString();
                //MessageBox.Show("Selected file : " + list_select_name);
                this.NavigationService.Navigate(new Uri("/GPX_View.xaml?FileName=" + selectedFileName, UriKind.Relative));

                this.RootListBox.SelectedItem = null;
            }
        } 
    }
}
