using System;
using System.IO.Ports;
using System.Linq;

namespace Solid.Arduino
{
    /// <summary>
    /// Represents a serial port connection.
    /// </summary>
    public class SerialConnection : SerialPort, ISerialConnection
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SerialConnection"/> class using the highest COM port available at 115,200 bits per second.
        /// </summary>
        public SerialConnection()
            : base(GetLastPortName(), (int) SerialBaudRate.Bps_115200)
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
            : base(portName, (int) baudRate)
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
}