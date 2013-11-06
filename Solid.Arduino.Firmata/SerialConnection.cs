using System;
using System.IO.Ports;
using System.Linq;

namespace Solid.Arduino.Firmata
{
    public sealed class SerialConnection: SerialPort, IDisposable
    {
        #region Constructors

        public SerialConnection()
            : base(GetLastPortName(), (int)SerialBaudRate.Bps_115200)
        {
            this.ReadTimeout = 100;
            this.WriteTimeout = 100;
        }

        public SerialConnection(string portName, SerialBaudRate baudRate)
            : base(portName, (int)baudRate)
        {
            this.ReadTimeout = 100;
            this.WriteTimeout = 100;
        }

        #endregion

        #region Fields

        #endregion

        #region Public Methods & Properties

        #endregion

        #region Private Methods

        private static string GetLastPortName()
        {
            return SerialPort.GetPortNames().Where(n => n.StartsWith("COM")).OrderByDescending(n => n).First();
        }

        #endregion
    }

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
