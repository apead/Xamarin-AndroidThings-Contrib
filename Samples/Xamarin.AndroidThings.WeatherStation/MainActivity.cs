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
    [IntentFilter(new[] {Intent.ActionMain}, Categories = new[] {"android.intent.category.IOT_LAUNCHER"})]

   public class MainActivity : Activity
    {
        private double _lastUpdatedPressure;
        private double _lastUpdatedTemperature;
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
        public const float BarometerRangeLow = 965f;
        public const float BarometerRangeHigh = 1035f;
        public const float BarometerRangeSunny = 1010f;
        public const float BarometerRangeRainy = 990f;

        private const int SpeakerReadyDelayMs = 300;

        private bool _useHubs = false;  //  Set this to true to use Azure Iot Hubs

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

            try
            {


                _ledRainbowStrip = new Apa102Contrib(BoardDefaults.GetSpiBus(), Apa102Contrib.Mode.Bgr);
                _ledRainbowStrip.Brightness = LedstripBrightness;
                for (var i = 0; i < _rainbow.Length; i++)
                {
                    float[] hsv = {i * 360f / _rainbow.Length, 1.0f, 1.0f};

                    _rainbow[i] = Color.HSVToColor(255, hsv);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                _ledRainbowStrip = null;
            }

            try
            {
                var pioService = new PeripheralManagerService();
                _led = pioService.OpenGpio(BoardDefaults.GetLedGpioPin());
                _led.SetEdgeTriggerType(Gpio.EdgeNone);
                _led.SetDirection(Gpio.DirectionOutInitiallyLow);
                _led.SetActiveType(Gpio.ActiveHigh);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }

            try
            {
                _buttonInputDriver = new ButtonInputDriver(BoardDefaults.GetButtonGpioPin(),
                    ButtonContrib.LogicState.PressedWhenLow,
                    (int) KeyEvent.KeyCodeFromString("KEYCODE_A"));
                _buttonInputDriver.Register();
                Log.Debug(Tag, "Initialized GPIO Button that generates a keypress with KEYCODE_A");
            }
            catch (Exception e)
            {
                throw new Exception("Error initializing GPIO button", e);
            }

            try
            {
                _bmx280SensorDriver = new Bmx280SensorDriver(BoardDefaults.GetI2cBus());
                SensorManager.RegisterDynamicSensorCallback(_dynamicSensorCallback);
                _bmx280SensorDriver.RegisterTemperatureSensor();
                _bmx280SensorDriver.RegisterPressureSensor();
                Log.Debug(Tag, "Initialized I2C BMP280");
            }
            catch (Exception e)
            {
                throw new Exception("Error initializing BMP280", e);
            }

            try
            {
                _display = new AlphanumericDisplay(BoardDefaults.GetI2cBus());
                _display.SetEnabled(true);
                _display.Clear();
                Log.Debug(Tag, "Initialized I2C Display");
            }
            catch (Exception e)
            {
                Log.Error(Tag, "Error initializing display", e);
                Log.Debug(Tag, "Display disabled");
                _display = null;
            }


            try
            {
                Speaker = new Speaker(BoardDefaults.GetSpeakerPwmPin());
                var slide = ValueAnimator.OfFloat(440, 440 * 4);
                slide.SetDuration(50);
                slide.RepeatCount = 5;
                slide.SetInterpolator(new LinearInterpolator());
                slide.AddUpdateListener(new SlideUpdateListener(this));

                //  slide.Start();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
        }

        private void InitializeHubs()
        {
            _weatherDevice = new WeatherDevice();

            _weatherDevice.AddTelemetry(new TelemetryFormat {Name = "Temperature", DisplayName = "Temp", Type = "Double"},
                (double) 0);
            _weatherDevice.AddTelemetry(new TelemetryFormat {Name = "Pressure", DisplayName = "hPa", Type = "Double"},
                (double) 0);

            _weatherDevice.DeviceId = "<Add Azure Iot Hub Device Id Here>";
            _weatherDevice.DeviceKey = "<Add Azure Iot Hub Device Key Here>";
            _weatherDevice.HostName = "<Add Azure Iot Hub Hostname Here>";

            _weatherDevice.SendTelemetryFreq = 60000;
            _weatherDevice.Connect();

            
            _weatherDevice.onReceivedMessage += WeatherDevice_onReceivedMessage;
        }

        private void WeatherDevice_onReceivedMessage(object sender, EventArgs e)
        {
            var receivedMessage = e as ReceivedMessageEventArgs;

            if (receivedMessage != null)
            {
                if (receivedMessage.Message.Name == "CustomDisplay")
                {
                    _displayMode = AlphaNumericDisplayMode.Custom;

                    var parameters = receivedMessage.Message.Parameters;

                    if (parameters != null)
                    {
                        _display.Display(parameters["Payload"].ToString());
                    }
                }

                Log.Debug(Tag, "Message Received: " + receivedMessage.ToString());


            }
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
                    _weatherDevice.UpdateTelemetryData("Temperature", message.TemperatureReading);
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
                    _weatherDevice.UpdateTelemetryData("Pressure", message.PressureReading);
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

