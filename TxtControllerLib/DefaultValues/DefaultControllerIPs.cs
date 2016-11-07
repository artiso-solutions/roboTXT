using System.Net;

namespace RoboticsTxt.Lib.DefaultValues
{
    /// <summary>
    /// Default values for the IP addresses used by the "Robotics TXT" controller.
    /// </summary>
    public static class DefaultControllerIPs
    {
        /// <summary>
        /// IP used for a USB connection.
        /// </summary>
        public static IPAddress DefaultUSBAddress => IPAddress.Parse("192.168.7.2");

        /// <summary>
        /// IP used for a WLan connection.
        /// </summary>
        public static IPAddress DefaultWLanAddress => IPAddress.Parse("192.168.8.2");

        /// <summary>
        /// IP used for a Bluetooth connection.
        /// </summary>
        public static IPAddress DefaultBluetoothAddress => IPAddress.Parse("192.168.9.2");
    }
}