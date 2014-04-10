using System;
using System.IO.Ports;
using System.Linq;

namespace Solid.Arduino
{
    /// <summary>
    /// Represents a serial port connection.
    /// </summary>
    public class SerialConnection: SerialPort, ISerialConnection
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SerialConnection"/> class using the highest COM port available at 115,200 bits per second.
        /// </summary>
        public SerialConnection()
            : base(GetLastPortName(), (int)SerialBaudRate.Bps_115200)
        {
            ReadTimeout = 100;
            WriteTimeout = 100;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SerialConnection"/> class on the given serial port and at the given baud rate.
        /// </summary>
        /// <param name="portName">The port name (e.g. 'COM3')</param>
        /// <param name="baudRate">The baud rate</param>
        public SerialConnection(string portName, SerialBaudRate baudRate)
            : base(portName, (int)baudRate)
        {
            ReadTimeout = 100;
            WriteTimeout = 100;
        }

        #endregion

        #region Fields

        #endregion

        #region Public Methods & Properties

        /// <inheritdoc cref="SerialPort"/>
        public new void Close()
        {
            if (IsOpen)
            {
                BaseStream.Flush();
                DiscardInBuffer();
                base.Close();
            }
        }

        #endregion

        #region Private Methods

        private static string GetLastPortName()
        {
            return GetPortNames().Where(n => n.StartsWith("COM")).OrderByDescending(n => n).First();
        }

        #endregion
    }

    /// <summary>
    /// Enumeration of common baud rates, supported by Arduino boards
    /// </summary>
    public enum SerialBaudRate
    {
        Bps_300 = 300,
        Bps_600 = 600,
        Bps_1200 = 1200,
        Bps_2400 = 2400,
        Bps_4800 = 4800,
        Bps_9600 = 9600,
        Bps_14400 = 14400,
        Bps_19200 = 19200,
        Bps_28800 = 28800,
        Bps_31250 = 31250,
        Bps_38400 = 38400,
        Bps_57600 = 57600,
        Bps_115200 = 115200
    }
}
