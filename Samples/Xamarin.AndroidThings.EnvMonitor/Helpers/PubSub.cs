using Messenger;

namespace Xamarin.AndroidThings.WeatherStation.Helpers
{
    public static class PubSubHandler
    {
        private static IMessenger _messenger = new MessengerHub();

        public static IMessenger GetInstance()
        {
            return _messenger;
        }
    }
}