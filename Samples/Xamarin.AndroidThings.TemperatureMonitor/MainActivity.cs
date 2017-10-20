using Android.App;
using Android.Widget;
using Android.OS;
using Com.Google.Android.Things.Contrib.Driver.Bmx280;
using Java.IO;

namespace Xamarin.AndroidThings.TemperatureMonitor
{
    [Activity(Label = "Xamarin.AndroidThings.TemperatureMonitor", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private Bmx280Contrib _bmx280;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            

            try
            {
                _bmx280 = new Bmx280Contrib(i2cBusName);
                _bmx280.se(Bmx280Contrib.Oversampling1x);
            }
            catch (IOException e)
            {
                // couldn't configure the device...
            }

            // Read the current temperature:

            try
            {
                float temperature = mBmx280.readTemperature();
            }
            catch (IOException e)
            {
                // error reading temperature
            }

            // Close the environmental sensor when finished:

            try
            {
                mBmx102.close();
            }
            catch (IOException e)
            {
                // error closing sensor
            }
        }
    }
}

