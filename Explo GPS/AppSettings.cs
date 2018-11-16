using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;


namespace Explo_GPS
{
    public class AppSettings
    {
        // Our isolated storage App_Settings
        IsolatedStorageSettings isolatedStore;

        //const string CheckBoxSettingKeyName = "CheckBoxSetting";
        //const string ListBoxSettingKeyName = "ListBoxSetting";
        //const string RadioButton1SettingKeyName = "RadioButton1Setting";
        //const string RadioButton2SettingKeyName = "RadioButton2Setting";
        //const string RadioButton3SettingKeyName = "RadioButton3Setting";
        //const string PasswordSettingKeyName = "PasswordSetting";
        const string GPS_Treshold_KeyName = "GPS_Threshold";
        const string Switch_AutostartSettingKeyName = "Switch_Autostart";
        const string Switch_FollowSettingKeyName = "Switch_Follow";
        const string Switch_RoadmodeSettingKeyName = "Switch_Roadmode";
        const string Switch_AutostartMapSettingKeyName = "Switch_AutostartMap";
        const string Switch_AutostartCompasSettingKeyName = "Switch_AutostartCompas";
        const string Switch_AutostartCameraSettingKeyName = "Switch_AutostartCamera";
        const string Switch_DecimalSettingKeyName = "Switch_Decimal";
        const string Switch_MetricSettingKeyName = "Switch_Metric";
        const string Switch_GPXSettingKeyName = "Switch_GPX";
        const string Switch_GPX_ResetSettingKeyName = "Switch_GPX_Reset";
        const string Switch_UnderlockSettingKeyName = "Switch_Underlock";
        const string Switch_AvoidlockSettingKeyName = "Switch_Avoidlock";
        const string Switch_GPS_SensibilitySettingKeyName = "Switch_GPS_Sensibility";
        const string First_LaunchKeyName = "First_Launch";
        //const string Skydrive_AuthKeyName = "Skydrive_Auth";

        //const bool CheckBoxSettingDefault = true;
        //const int ListBoxSettingDefault = 1;
        //const bool RadioButton1SettingDefault = true;
        //const bool RadioButton2SettingDefault = false;
        //const bool RadioButton3SettingDefault = false;
        //const string PasswordSettingDefault = "";
        const string GPS_Treshold_Default = "25";
        const bool Switch_AutostartSettingDefault = true;
        const bool Switch_FollowSettingDefault = true;
        const bool Switch_RoadmodeSettingDefault = true;
        const bool Switch_AutostartMapSettingDefault = true;
        const bool Switch_AutostartCompasSettingDefault = true;
        const bool Switch_AutostartCameraSettingDefault = false;
        const bool Switch_DecimalSettingDefault = true;
        const bool Switch_MetricSettingDefault = true;
        const bool Switch_GPXSettingDefault = true;
        const bool Switch_GPX_ResetSettingDefault = true;
        const bool Switch_UnderlockSettingDefault = true; // a metre false si marketplace requier l'autorisation explicite de l'utilisateur (1er demmarrage)
        const bool Switch_AvoidlockSettingDefault = false;
        const bool Switch_GPS_SensibilitySettingDefault = true;
        const bool First_LaunchDefault = true;
        //const string Skydrive_AuthDefault = "null";

        // Constructor
        public AppSettings()
        {
            //Debug.WriteLine("AppSettings()");
            try
            {
                // Get the App_Settings for this application.
                isolatedStore = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Erreur dans IsolatedStorageSettings: " + e.ToString());
                Debug.WriteLine("Erreur dans IsolatedStorageSettings: " + e.ToString());
            }
        }
        public string GPS_Threshold
        {
            get
            {
                return GetValueOrDefault<string>(GPS_Treshold_KeyName, GPS_Treshold_Default);
            }
            set
            {
                //CultureInfo culture = new CultureInfo("fr-FR");
                //decimal tempnumber;
                //if (Decimal.TryParse(value, NumberStyles.AllowDecimalPoint, culture, out tempnumber))
                //{
                //    if (Convert.ToDouble(value) < Convert.ToDouble(1))
                //    {
                //        value = "1";
                //        MessageBox.Show("<1 atteint");
                //    }
                //    else if (Convert.ToDouble(value) > Convert.ToDouble(1000))
                //    {
                //        value = "1000";
                //        MessageBox.Show(">1000 atteint");
                //    }
                //}
                //else
                //{
                //    MessageBox.Show("Erreur de format");
                //    value = "5";
                //}
                AddOrUpdateValue(GPS_Treshold_KeyName, value);
                Save();
                //MessageBox.Show("Save fait");
            }
        }
        public bool First_Launch
        {
            get
            {
                return GetValueOrDefault<bool>(First_LaunchKeyName, First_LaunchDefault);
            }
            set
            {
                AddOrUpdateValue(First_LaunchKeyName, value);
                Save();
            }
        }
        public bool Switch_Autostart_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_AutostartSettingKeyName, Switch_AutostartSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_AutostartSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_Follow_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_FollowSettingKeyName, Switch_FollowSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_FollowSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_Roadmode_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_RoadmodeSettingKeyName, Switch_RoadmodeSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_RoadmodeSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_AutostartMap_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_AutostartMapSettingKeyName, Switch_AutostartMapSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_AutostartMapSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_AutostartCompas_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_AutostartCompasSettingKeyName, Switch_AutostartCompasSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_AutostartCompasSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_AutostartCamera_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_AutostartCameraSettingKeyName, Switch_AutostartCameraSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_AutostartCameraSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_Decimal_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_DecimalSettingKeyName, Switch_DecimalSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_DecimalSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_Metric_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_MetricSettingKeyName, Switch_MetricSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_MetricSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_GPX_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_GPXSettingKeyName, Switch_GPXSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_GPXSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_GPX_Reset_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_GPX_ResetSettingKeyName, Switch_GPX_ResetSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_GPX_ResetSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_Underlock_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_UnderlockSettingKeyName, Switch_UnderlockSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_UnderlockSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_Avoidlock_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_AvoidlockSettingKeyName, Switch_AvoidlockSettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_AvoidlockSettingKeyName, value);
                Save();
            }
        }
        public bool Switch_GPS_Sensibility_Setting
        {
            get
            {
                return GetValueOrDefault<bool>(Switch_GPS_SensibilitySettingKeyName, Switch_GPS_SensibilitySettingDefault);
            }
            set
            {
                AddOrUpdateValue(Switch_GPS_SensibilitySettingKeyName, value);
                Save();
            }
        }
        /*public string Skydrive_Auth
        {
            get
            {
                return GetValueOrDefault<string>(Skydrive_AuthKeyName, Skydrive_AuthDefault);
            }
            set
            {
                AddOrUpdateValue(Skydrive_AuthKeyName, value);
                Save();
            }
        }*/


        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (isolatedStore.Contains(Key))
            {
                // If the value has changed
                if (isolatedStore[Key] != value)
                {
                    // Store the new value
                    isolatedStore[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                isolatedStore.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            //Debug.WriteLine("GetValueOrDefault<valueType");
            valueType value;

            // If the key exists, retrieve the value.
            if (isolatedStore.Contains(Key))
            {
                value = (valueType)isolatedStore[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the App_Settings.
        /// </summary>
        public void Save()
        {
            isolatedStore.Save();
        }

        /// <summary>
        /// Property to get and set a CheckBox Setting Key.
        /// </summary>
        //public bool CheckBoxSetting
        //{
        //    get
        //    {
        //        return GetValueOrDefault<bool>(CheckBoxSettingKeyName, CheckBoxSettingDefault);
        //    }
        //    set
        //    {
        //        AddOrUpdateValue(CheckBoxSettingKeyName, value);
        //        Save();
        //    }
        //}

        /// <summary>
        /// Property to get and set a ListBox Setting Key.
        /// </summary>
        //public int ListBoxSetting
        //{
        //    get
        //    {
        //        return GetValueOrDefault<int>(ListBoxSettingKeyName, ListBoxSettingDefault);
        //    }
        //    set
        //    {
        //        AddOrUpdateValue(ListBoxSettingKeyName, value);
        //        Save();
        //    }
        //}

        /// <summary>
        /// Property to get and set a RadioButton Setting Key.
        /// </summary>
        //public bool RadioButton1Setting
        //{
        //    get
        //    {
        //        return GetValueOrDefault<bool>(RadioButton1SettingKeyName, RadioButton1SettingDefault);
        //    }
        //    set
        //    {
        //        AddOrUpdateValue(RadioButton1SettingKeyName, value);
        //        Save();
        //    }
        //}

        /// <summary>
        /// Property to get and set a RadioButton Setting Key.
        /// </summary>
        //public bool RadioButton2Setting
        //{
        //    get
        //    {
        //        return GetValueOrDefault<bool>(RadioButton2SettingKeyName, RadioButton2SettingDefault);
        //    }
        //    set
        //    {
        //        AddOrUpdateValue(RadioButton2SettingKeyName, value);
        //        Save();
        //    }
        //}

        /// <summary>
        /// Property to get and set a RadioButton Setting Key.
        /// </summary>
        //public bool RadioButton3Setting
        //{
        //    get
        //    {
        //        return GetValueOrDefault<bool>(RadioButton3SettingKeyName, RadioButton3SettingDefault);
        //    }
        //    set
        //    {
        //        AddOrUpdateValue(RadioButton3SettingKeyName, value);
        //        Save();
        //    }
        //}

        /// <summary>
        /// Property to get and set a Password Setting Key.
        /// </summary>
        //public string PasswordSetting
        //{
        //    get
        //    {
        //        return GetValueOrDefault<string>(PasswordSettingKeyName, PasswordSettingDefault);
        //    }
        //    set
        //    {
        //        AddOrUpdateValue(PasswordSettingKeyName, value);
        //        Save();
        //    }
        //}
    }
}