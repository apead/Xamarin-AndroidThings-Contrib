using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Xamarin.AndroidThings.WeatherStation.Hubs
{
 
 /// <summary>
    /// RemoteMonitoringDevice class
    /// Provides helper functions for easily connecting a device to Azure IoT Suite Remote Monitoring 
    /// </summary>
    public class RemoteMonitoringDevice
    {
        // Azure IoT Hub client
        private DeviceClient deviceClient;

        private EnvironmentTelemetry _currentTelemetry = new EnvironmentTelemetry();

        // Device Model values

        // Collection of telemetry data

        private string _deviceId;
        public string DeviceId
        {
            get { return _deviceId; }
            set
            {
                _deviceId = value;

                _currentTelemetry.DeviceId = value;
            }
        }

        public void UpdateTemperature(double temp)
        {
            _currentTelemetry.Temperature = temp;
        }

        public void UpdatePressure(double pressure)
        {
            _currentTelemetry.Pressure = pressure;
        }

        public void UpdateHumidity(double humidity)
        {
            _currentTelemetry.Humidity = humidity;
        }

        public string DeviceKey { get; set; }
        public string HostName { get; set; }

        // Users can decide when to start/stop sending telemetry data and at what frequency
        public bool SendTelemetryData { get; set; }
        public int SendTelemetryFreq { get; set; } = 1000;
        public bool IsConnected { get; set; } = false;

        // Sending and receiving tasks
        CancellationTokenSource TokenSource = new CancellationTokenSource();

        private byte[] Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public async void SendData(EnvironmentTelemetry data)
        {
            try
            {
                data.MessageId++;
                var msg = new Message(Serialize(data));
                if (deviceClient != null)
                {
                    await deviceClient.SendEventAsync(msg);
                    Debug.WriteLine("Sent message to IoT Hub:\n" + JsonConvert.SerializeObject(data));
                }
                else Debug.WriteLine("Connection To IoT Hub is not established. Cannot send message now");
                
            }
            catch (System.Exception e)
            {
                Debug.WriteLine("Exception while sending message to IoT Hub:\n" + e.Message.ToString());
            }
        }

        /// <summary>
        /// Create the Connection String out of the device credentials
        /// </summary>
        /// <returns></returns>
        private string ConnectionString()
        {
            return "HostName=" + this.HostName + ";DeviceId=" + this.DeviceId + ";SharedAccessKey=" + this.DeviceKey;
        }

        public bool Connect()
        {
            try
            {
                // Create Azure IoT Hub Client and open messaging channel
                deviceClient = DeviceClient.CreateFromConnectionString(ConnectionString(), TransportType.Http1);
                deviceClient.OpenAsync();
                IsConnected = true;
                
                CancellationToken ct = TokenSource.Token;
                // Create send task
                Task.Factory.StartNew(async()=> {
                    while (true)
                    {
                        if (SendTelemetryData)
                        {
                             // Send current telemetry data
                            SendData(_currentTelemetry);
                        }
                        await Task.Delay(SendTelemetryFreq);

                        if (ct.IsCancellationRequested)
                        {
                            // Cancel was called
                            Debug.WriteLine("Sending task canceled");
                            break;
                        }

                    }
                }, ct);

  
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while trying to connect to IoT Hub:" + e.Message.ToString());
                deviceClient = null;
                return false;
            }
            return true;
        }

        public bool Disconnect()
        {
            if (deviceClient != null)
            {
                try
                {
                    deviceClient.CloseAsync();
                    deviceClient = null;
                    IsConnected = false;
                }
                catch
                {
                    Debug.WriteLine("Error while trying close the IoT Hub connection");
                    return false;
                }
            }
            return true;
        }
    }
}
