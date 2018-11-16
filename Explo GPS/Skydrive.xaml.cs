using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Live;
using Microsoft.Live.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;


namespace Explo_GPS
{
    public partial class Skydrive : PhoneApplicationPage
    {
        public class SkyDriveContent
        {
            public string Name
            {
                get;
                set;
            }
            public string Description
            {
                get;
                set;
            }
            public string Id
            {
                get;
                set;
            }
        }
        IsolatedStorageFile My_Isolated_Storage = IsolatedStorageFile.GetUserStoreForApplication();
        List<SkyDriveContent> list_files_skydrive;
        private LiveConnectClient client;
        private LiveConnectSession session;
        string[] list_files_isolated;
        string sky_base_folder = "Explo GPS";
        string sky_base_folder_id = null;
        int list_select_index;
        string list_select_name = null;
        string list_select_id = null;
        bool wait_async = false;
        List<int> progress_list = new List<int>();
        bool progress_multiple = false;
        ProgressIndicator progress_indicator = new ProgressIndicator() { IsVisible = false, IsIndeterminate = false, Text = "Opération en cours..." };
 

        public Skydrive()
        {
            InitializeComponent();
            SystemTray.SetProgressIndicator(this, progress_indicator);
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            AppReportingService.Flurry_Page_Init();
        }

        private void Signin_Get_Session(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                session = e.Session;
                client = new LiveConnectClient(session);
                textBlockStatus.Text = "Informations...";
                client.GetCompleted += Signin_Get_Infos;
                client.GetAsync("me", null);
                client.DownloadCompleted += new EventHandler<LiveDownloadCompletedEventArgs>(Signin_Get_Picture);
                client.DownloadAsync("me/picture");
            }
            else
            {
                textBlockStatus.Text = "Déconnecté";
                ListBox_SkyContent.ItemsSource = "";
                client = null;
                session = null;
                sky_base_folder_id = null;
            }
        }
        private void Signin_Get_Picture(object sender, LiveDownloadCompletedEventArgs e)
        {
            client.DownloadCompleted -= Signin_Get_Picture;
            if (e.Result != null)
            {
                try
                {
                    ProfilePicture.Visibility = Visibility.Visible;
                    BitmapImage imgSource = new BitmapImage();
                    imgSource.SetSource(e.Result);
                    ProfilePicture.Source = imgSource;
                    e.Result.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue.");
                    FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": Skydrive Signin_Get_Picture: " + ex.Message.ToString(), ex);
                    FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
                }
            }
            else
            {
                MessageBox.Show("Erreur image profil: " + e.Error.ToString());
            }
        }
        private void Signin_Get_Infos(object sender, LiveOperationCompletedEventArgs e) 
        {
            client.GetCompleted -= Signin_Get_Infos; 
            if (e.Error == null) 
            {
                string firstName = e.Result.ContainsKey("first_name") ? e.Result["first_name"] as string : string.Empty; 
                string lastName = e.Result.ContainsKey("last_name") ? e.Result["last_name"] as string : string.Empty;
                textBlockName.Text = firstName + " " + lastName;
            } 
            else 
            {
                MessageBox.Show("Erreur informations profil: " + e.Error.ToString()); 
            }
            textBlockStatus.Text = "Quota...";
            client.GetCompleted += Signin_Get_Quota;
            client.GetAsync("/me/skydrive/quota", null);
        }
        private void Signin_Get_Quota(object sender, LiveOperationCompletedEventArgs e)
        {
            client.GetCompleted -= Signin_Get_Quota;
            if (e.Error == null)
            {
                double quota_tot = Convert.ToDouble(e.Result.ContainsKey("quota") ? e.Result["quota"].ToString() as string : string.Empty);
                double quota_use = Convert.ToDouble(e.Result.ContainsKey("available") ? e.Result["available"].ToString() as string : string.Empty);
                textBlockQuota.Text = "Quota: " + (quota_use / 1073741824).ToString("0.00") + "/" + (quota_tot / 1073741824) + "Go";
            }
            else
            {
                MessageBox.Show("Erreur quota" + e.Error.ToString());
            }
            textBlockStatus.Text = "Dossiers...";
            client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Signin_BaseFolder);
            client.GetAsync("/me/skydrive/files");
        }
        private void Signin_BaseFolder(object sender, LiveOperationCompletedEventArgs e)
        {
            client.GetCompleted -= Signin_BaseFolder; 
            if (e.Result == null)
            {
                MessageBox.Show("Erreur lecture dossiers: " + e.Error.ToString());
                return;
            }
            List<object> entries;
            Dictionary<string, object> properties;
            entries = (List<object>)e.Result["data"];
            foreach (object entry in entries)
            {
                properties = (Dictionary<string, object>)entry;
                if ((((string)properties["name"]) == sky_base_folder) && (((string)properties["type"]) == "folder"))
                {
                    sky_base_folder_id = (string)properties["id"];
                    textBlockStatus.Text = "Fichiers...";
                    client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Show_Files_Completed);
                    client.GetAsync(sky_base_folder_id + "/files");
                }
            }
            if (sky_base_folder_id == null)
            {// create repertoire + recup ID
                Dictionary<string, object> folderData = new Dictionary<string, object>();
                folderData.Add("name", sky_base_folder);
                client.PostCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Signin_BaseFolder_Completed);
                client.PostAsync("me/skydrive", folderData);
            }
        }
        private void Signin_BaseFolder_Completed(object sender, LiveOperationCompletedEventArgs e)
        {
            client.GetCompleted -= Signin_BaseFolder_Completed; 
            if (e.Result == null)
            {
                MessageBox.Show("Erreur creation dossiers: " + e.Error.ToString());
                return;
            }
            sky_base_folder_id = e.Result.ContainsKey("id") ? e.Result["id"] as string : string.Empty;
            textBlockStatus.Text = "Fichiers...";
            client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Show_Files_Completed);
            client.GetAsync(sky_base_folder_id + "/files");
        }
        
        void Progress_Init()
        {
            SystemTray.ProgressIndicator.Value = 0;
            SystemTray.ProgressIndicator.IsVisible = true;
            if (progress_multiple == true)
            {
                progress_list.Clear();
                client.UploadProgressChanged += new EventHandler<LiveUploadProgressChangedEventArgs>(Progress_Update);
            }
            else
            {
                client.UploadProgressChanged += new EventHandler<LiveUploadProgressChangedEventArgs>(Progress_Update);
            }
        }
        public void Progress_Update(object sender, LiveUploadProgressChangedEventArgs e)
        {
            if (progress_multiple == true)
            {
                progress_list[(int)e.UserState] = e.ProgressPercentage;
                progress_indicator.Value = progress_list.Average() / 100;
            }
            else
            {
                progress_indicator.Value = e.ProgressPercentage / 100;
            }
        }

        private void File_Select_Click(object sender, MouseButtonEventArgs e)
        {
            if (ListBox_SkyContent.SelectedItem != null)
            {
                list_select_index = ListBox_SkyContent.SelectedIndex;
                list_select_name = list_files_skydrive.ElementAt(list_select_index).Name.ToString();
                list_select_id = list_files_skydrive.ElementAt(list_select_index).Id.ToString();
            }
        } 
        private void Show_Files_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Veuillez vous connecter !");
            }
            else
            {
                if (wait_async == false)
                {
                    wait_async = true;
                    client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Show_Files_Completed);
                    client.GetAsync(sky_base_folder_id + "/files");
                }
            }
        }
        void Show_Files_Completed(object sender, LiveOperationCompletedEventArgs e)
        {
            wait_async = false;
            client.GetCompleted -= Show_Files_Completed;
            if (e.Error == null)
            {
                list_files_skydrive = new List<SkyDriveContent>();
                List<object> data = (List<object>)e.Result["data"];
                foreach (IDictionary<string, object> content in data)
                {
                    string test_extension = content["name"].ToString().Substring(content["name"].ToString().Length - 7, 7);
                    if (test_extension == "gpx.txt")
                    {
                        SkyDriveContent skyContent = new SkyDriveContent();
                        skyContent.Name = content["name"].ToString().Substring(0, content["name"].ToString().Length - 4);
                        skyContent.Id = (string)content["id"];
                        list_files_skydrive.Add(skyContent);
                    }
                }
                ListBox_SkyContent.ItemsSource = list_files_skydrive;
                textBlockStatus.Text = "Connecté";
            }
            else
            {
                MessageBox.Show("Erreur liste fichiers :" + e.Error.ToString());
                return;
            }
        }
        void Folder_Upload_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Veuillez vous connecter !");
            }
            else
            {
                Folder_Upload();
            }
        }
        void Folder_Upload()
        {
            if (sky_base_folder_id != null)
            {
                if (wait_async == false)
                {
                    wait_async = true;
                    list_files_isolated = My_Isolated_Storage.GetFileNames("GPX\\*.gpx");
                    progress_multiple = true;
                    Progress_Init();
                    int loop_count = 0;
                    client.UploadCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Folder_Upload_Completed);
                    foreach (string content in list_files_isolated)
                    {
                        int scan_loop = 0;
                        bool scan_file_exist = false;
                        while (scan_loop + 1 <= list_files_skydrive.Count())
                        {
                            if (content == list_files_skydrive.ElementAt(scan_loop).Name.ToString())
                            {
                                scan_file_exist = true;
                                //MessageBox.Show("Deja existant : " + content);
                            }
                            scan_loop += 1;
                        }
                        if (scan_file_exist == false)
                        {
                            progress_list.Add(0);
                            IsolatedStorageFileStream fileStream = My_Isolated_Storage.OpenFile("GPX\\" + content, FileMode.Open, FileAccess.Read);
                            StreamReader reader = new StreamReader(fileStream);
                            client.UploadAsync(sky_base_folder_id, content + ".txt", fileStream, OverwriteOption.DoNotOverwrite, loop_count);
                            loop_count = loop_count + 1;
                        }
                    }
                    if (loop_count == 0)
                    {
                        wait_async = false;
                        client.UploadCompleted -= Folder_Upload_Completed;
                        SystemTray.ProgressIndicator.IsVisible = false;
                        //MessageBox.Show("Tous les fichiers ont été envoyés.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Erreur upload: sky_base_folder_id = null");
            }
        }
        void Folder_Upload_Completed(object sender, LiveOperationCompletedEventArgs e)
        {
            wait_async = false;
            if (progress_list.Average() == 100 && progress_list.Count() == (int)e.UserState + 1)
            {
                client.UploadCompleted -= Folder_Upload_Completed;
                SystemTray.ProgressIndicator.IsVisible = false;
                //MessageBox.Show("Tous les fichiers ont été envoyés.");
                wait_async = true;
                client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Show_Files_Completed);
                client.GetAsync(sky_base_folder_id + "/files");
            }
            if (e.Error != null)
            {
                MessageBox.Show("Erreur envoi de fichier: " + e.Error.ToString());
            }
        }
        private void Folder_Download_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Veuillez vous connecter !");
            }
            else
            {
                if (MessageBox.Show("Voulez-vous vraiment télécharger tous les fichiers ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (wait_async == false)
                    {
                        progress_multiple = true;
                        Progress_Init();
                        wait_async = true;
                        int loop_count = 0;
                        client.DownloadCompleted += new EventHandler<LiveDownloadCompletedEventArgs>(Folder_Download_Completed);
                        foreach (object entry in list_files_skydrive)
                        {
                            if (My_Isolated_Storage.FileExists("GPX\\" + list_files_skydrive.ElementAt(loop_count).Name.ToString()) == false)
                            {
                                progress_list.Add(0);
                                client.DownloadAsync(list_files_skydrive.ElementAt(loop_count).Id + "/content", loop_count);
                            }
                            else
                            {
                                progress_list.Add(100);
                            }
                            loop_count = loop_count + 1;
                        }
                        if (progress_list.Average() == 100)
                        {
                            wait_async = false;
                            client.DownloadCompleted -= Folder_Download_Completed;
                            SystemTray.ProgressIndicator.IsVisible = false;
                            //MessageBox.Show("Tous les fichiers sont déjà téléchargés.");
                        }
                    }
                }
            }
        }
        void Folder_Download_Completed(object sender, LiveDownloadCompletedEventArgs e)
        {
            wait_async = false;
            if (progress_list.Average() == 100)
            {
                client.DownloadCompleted -= Folder_Download_Completed;
                SystemTray.ProgressIndicator.IsVisible = false;
                //MessageBox.Show("Tous les fichiers ont été téléchargés.");
            }
            if (e.Error == null) // enregistre le fichier
            {
                using (StreamReader reader = new StreamReader(e.Result))
                {
                    string curr_file = list_files_skydrive.ElementAt((int)e.UserState).Name.ToString();
                    string curr_content = reader.ReadToEnd();
                    using (IsolatedStorageFileStream fileStream = My_Isolated_Storage.OpenFile("GPX\\" + curr_file, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.Write(curr_content);
                            writer.Close();
                        }
                        fileStream.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Erreur téléchargement de fichier: " + e.Error.ToString());
            }
        }
        private void Folder_Delete_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Veuillez vous connecter !");
            }
            else
            {
                if (MessageBox.Show("Voulez-vous vraiment effacer tous les fichiers ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (wait_async == false)
                    {
                        progress_multiple = true;
                        Progress_Init();
                        wait_async = true;
                        int loop_count = 0;
                        client.DeleteCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Folder_Delete_Completed);
                        foreach (object entry in list_files_skydrive)
                        {
                            client.DeleteAsync(list_files_skydrive.ElementAt(loop_count).Id.ToString(), loop_count);
                            loop_count = loop_count + 1;
                        }
                    }
                }
            }
            list_select_id = null;
            list_select_name = null;
            ListBox_SkyContent.SelectedItem = null;
        }
        void Folder_Delete_Completed(object sender, LiveOperationCompletedEventArgs e)
        {
            wait_async = false;
            if ((int)e.UserState + 1 == list_files_skydrive.Count())
            {
                wait_async = true;
                client.DeleteCompleted -= Folder_Delete_Completed;
                client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Show_Files_Completed);
                client.GetAsync(sky_base_folder_id + "/files");
                SystemTray.ProgressIndicator.IsVisible = false;
                //MessageBox.Show("Tous les fichiers ont été effacés.");
            }
            if (e.Error != null) 
            {
                MessageBox.Show("Erreur effacement de fichier: " + e.Error.ToString());
            }
        }
        private void File_Download_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Veuillez vous connecter !");
            }
            else
            {
                if (list_select_id == null | list_select_name == null)
                {
                    MessageBox.Show("Selectionnez un fichier");
                }
                else if (My_Isolated_Storage.FileExists("GPX\\" + list_select_name) == true)
                {
                    //MessageBox.Show("Le fichier " + list_select_name + " est déjà téléchargé");
                }
                else
                {
                    if (wait_async == false)
                    {
                        progress_multiple = false;
                        Progress_Init();
                        wait_async = true;
                        client.DownloadCompleted += new EventHandler<LiveDownloadCompletedEventArgs>(File_Download_Completed);
                        client.DownloadAsync(list_select_id + "/content", list_select_name);
                    }
                }
            }
        }
        void File_Download_Completed(object sender, LiveDownloadCompletedEventArgs e)
        {
            wait_async = false;
            client.DownloadCompleted -= File_Download_Completed;
            if (e.Result != null)
            {
                using (StreamReader reader = new StreamReader(e.Result))
                {
                    string curr_file = e.UserState.ToString();
                    string curr_content = reader.ReadToEnd();
                    using (IsolatedStorageFileStream fileStream = My_Isolated_Storage.OpenFile("GPX\\" + curr_file, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.Write(curr_content);
                            writer.Close();
                            SystemTray.ProgressIndicator.IsVisible = false;
                            //MessageBox.Show("Le fichier " + curr_file + " à été téléchargé");
                        }
                        fileStream.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Erreur téléchargement de fichier: " + e.Error.ToString());
            }
        }
        private void File_Delete_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Veuillez vous connecter !");
            }
            else
            {
                if (list_select_id == null | list_select_name == null)
                {
                    MessageBox.Show("Selectionnez un fichier");
                }
                else
                {
                    if (MessageBox.Show("Voulez-vous vraiment effacer " + list_select_name + " ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        if (wait_async == false)
                        {
                            progress_multiple = false;
                            Progress_Init();
                            wait_async = true;
                            client.DeleteCompleted += new EventHandler<LiveOperationCompletedEventArgs>(File_Delete_Completed);
                            client.DeleteAsync(list_select_id);
                        }
                    }
                }
            }
        }
        void File_Delete_Completed(object sender, LiveOperationCompletedEventArgs e)
        {
            wait_async = false;
            client.DeleteCompleted -= File_Delete_Completed;
            if (e.Error == null)
            {
                wait_async = true;
                client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(Show_Files_Completed);
                client.GetAsync(sky_base_folder_id + "/files");
                SystemTray.ProgressIndicator.IsVisible = false;
                //MessageBox.Show("Le fichier " + list_select_name + " à été effacé.");
            }
            else
            {
                MessageBox.Show("Erreur suppresion de fichier: " + e.Error.ToString());
            }
            list_select_id = null;
            list_select_name = null;
            ListBox_SkyContent.SelectedItem = null;
        }
   }
}