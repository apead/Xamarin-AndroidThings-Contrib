using Messenger;

namespace Xamarin.AndroidThings.WeatherStation.Messages
{
    public class PressureMessage : MessengerMessage
    {
        public float PressureReading { get; set; }

        public PressureMessage(object sender, float pressure) : base(sender)
        {
            PressureReading = pressure;
        }
    }
}