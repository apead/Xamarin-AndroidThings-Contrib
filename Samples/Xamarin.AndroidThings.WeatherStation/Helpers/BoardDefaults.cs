using Android.OS;
using Android.Things.Pio;
using Java.Lang;

namespace Xamarin.AndroidThings.WeatherStation.Helpers
{
    public class BoardDefaults
    {
    private const string DeviceEdisonArduino = "edison_arduino";
    private const string DeviceEdison = "edison";
    private const string DeviceJoule = "joule";
    private const string DeviceRpi3 = "rpi3";
    private const string DeviceImx6UlPico = "imx6ul_pico";
    private const string DeviceImx6UlVvdn = "imx6ul_iopb";
    private const string DeviceImx7DPico = "imx7d_pico";
    public static string BoardVariant = "";

        public static string GetButtonGpioPin()
        {
            switch (GetBoardVariant())
            {
                case DeviceEdisonArduino:
                    return "IO12";
                case DeviceEdison:
                    return "GP44";
                case DeviceJoule:
                    return "J7_71";
                case DeviceRpi3:
                    return "BCM21";
                case DeviceImx6UlPico:
                    return "GPIO4_IO20";
                case DeviceImx6UlVvdn:
                    return "GPIO3_IO01";
                case DeviceImx7DPico:
                    return "GPIO_174";
                default:
                    throw new IllegalArgumentException("Unknown device: " + Build.Device);
            }
        }

        public static string GetLedGpioPin()
        {
            switch (GetBoardVariant())
            {
                case DeviceEdisonArduino:
                    return "IO13";
                case DeviceEdison:
                    return "GP45";
                case DeviceJoule:
                    return "J6_25";
                case DeviceRpi3:
                    return "BCM6";
                case DeviceImx6UlPico:
                    return "GPIO4_IO21";
                case DeviceImx6UlVvdn:
                    return "GPIO3_IO06";
                case DeviceImx7DPico:
                    return "GPIO_34";
                default:
                    throw new IllegalArgumentException("Unknown device: " + Build.Device);
            }
        }

        public static string GetI2cBus()
        {
            switch (GetBoardVariant())
            {
                case DeviceEdisonArduino:
                    return "I2C6";
                case DeviceEdison:
                    return "I2C1";
                case DeviceJoule:
                    return "I2C0";
                case DeviceRpi3:
                    return "I2C1";
                case DeviceImx6UlPico:
                    return "I2C2";
                case DeviceImx6UlVvdn:
                    return "I2C4";
                case DeviceImx7DPico:
                    return "I2C1";
                default:
                    throw new IllegalArgumentException("Unknown device: " + Build.Device);
            }
        }

        public static string GetSpiBus()
        {
            switch (GetBoardVariant())
            {
                case DeviceEdisonArduino:
                    return "SPI1";
                case DeviceEdison:
                    return "SPI2";
                case DeviceJoule:
                    return "SPI0.0";
                case DeviceRpi3:
                    return "SPI0.0";
                case DeviceImx6UlPico:
                    return "SPI3.0";
                case DeviceImx6UlVvdn:
                    return "SPI1.0";
                case DeviceImx7DPico:
                    return "SPI3.1";
                default:
                    throw new IllegalArgumentException("Unknown device: " + Build.Device);
            }
        }

        public static string GetSpeakerPwmPin()
        {
            switch (GetBoardVariant())
            {
                case DeviceEdisonArduino:
                    return "IO3";
                case DeviceEdison:
                    return "GP13";
                case DeviceJoule:
                    return "PWM_0";
                case DeviceRpi3:
                    return "PWM1";
                case DeviceImx6UlPico:
                    return "PWM7";
                case DeviceImx6UlVvdn:
                    return "PWM3";
                case DeviceImx7DPico:
                    return "PWM2";
                default:
                    throw new IllegalArgumentException("Unknown device: " + Build.Device);
            }
        }

        private static string GetBoardVariant()
        {
            if (!string.IsNullOrEmpty(BoardVariant))
            {
                return BoardVariant;
            }
            BoardVariant = Build.Device;
            // For the edison check the pin prefix
            // to always return Edison Breakout pin name when applicable.
            if (!BoardVariant.Equals(DeviceEdison)) return BoardVariant;

            var pioService = new PeripheralManagerService();
            var gpioList = pioService.GpioList;
            if (gpioList.Count == 0) return BoardVariant;

            var pin = gpioList[0];
            if (pin.StartsWith("IO"))
                BoardVariant = DeviceEdisonArduino;

            return BoardVariant;
        }
    }
}