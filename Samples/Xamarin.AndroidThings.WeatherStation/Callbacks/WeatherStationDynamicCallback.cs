using System;
using Android.Animation;
using Android.Hardware;
using Android.Runtime;
using Android.Util;
using Xamarin.AndroidThings.WeatherStation.Helpers;
using Xamarin.AndroidThings.WeatherStation.Messages;

namespace Xamarin.AndroidThings.WeatherStation.Callbacks
{
    public class WeatherDynamicSensorCallback : SensorManager.DynamicSensorCallback
    {
        private MainActivity _context;

        public WeatherDynamicSensorCallback(MainActivity context)
        {
            _context = context;
        }

        public override void OnDynamicSensorConnected(Sensor sensor)
        {
            if (sensor.Type == SensorType.AmbientTemperature)
            {
                _context.SensorManager.RegisterListener(new TemperatureListener(), sensor,
                    SensorDelay.Normal);
            }
            else if (sensor.Type == SensorType.Pressure)
            {
                _context.SensorManager.RegisterListener(new PressureListener(), sensor,
                    SensorDelay.Normal);

            }
        }
    }

    public class TemperatureListener : Java.Lang.Object, ISensorEventListener
    {
        public static string Tag = typeof(MainActivity).FullName;
        private double _lastTemperature;
        private double _updateThreshold = 0.5;


        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            Log.Debug(Tag, "accuracy changed: " + accuracy);
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Values.Count <= 0) return;


            var value = e.Values[0];

            var diff = Math.Abs(_lastTemperature - value);

            if (!(diff > _updateThreshold)) return;
            Log.Debug(Tag, "sensor changed: " + value);
            _lastTemperature = value;
            PubSubHandler.GetInstance().Publish(new TemperatureMessage(this, value));
        }
    }

    public class PressureListener : Java.Lang.Object, ISensorEventListener
    {
        public static string Tag = typeof(MainActivity).FullName;
        private double _lastPressure = 0;
        private double _updateThreshold = 0.5;


        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            Log.Debug(Tag, "accuracy changed: " + accuracy);
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Values.Count <= 0) return;

            var value = e.Values[0];

            var diff = Math.Abs(_lastPressure - value);

            if (value < MainActivity.BarometerRangeLow) return;
            if (value > MainActivity.BarometerRangeHigh) return;

            if (!(diff > _updateThreshold)) return;
            Log.Debug(Tag, "sensor changed: " + value);
            _lastPressure = value;
            PubSubHandler.GetInstance().Publish(new PressureMessage(this, value));
        }
}

    public class SlideUpdateListener : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
    {
        private MainActivity _context;

        public SlideUpdateListener(MainActivity context)
        {
            _context = context;
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            try
            {
                float v = (float)animation.AnimatedValue;
                _context.Speaker.Play(v);
            }
            catch (Exception e)
            {
                throw new Exception("Error sliding speaker", e);
            }
        }
    }
}

