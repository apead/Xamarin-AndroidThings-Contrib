using System;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Widget;
using Android.OS;
using Android.Things.Pio;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Com.Google.Android.Things.Contrib.Driver.Apa102;
using Com.Google.Android.Things.Contrib.Driver.Bmx280;
using Com.Google.Android.Things.Contrib.Driver.Button;
using Com.Google.Android.Things.Contrib.Driver.Ht16k33;
using Com.Google.Android.Things.Contrib.Driver.Pwmspeaker;
using Xamarin.AndroidThings.WeatherStation.Callbacks;
using Xamarin.AndroidThings.WeatherStation.Enum;
using Xamarin.AndroidThings.WeatherStation.Helpers;
using Xamarin.AndroidThings.WeatherStation.Hubs;
using Xamarin.AndroidThings.WeatherStation.Messages;
using Java.IO;

namespace Xamarin.AndroidThings.WeatherStation
{
    [Activity(Label = "Weather Station - Azure Iot Hubs")]
    [IntentFilter(new[] {Intent.ActionMain}, Categories = new[] {Intent.CategoryLauncher})]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "android.intent.category.IOT_LAUNCHER", "android.intent.category.DEFAULT" })]
    public class MainActivity : Activity
    {
        private double _lastUpdatedPressure;
        private double _lastUpdatedTemperature;
        private double _lastUpdatedHumidity;
        private ImageView _imageView;
        public static string Tag = typeof(MainActivity).FullName;
        public SensorManager SensorManager { get; set; }

        private SensorManager.DynamicSensorCallback _dynamicSensorCallback;
        private ButtonInputDriver _buttonInputDriver;
        private Bmx280SensorDriver _bmx280SensorDriver;
        private AlphanumericDisplay _display;
        private Apa102Contrib _ledRainbowStrip;
        private Gpio _led;
        private AlphaNumericDisplayMode _displayMode = AlphaNumericDisplayMode.Temperature;

        private int[] _rainbow = new int[7];
        private const int LedstripBrightness = 1;
        public const float BarometerRangeLow = 800f;
        public const float BarometerRangeHigh = 1080f;
        public const float BarometerRangeSunny = 1010f;
        public const float BarometerRangeRainy = 990f;

        private const int SpeakerReadyDelayMs = 300;

        private bool _useHubs = true;  //  Set this to true to use Azure Iot Hubs

        public Speaker Speaker;

        private WeatherDevice _weatherDevice;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (_useHubs)
            InitializeHubs();


            SetContentView(Resource.Layout.Main);

            SensorManager = (SensorManager) GetSystemService(SensorService);
            _dynamicSensorCallback = new WeatherDynamicSensorCallback(this);
            _imageView = FindViewById<ImageView>(Resource.Id.imageView);
            
            PubSubHandler.GetInstance().Subscribe<TemperatureMessage>(OnTemperatureMessage);
            PubSubHandler.GetInstance().Subscribe<PressureMessage>(OnPressureMessage);
            PubSubHandler.GetInstance().Subscribe<HumidityMessage>(OnHumidityMessage);
        }

        private void InitializeHubs()
        {
            _weatherDevice = new WeatherDevice();

            _weatherDevice.DeviceId = "[DeviceId]";
            _weatherDevice.DeviceKey = "[DeviceKey]";
            _weatherDevice.HostName = "[HostName]";

            _weatherDevice.SendTelemetryFreq = 10000;
            _weatherDevice.Connect();

            
        }

        private void OnTemperatureMessage(TemperatureMessage message)
        {
            Log.Debug(Tag, "Temperature Message: " +message.TemperatureReading);

            _lastUpdatedTemperature = message.TemperatureReading;

            if (_display != null)
            {
                if (_displayMode == AlphaNumericDisplayMode.Temperature)
                    UpdateDisplay(message.TemperatureReading);

                if (_useHubs)
                {
                    _weatherDevice.UpdateTemperature(message.TemperatureReading);
                    _weatherDevice.SendTelemetryData = true;
                }
            }
        }

        private void OnPressureMessage(PressureMessage message)
        {
            Log.Debug(Tag, "Pressure Message: " + message.PressureReading);

            _lastUpdatedPressure = message.PressureReading;

            if (_display != null)
            {
                if (_displayMode == AlphaNumericDisplayMode.Pressure)
                    UpdateDisplay(message.PressureReading);

                    UpdateBarometer(message.PressureReading);

                if (_useHubs)
                {
                    _weatherDevice.UpdatePressure(message.PressureReading);
                    _weatherDevice.SendTelemetryData = true;
                }
            }
        }

        private void OnHumidityMessage(HumidityMessage message)
        {
            Log.Debug(Tag, "Humidity Message: " + message.HumidityReading);

            _lastUpdatedHumidity = message.HumidityReading;

            if (_display != null)
            {
                if (_displayMode == AlphaNumericDisplayMode.Temperature)
                    UpdateDisplay(message.HumidityReading);

                if (_useHubs)
                {
                    _weatherDevice.UpdateHumidity(message.HumidityReading);
                    _weatherDevice.SendTelemetryData = true;
                }
            }
        }

        private void UpdateDisplay(double value)
        {
            try
            {
                _display.Display(value);

            }
            catch (Exception e)
            {
                Log.Error(Tag, "Error setting display", e);
            }
        }

        public void UpdateBarometer(float pressure)
        {
            if (pressure > BarometerRangeSunny)
                _imageView.SetImageResource(Resource.Drawable.ic_sunny);
            else if (pressure < BarometerRangeRainy)
                _imageView.SetImageResource(Resource.Drawable.ic_rainy);
            else
                _imageView.SetImageResource(Resource.Drawable.ic_cloudy);

            if (_ledRainbowStrip == null)
                return;

            int[] clearColors = new int[_rainbow.Length];

            float t = (pressure - BarometerRangeLow) / (BarometerRangeHigh - BarometerRangeLow);
            int n = (int) Math.Ceiling(_rainbow.Length * t);
            n = Math.Max(0, Math.Min(n, _rainbow.Length));
            int[] colors = new int[_rainbow.Length];
            for (int i = 0; i < n; i++)
            {
                int ri = _rainbow.Length - 1 - i;
                colors[ri] = _rainbow[ri];
            }
            try
            {
                _ledRainbowStrip.Write(clearColors);
                _ledRainbowStrip.Write(colors);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
                {
                    if (keyCode == Keycode.A)
                    {
                        _displayMode = AlphaNumericDisplayMode.Pressure;

                        UpdateDisplay(_lastUpdatedPressure);

                        try
                        {
                            _led.Value = true;
                        }
                        catch (Exception exception)
                        {
                    System.Console.WriteLine(exception);
                        }
                        return true;
                    }
                    return base.OnKeyUp(keyCode, e);
                    }
        
                public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
           {
                if (keyCode == Keycode.A)
                {
                    _displayMode = AlphaNumericDisplayMode.Temperature;
                    UpdateDisplay(_lastUpdatedTemperature);
                    try
                    {
                        _led.Value = false;
                    }
                catch (Exception exception)
                {
                    System.Console.WriteLine(exception);
                }
                return true;
            }

            return base.OnKeyUp(keyCode, e);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_display != null)
            {
                try
                {
                    _display.Clear();
                    _display.SetEnabled(false);
                    _display.Close();
                }
                catch (IOException e)
                {
                    Log.Error(Tag, "Error disabling display", e);
                }
                finally
                {
                    _display = null;
                }
            }

            if (_ledRainbowStrip != null)
            {
                try
                {
                    _ledRainbowStrip.Write(new int[7]);
                    _ledRainbowStrip.Brightness = 0;
                    _ledRainbowStrip.Close();
                }
                catch (IOException e)
                {
                    Log.Error(Tag, "Error disabling ledstrip", e);
                }
                finally
                {
                    _ledRainbowStrip = null;
                }
            }
        }
    }
}

