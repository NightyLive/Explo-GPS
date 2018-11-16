using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net.NetworkInformation;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;


namespace Explo_GPS
{
    public partial class GPX_View : PhoneApplicationPage
    {
        IsolatedStorageFile appIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
        EmailAddressChooserTask Email_Chooser_Task;
        string currentFileNamePath;
        string currentFileName;
        
        public GPX_View()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            FlurryWP7SDK.Api.LogEvent("GPX_View");
            FlurryWP7SDK.Api.LogPageView();
            base.OnNavigatedTo(e);
            currentFileNamePath = "GPX\\" + NavigationContext.QueryString["FileName"];
            currentFileName = NavigationContext.QueryString["FileName"];
            this.actualFileName.Text = currentFileName;
            this.ReadFileData(currentFileNamePath);
        }

        private void Click_Save(object sender, EventArgs e) 
        {
            string fileSaveName = DateTime.Today.Year.ToString("0000") + "-" + DateTime.Today.Month.ToString("00") + "-" + DateTime.Today.Day.ToString("00") + "_" + DateTime.Now.Hour.ToString("00") + "-" + DateTime.Now.Minute.ToString("00") + "-" + DateTime.Now.Second.ToString("00") + ".gpx";
            string fileSaveNamePath = "GPX\\" + fileSaveName; // +DateTime.Today.Year.ToString("0000") + "-" + DateTime.Today.Month.ToString("00") + "-" + DateTime.Today.Day.ToString("00") + "_" + DateTime.Now.Hour.ToString("00") + "-" + DateTime.Now.Minute.ToString("00") + "-" + DateTime.Now.Second.ToString("00") + ".gpx";
            if (appIsolatedStorage.FileExists(fileSaveNamePath) == false)
            {
                using (IsolatedStorageFileStream fileStream = appIsolatedStorage.OpenFile(fileSaveNamePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        string editedText = this.fileContent.Text.Trim();
                        writer.Write(editedText);
                        writer.Close();
                        MessageBox.Show("Le fichier " + fileSaveName + " à été enregistré");
                    }
                }
            }            
        }
        private void Click_Map(object sender, EventArgs e)
        {
            //MessageBox.Show("Pas encore implémenté");
            if (NetworkInterface.GetIsNetworkAvailable() == true)
            {
                NavigationService.Navigate(new Uri("/GPX_Map.xaml?FileName=" + NavigationContext.QueryString["FileName"], UriKind.Relative));
            }
            else
            {
                MessageBox.Show("Connexion de données non disponible !");
            }
        }
        private void Click_Delete(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment effacer ce fichier ?", "Attention !", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                using (IsolatedStorageFile appIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        appIsolatedStorage.DeleteFile(currentFileNamePath);
                        //MessageBox.Show("Le fichier " + currentFileName + " à été effacé");
                        NavigationService.GoBack();
                    }
                    catch
                    {
                        MessageBox.Show("Le fichier n'a pas pu être effacé !");
                    }
                }
            }
        }
        private void Click_Back(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
        private void ReadFileData(string filePath) 
        { 
                if (appIsolatedStorage.FileExists(filePath)) 
                { 
                    using (IsolatedStorageFileStream fileStream = appIsolatedStorage.OpenFile(filePath, FileMode.Open, FileAccess.Read)) 
                    { 
                        using (StreamReader reader = new StreamReader(fileStream)) 
                        { 
                            this.fileContent.Text = reader.ReadToEnd(); 
                        }
                    } 
                } 
        } 
/*        private void EditFileData(string filePath) 
        { 
            using (IsolatedStorageFile appIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication()) 
            { 
                if (appIsolatedStorage.FileExists(filePath)) 
                { 
                    using (IsolatedStorageFileStream fileStream = appIsolatedStorage.OpenFile(filePath, FileMode.Open, FileAccess.Write)) 
                    { 
                        using (StreamWriter writer = new StreamWriter(fileStream)) 
                        { 
                            string editedText = this.fileContent.Text.Trim(); 
                            writer.Write(editedText); 
                            writer.Close(); 
                        } 
                    } 
                } 
            } 

            this.NavigationService.Navigate(new Uri("/GPX.xaml", UriKind.Relative)); 
        }*/
        private void Click_Email(object sender, EventArgs e)
        {
            try
            {
                if (fileContent.Text != "")
                {
                    using (IsolatedStorageFileStream fileStream = appIsolatedStorage.OpenFile(currentFileNamePath, FileMode.Open, FileAccess.Read))
                    {
                        double gpx_length = fileStream.Length;
                        if (gpx_length > 32700) // 64k mais chaque char vaut 2 octets
                        {
                            MessageBox.Show("Windows Phone n'autorise pas l'envoi de pièces jointes et limite la taille maximale du contenu des e-mails à 32 kilo octets.\nMalheureusement vous avez dépassé cette taille (" + (gpx_length / 1024).ToString("0.0") + " ko).\nVeuillez utiliser Skydrive afin de transférer votre fichier et éventuellement l'utiliser ou l'envoyer à partir de votre ordinateur.\n");
                        }
                        else
                        {
                            Email_Chooser_Task = new EmailAddressChooserTask();
                            Email_Chooser_Task.Completed += new EventHandler<EmailResult>(TaskChooserEmailCompleted);
                            Email_Chooser_Task.Show();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("La localisation n'est pas démarrée !");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue.");
                FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": GPX_View Click_Email: " + ex.Message.ToString(), ex);
                FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
            }
        }
        private void TaskChooserEmailCompleted(object sender, EmailResult e)
        {
            try
            {
                if (e.TaskResult == TaskResult.OK)
                {
                    EmailComposeTask MyEmailComposer = new EmailComposeTask();
                    MyEmailComposer.To = e.Email;
                    MyEmailComposer.Subject = "Explo GPS : " + currentFileName;
                    MyEmailComposer.Body = fileContent.Text;
                    MyEmailComposer.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur pendant la génération de l'email");
                //try
                //{
                    FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": GPX_View TaskChooserEmailCompleted: " + ex.Message.ToString(), ex);
                    FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
                //}
                //catch
                //{
                //}
                //throw;// ex;
            }
        }
    } 
} 
