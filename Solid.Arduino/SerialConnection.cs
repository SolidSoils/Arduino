using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;

using Solid.Arduino.Firmata;

namespace Solid.Arduino
{
    /// <summary>
    /// Represents a serial port connection.
    /// </summary>
    /// <inheritdoc cref="ISerialConnection" />
    public class SerialConnection : SerialPort, ISerialConnection
    {
        private static readonly SerialBaudRate[] PopularBaudRates =
        {
            SerialBaudRate.Bps_9600,
            SerialBaudRate.Bps_57600,
            SerialBaudRate.Bps_115200
        };

        private static readonly SerialBaudRate[] OtherBaudRates =
        {
            SerialBaudRate.Bps_28800,
            SerialBaudRate.Bps_14400,
            SerialBaudRate.Bps_38400,
            SerialBaudRate.Bps_31250,
            SerialBaudRate.Bps_4800,
            SerialBaudRate.Bps_2400
        };

        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of <see cref="SerialConnection"/> class using the highest COM-port available at 115,200 bits per second.
        /// </summary>
        public SerialConnection()
            : base(GetHighestComPortName(), (int) SerialBaudRate.Bps_115200)
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

        /// <inheritdoc cref="SerialPort" />
        public new void Open()
        {
            if (IsOpen)
                return;

            try
            {
                base.Open();
            }
            catch (UnauthorizedAccessException)
            {
                // Connection closure has probably not yet been finalized.
                // Wait 250 ms and try again once.
                Thread.Sleep(250);
                base.Open();
            }
        }

        /// <inheritdoc cref="SerialPort.Close"/>
        public new void Close()
        {
            if (!IsOpen)
                return;

            Thread.Sleep(250);
            BaseStream.Flush();
            DiscardInBuffer();
            BaseStream.Close();
            base.Close();
        }

        /// <inheritdoc cref="SerialPort.Dispose"/>
        public new void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            BaseStream.Dispose();
            GC.SuppressFinalize(BaseStream);
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="ISerialConnection.Write(string)" />
        /// <inheritdoc cref="ISerialConnection.Write(byte[],int,int)" />

        /// <summary>
        /// Finds a serial connection to a device supporting the Firmata protocol.
        /// </summary>
        /// <returns>A <see cref="ISerialConnection"/> instance or <c>null</c> if no connection is found</returns>
        /// <remarks>
        /// <para>
        /// This method searches all available serial ports until it finds a working serial connection.
        /// For every available serial port an attempt is made to open a connection at a range of common baudrates.
        /// The connection is tested by issueing an <see cref="IFirmataProtocol.GetFirmware()"/> command.
        /// (I.e. a Firmata SysEx Firmware query (0xF0 0x79 0xF7).)
        /// </para>
        /// <para>
        /// The connected device is expected to respond by sending the version number of the supported protocol.
        /// When a major version of 2 or higher is received, the connection is regarded to be valid.
        /// </para>
        /// </remarks>
        /// <seealso cref="IFirmataProtocol"/>
        /// <seealso href="http://www.firmata.org/wiki/Protocol#Query_Firmware_Name_and_Version">Query Firmware Name and Version</seealso>
        public static ISerialConnection Find()
        {
            bool isAvailableFunc(ArduinoSession session)
            {
                Firmware firmware = session.GetFirmware();
                return firmware.MajorVersion >= 2;
            }

            string[] portNames = GetPortNames();
            ISerialConnection connection = FindConnection(isAvailableFunc, portNames, PopularBaudRates);
            return connection ?? FindConnection(isAvailableFunc, portNames, OtherBaudRates);
        }

        /// <summary>
        /// Finds a serial connection to a device supporting plain serial communications.
        /// </summary>
        /// <param name="query">The query text used to inquire the connection</param>
        /// <param name="expectedReply">The reply text the connected device is expected to respond with</param>
        /// <returns>A <see cref="ISerialConnection"/> instance or <c>null</c> if no connection is found</returns>
        /// <remarks>
        /// <para>
        /// This method searches all available serial ports until it finds a working serial connection.
        /// For every available serial port an attempt is made to open a connection at a range of common baudrates.
        /// The connection is tested by sending the query string passed to this method.
        /// </para>
        /// <para>
        /// The connected device is expected to respond by sending the reply string passed to this method.
        /// When the string received is equal to the expected reply string, the connection is regarded to be valid.
        /// </para>
        /// </remarks>
        /// <example>
        /// The Arduino sketch below can be used to demonstrate this method.
        /// Upload the sketch to your Arduino device.
        /// <code lang="Arduino Sketch">
        /// char query[] = "Hello?";
        /// char reply[] = "Arduino!";
        ///
        /// void setup()
        /// {
        ///   Serial.begin(9600);
        ///   while (!Serial) {}
        /// }
        ///
        /// void loop()
        /// {
        ///   if (Serial.find(query))
        ///   {
        ///     Serial.println(reply);
        ///   }
        ///   else
        ///   {
        ///     Serial.println("Listening...");
        ///     Serial.flush();
        ///   }
        ///
        ///   delay(25);
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="IStringProtocol"/>
        public static ISerialConnection Find(string query, string expectedReply)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException(Messages.ArgumentEx_NotNullOrEmpty, nameof(query));

            if (string.IsNullOrEmpty(expectedReply))
                throw new ArgumentException(Messages.ArgumentEx_NotNullOrEmpty, nameof(expectedReply));

            bool isAvailableFunc(ArduinoSession session)
            {
                session.Write(query);
                return session.Read(expectedReply.Length) == expectedReply;
            }

            string[] portNames = GetPortNames();
            ISerialConnection connection = FindConnection(isAvailableFunc, portNames, PopularBaudRates);
            return connection ?? FindConnection(isAvailableFunc, portNames, OtherBaudRates);
        }

        private static string GetHighestComPortName()
        {
            return GetPortNames().Where(n => n.StartsWith("COM")).OrderByDescending(n => n).FirstOrDefault();
        }

        private static ISerialConnection FindConnection(Func<ArduinoSession, bool> isDeviceAvailable, string[] portNames, SerialBaudRate[] baudRates)
        {
            bool found = false;

            for (int x = portNames.Length - 1; x >= 0; x--)
            {
                foreach (SerialBaudRate rate in baudRates)
                {
                    try
                    {
                        using (var connection = new EnhancedSerialConnection(portNames[x], rate))
                        {
                            using (var session = new ArduinoSession(connection, 100))
                            {
#if TRACE
                                Debug.WriteLine("{0}:{1}; ", portNames[x], (int)rate);
#endif
                                if (isDeviceAvailable(session))
                                    found = true;
                            }
                        }

                        if (found)
                            return new EnhancedSerialConnection(portNames[x], rate);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Port is not available.
#if TRACE
                        Debug.WriteLine("{0} NOT AVAILABLE; ", portNames[x]);
#endif
                        break;
                    }
                    catch (TimeoutException)
                    {
                        // Baudrate or protocol error.
                    }
                    catch (IOException ex)
                    {
#if TRACE
                        Debug.WriteLine($"HResult 0x{ex.HResult:X} - {ex.Message}");
#endif
                    }
                }
            }
            return null;
        }
    }
}