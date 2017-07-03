using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.AndroidThings.WeatherStation.Helpers;

namespace Xamarin.AndroidThings.WeatherStation.Hubs
{
    public class WeatherDevice : RemoteMonitoringDevice
    {
        public WeatherDevice()
        {
            this.DeviceId = Settings.DeviceId;
            this.DeviceKey = Settings.DeviceKey;
            this.HostName = Settings.HostName;
        }
        public bool CheckConfig()
        {
            if (((this.DeviceId != null) && (this.DeviceKey != null) && (this.HostName != null) &&
                 (this.DeviceId != "") && (this.DeviceKey != "") && (this.HostName != "")))
            {
                Settings.DeviceId = this.DeviceId;
                Settings.DeviceKey = this.DeviceKey;
                Settings.HostName = this.HostName;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
