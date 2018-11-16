using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Info;

namespace Explo_GPS
{
    public class AppReportingService
    {
        const string filename = "LittleWatson.txt";
 
        internal static void Email_Report_Error(Exception ex, string extra)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    Email_Delete_File(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        output.WriteLine(extra);
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        internal static void Email_Check_Previous_Excepetion()
        {
            try
            {
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(filename))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            contents = reader.ReadToEnd();
                        }
                        Email_Delete_File(store);
                    }
                }
                if (contents != null)
                {
                    if (MessageBox.Show("Une erreur est survenue depuis le dernier lancement de l'application. Voulez-vous envoyer un rapport d'erreur ?", "Rapport d'erreur", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        string deviceManufacturer = DeviceStatus.DeviceManufacturer;
                        string deviceName = DeviceStatus.DeviceName;
                        string deviceHardwareVersion = DeviceStatus.DeviceHardwareVersion;
                        string deviceFirmwareVersion = DeviceStatus.DeviceFirmwareVersion;
                        string OSVersion = Environment.OSVersion.Version.ToString();
                        long deviceTotalMemory = DeviceStatus.DeviceTotalMemory;
                        EmailComposeTask email = new EmailComposeTask();
                        email.To = "my@email.com";
                        email.Subject = "Explo GPS auto-generated problem report";
                        email.Body = contents + "\n\nConstructeur: " + deviceManufacturer + "\nModèle: " + deviceName + "\nH/W version: " + deviceHardwareVersion + "\nF/W Version: " + deviceFirmwareVersion + "\nOS: Windows Phone " + OSVersion + "\nTotal Mem: " + (deviceTotalMemory / 1048576) + "Mo";
                        Email_Delete_File(IsolatedStorageFile.GetUserStoreForApplication());
                        email.Show();
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Email_Delete_File(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }
        private static void Email_Delete_File(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(filename);
            }
            catch (Exception)
            {
            }
        }

        public static void Flurry_Page_Init()
        {
            string page_name = ((App)Application.Current).RootFrame.CurrentSource.ToString();
            FlurryWP7SDK.Api.LogEvent(page_name.Substring(1, page_name.Length - 6));
            FlurryWP7SDK.Api.LogPageView();
        }

        /*internal void Flurry_Report_Error(Exception ex, string extra)
        {
            try
            {
                FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": " + ex.Message.ToString(), ex);
                FlurryWP7SDK.Api.LogError(ex.StackTrace.ToString(), ex);
                FlurryWP7SDK.Api.EndSession();
            }
            catch
            {
            }
        }*/
    }
}
