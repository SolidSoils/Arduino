using System.IO.Ports;
using System.Linq;

namespace Solid.Arduino
{
    /// <summary>
    /// Represents a serial port connection, supporting Mono.
    /// </summary>
    /// <seealso href="http://www.mono-project.com/">The official Mono project site</seealso>
    /// <inheritdoc />
    public class EnhancedSerialConnection : EnhancedSerialPort, ISerialConnection
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EnhancedSerialConnection"/> class using the highest serial port available at 115,200 bits per second.
        /// </summary>
        public EnhancedSerialConnection()
            : base(GetLastPortName(), (int)SerialBaudRate.Bps_115200)
        {
            ReadTimeout = 100;
            WriteTimeout = 100;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EnhancedSerialConnection"/> class on the given serial port and at the given baud rate.
        /// </summary>
        /// <param name="portName">The port name (e.g. 'COM3')</param>
        /// <param name="baudRate">The baud rate</param>
        public EnhancedSerialConnection(string portName, SerialBaudRate baudRate)
            : base(portName, (int)baudRate)
        {
            ReadTimeout = 100;
            WriteTimeout = 100;
        }

        /// <inheritdoc cref="SerialConnection.Find()"/>
        public static ISerialConnection Find()
        {
            return SerialConnection.Find();
        }

        /// <inheritdoc cref="SerialConnection.Find(string, string)"/>
        public static ISerialConnection Find(string query, string expectedReply)
        {
            return SerialConnection.Find(query, expectedReply);
        }

        /// <inheritdoc cref="SerialPort.Close"/>
        public new void Close()
        {
            if (IsOpen)
            {
                BaseStream.Flush();
                DiscardInBuffer();
                base.Close();
            }
        }

        private static string GetLastPortName() => GetPortNames()
            .Where(p => p.StartsWith(@"/dev/ttyUSB") || p.StartsWith(@"/dev/ttyAMA") || p.StartsWith(@"/dev/ttyACM") || p.StartsWith("COM"))
            .OrderByDescending(p => p)
            .FirstOrDefault();
    }
}
