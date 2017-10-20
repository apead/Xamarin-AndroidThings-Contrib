using Newtonsoft.Json;

namespace Xamarin.AndroidThings.WeatherStation.Hubs
{
    public class EnvironmentTelemetry
    {
        [JsonProperty("messageId")]
        public int MessageId { get; set; }

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("humidity")]
        public double Humidity { get; set; }

        [JsonProperty("pressure")]
        public double Pressure { get; set; }

    }
}