// Helpers/Settings.cs This file was automatically added when you installed the Settings Plugin. If you are not using a PCL then comment this file back in to use it.
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Xamarin.AndroidThings.WeatherStation.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = string.Empty;

        #endregion


        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault<string>(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue<string>(SettingsKey, value);
            }
        }

        #region Setting Constants

        private const string DeviceIdKey = "DeviceId_key";
        private static readonly string DeviceIdDefault = string.Empty;

        private const string DeviceKeyKey = "DeviceKey_key";
        private static readonly string DeviceKeyDefault = string.Empty;

        private const string HostNameKey = "HostName_key";
        private static readonly string HostNameDefault = string.Empty;


        private const string TemperatureUpdateThresholdKey = "TemperatureUpdateThreshold_Key";
        private static readonly double TemperatureUpdateThresholdDefault = 0.01d;

        #endregion


        public static string DeviceId
        {
            get { return AppSettings.GetValueOrDefault<string>(DeviceIdKey, DeviceIdDefault); }
            set { AppSettings.AddOrUpdateValue<string>(DeviceIdKey, value); }
        }

        public static string DeviceKey
        {
            get { return AppSettings.GetValueOrDefault<string>(DeviceKeyKey, DeviceKeyDefault); }
            set { AppSettings.AddOrUpdateValue<string>(DeviceKeyKey, value); }
        }

        public static string HostName
        {
            get { return AppSettings.GetValueOrDefault<string>(HostNameKey, HostNameDefault); }
            set { AppSettings.AddOrUpdateValue<string>(HostNameKey, value); }
        }


        public static double TemperatureUpdateThreshold
        {
            get { return AppSettings.GetValueOrDefault<double>(TemperatureUpdateThresholdKey, TemperatureUpdateThresholdDefault); }
            set { AppSettings.AddOrUpdateValue<double>(TemperatureUpdateThresholdKey, value); }
        }

    }
}