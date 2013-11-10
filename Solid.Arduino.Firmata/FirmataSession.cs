using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public class FirmataSession: IDisposable
    {
        #region Type declarations

        private enum MessageHeader
        {
            Undefined = 0x00,
            AnalogState = 0xE0,
            DigitalState = 0x90,
            ReportAnalog = 0xC0,
            ReportDigital = 0xD0,
            SystemExtension = 0xF0,
            SetPinMode = 0xF4,
            ProtocolVersion = 0xF9,
            SystemReset = 0xFF
        }

        private struct MessageBuffer
        {
            public MessageHeader CurrentMessage { get; private set; }
            public int DataByteIndex { get; private set; }
            public int[] Data;

            public void Prepare(int messageByte)
            {
                CurrentMessage = (MessageHeader)messageByte;

                switch (CurrentMessage)
                {
                    case MessageHeader.AnalogState:
                    case MessageHeader.DigitalState:
                    case MessageHeader.SystemExtension:
                    case MessageHeader.ProtocolVersion:
                        break;

                    default:
                        // No message header or message not supported.
                        CurrentMessage = MessageHeader.Undefined;
                        return;
                }

                Data[0] = messageByte;
                DataByteIndex = 1;
            }

            public void WriteDataByte(int dataByte)
            {
                if (DataByteIndex == BUFFERSIZE)
                    throw new OverflowException("The command parsing buffer is full.");

                Data[DataByteIndex] = dataByte;
                DataByteIndex++;
            }

            public void Clear()
            {
                CurrentMessage = MessageHeader.Undefined;
                DataByteIndex = 0;
            }
        }        

        public delegate void MessageReceivedHandler(object par_Sender, FirmataMessageEventArgs par_EventArgs);
        public delegate void AnalogStateMessageReceivedHandler(object par_Sender, FirmataMessageEventArgs<AnalogState> par_EventArgs);
        public delegate void DigitalStateMessageReceivedHandler(object par_Sender, FirmataMessageEventArgs<DigitalPortState> par_EventArgs);

        #endregion

        #region Fields

        private const byte AnalogMessage = 0xE0;
        private const byte DigitalMessage = 0x90;
        private const byte SysExStart = 0xF0;
        private const byte SysExEnd = 0xF7;

        private const int BUFFERSIZE = 512;

        private readonly SerialConnection _connection;
        private readonly bool _gotOpenConnection;
        private readonly Queue<FirmataMessage> _receivedMessageQueue = new Queue<FirmataMessage>();

        private int _messageTimeout = -1;
        private MessageBuffer _inputBuffer = new MessageBuffer { Data = new int[BUFFERSIZE] };

        #endregion

        #region Constructors

        public FirmataSession(SerialConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            _connection = connection;
            _gotOpenConnection = connection.IsOpen;

            if (!connection.IsOpen)
                _connection.Open();

            _connection.DataReceived += SerialDataReceived;
        }

        #endregion

        #region Public Events, Methods & Properties

        public int TimeOut
        {
            get { return _messageTimeout; }
            set
            {
                if (value < -1)
                    throw new InvalidOperationException();

                _messageTimeout = value;
            }
        }

        public event MessageReceivedHandler OnMessageReceived;
        public event AnalogStateMessageReceivedHandler OnAnalogStateMessageReceived;
        public event DigitalStateMessageReceivedHandler OnDigitalStateMessageReceived;

        public void SetAnalogLevel(int channel, ulong level)
        {
            if (channel < 0 || channel > 127U)
                throw new ArgumentOutOfRangeException("channel", "Channel number must be between 0 and 127.");

            byte[] message;

            if (channel < 16U && level < 0xC000)
            {
                // Send value in a conventional Analog Message.
                message = new byte[] {
                    (byte)(AnalogMessage | channel),
                    (byte)(level & 0x7F),
                    (byte)((level >> 7) & 0x7F)
                };
                _connection.Write(message, 0, 3);
                return;
            }

            // Send long value in an Extended Analog Message.
            message = new byte[14];
            message[0] = SysExStart;
            message[1] = (byte)0x6F;
            message[2] = (byte)channel;
            int index = 3;

            do
            {
                message[index] = (byte)(level & 0x7F);
                level >>= 7;
                index++;
            } while (level > 0);

            message[index] = SysExEnd;
            _connection.Write(message, 0, index + 1);
        }

        public void SetAnalogReportMode(int channel, bool enable)
        {
            if (channel < 0 || channel > 15)
                throw new ArgumentOutOfRangeException("channel", "Channel number must be in range 0 - 15.");

            _connection.Write(new byte[] { (byte)(0xC0 | channel), (byte)(enable ? 1 : 0) }, 0, 2);
        }

        public void SetDigitalPortState(int portNumber, uint pins)
        {
            if (portNumber < 0 || portNumber > 15)
                throw new ArgumentOutOfRangeException("portNumber", "Port number must be in range 0 - 15.");

            _connection.Write(new byte[] { (byte)(DigitalMessage | portNumber), (byte)(pins & 0x7F), (byte)((pins >> 7) & 0x7F) }, 0, 3);
        }

        public void SetDigitalReportMode(int portNumber, bool enable)
        {
            if (portNumber < 0 || portNumber > 15)
                throw new ArgumentOutOfRangeException("portNumber", "Port number must be in range 0 - 15.");

            _connection.Write(new byte[] { (byte)(0xD0 | portNumber), (byte)(enable ? 1 : 0) }, 0, 2);
        }

        public void SetPinMode(int pinNumber, PinMode mode)
        {
            if (pinNumber < 0 || pinNumber > 127)
                throw new ArgumentOutOfRangeException("pinNumber", "Pin number must be in range 0 - 127.");

            _connection.Write(new byte[] { 0xF4, (byte)pinNumber, (byte)mode }, 0, 3);
        }

        public void SetSamplingInterval(int milliseconds)
        {
            if (milliseconds < 1 || milliseconds > 0x3FFF)
                throw new ArgumentOutOfRangeException("milliseconds", "Sampling interval must be between 1 and 16,383 milliseconds.");

            var command = new byte[]
            {
                SysExStart,
                (byte)0x7A,
                (byte)(milliseconds & 0x7F),
                (byte)((milliseconds >> 7) & 0x7F),
                SysExEnd
            };
            _connection.Write(command, 0, 5);
        }

        public void ConfigureServo(int pinNumber, int minPulse, int maxPulse)
        {
            if (pinNumber < 0 || pinNumber > 127)
                throw new ArgumentOutOfRangeException("pinNumber", "Pin number must be between 0 and 127.");

            if (minPulse < 0 || minPulse > 0x3FFF)
                throw new ArgumentOutOfRangeException("minPulse", "Minimal pulse width must be between 0 and 16,383 milliseconds.");

            if (maxPulse < 0 || maxPulse > 0x3FFF)
                throw new ArgumentOutOfRangeException("maxPulse", "Maximal pulse width must be between 0 and 16,383 milliseconds.");

            if (minPulse > maxPulse)
                throw new ArgumentException("Minimal pulse width is greater than maximal pulse width.");

            var command = new byte[]
            {
                SysExStart,
                (byte)0x70,
                (byte)pinNumber,
                (byte)(minPulse & 0x7F),
                (byte)((minPulse >> 7) & 0x7F),
                (byte)(maxPulse & 0x7F),
                (byte)((maxPulse >> 7) & 0x7F),
                SysExEnd
            };
            _connection.Write(command, 0, 7);
        }

        public void ResetSystem()
        {
            _connection.Write(new byte[] { (byte)0xFF }, 0, 1);
        }

        public void SendStringData(string data)
        {
            // TODO: Test if text length does not exceed host's capability.
            if (data == null)
                data = string.Empty;

            byte[] command = new byte[data.Length * 2 + 3];
            command[0] = SysExStart;
            command[1] = (byte)0x71;

            for (int x = 0; x < data.Length; x++)
            {
                short c = Convert.ToInt16(data[x]);
                command[x * 2 + 2] = (byte)(c & 0x7F);
                command[x * 2 + 3] = (byte)((c >> 7) & 0x7F);
            }

            command[command.Length - 1] = SysExEnd;

            _connection.Write(command, 0, command.Length);
        }

        public void I2cSetInterval(int microseconds)
        {
            if (microseconds < 0 || microseconds > 0x3FFF)
                throw new ArgumentOutOfRangeException("microseconds", "Interval must be between 0 and 16,383 milliseconds.");

            var command = new byte[]
            {
                SysExStart,
                (byte)0x78,
                (byte)(microseconds & 0x7F),
                (byte)((microseconds >> 7) & 0x7F),
                SysExEnd
            };
            _connection.Write(command, 0, 5);
        }

        public void I2cWrite(int slaveAddress, byte[] data)
        {
            if (slaveAddress < 0 || slaveAddress > 0x3FF)
                throw new ArgumentOutOfRangeException("slaveAddress", "Address must be in range of 0 and 1,023.");

            // TODO: Test if data length does not exceed host's capability.
            if (data == null)
                throw new ArgumentNullException("data");

            byte[] command = new byte[data.Length * 2 + 5];
            command[0] = SysExStart;
            command[1] = (byte)0x76;
            command[2] = (byte)(slaveAddress & 0x7F);
            command[3] = (byte)(((slaveAddress >> 7) & 0x03) | 0x20);

            for (int x = 0; x < data.Length; x++)
            {
                command[x * 2 + 4] = (byte)(data[x] & 0x7F);
                command[x * 2 + 5] = (byte)((data[x] >> 7) & 0x7F);
            }

            command[command.Length - 1] = SysExEnd;

            _connection.Write(command, 0, command.Length);
        }

        public void I2cReadOnce(int slaveAddress, int bytesToRead)
        {
            I2cRead(false, slaveAddress, -1, bytesToRead);
        }

        public void I2cReadOnce(int slaveAddress, int slaveRegister, int bytesToRead)
        {
            I2cRead(false, slaveAddress, slaveRegister, bytesToRead);
        }

        public void I2cReadContinuous(int slaveAddress, int bytesToRead)
        {
            I2cRead(true, slaveAddress, -1, bytesToRead);
        }

        public void I2cReadContinuous(int slaveAddress, int slaveRegister, int bytesToRead)
        {
            I2cRead(true, slaveAddress, slaveRegister, bytesToRead);
        }

        public void I2cStopReading()
        {
            // The Firmata specification states that the I2c_read_stop message
            // should only stop the specified query. However, the current Firmata.h implementation
            // stops all registered queries.
            byte[] command = new byte[5];
            command[0] = SysExStart;
            command[1] = (byte)0x76;
            command[2] = (byte)0x00;
            command[3] = (byte)0x18;
            command[4] = SysExEnd;

            _connection.Write(command, 0, command.Length);
        }

        public void RequestFirmware()
        {
            SendSysExCommand(0x79);
        }

        public void RequestBoardCapability()
        {
            SendSysExCommand(0x6B);
        }

        public void RequestBoardAnalogMapping()
        {
            SendSysExCommand(0x69);
        }

        public void RequestDigitalPinState(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 15)
                throw new ArgumentOutOfRangeException("pinNumber", "Pin number must be in range 0 - 15.");

            var command = new byte[]
            {
                SysExStart,
                (byte)0x6D,
                (byte)pinNumber,
                SysExEnd
            };
            _connection.Write(command, 0, 4);
        }

        public async Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int bytesToRead)
        {
            I2cReadOnce(slaveAddress, bytesToRead);

            return await new Task<I2cReply>(() =>
                (I2cReply)((FirmataMessage)GetMessageFromQueue(MessageType.I2CReply)).Value);
        }

        public async Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int slaveRegister, int bytesToRead)
        {
            I2cReadOnce(slaveAddress, slaveRegister, bytesToRead);

            return await new Task<I2cReply>(() =>
                (I2cReply)((FirmataMessage)GetMessageFromQueue(MessageType.I2CReply)).Value);
        }

        public async Task<Firmware> GetFirmwareAsync()
        {
            RequestFirmware();

            return await new Task<Firmware>(() =>
                (Firmware)((FirmataMessage)GetMessageFromQueue(MessageType.FirmwareResponse)).Value);
        }

        public async Task<BoardCapability> GetBoardCapabilityAsync()
        {
            RequestBoardCapability();

            return await new Task<BoardCapability>(() =>
                (BoardCapability)((FirmataMessage)GetMessageFromQueue(MessageType.CapabilityResponse)).Value);
        }

        public async Task<BoardAnalogMapping> GetBoardAnalogMappingAsync()
        {
            RequestBoardAnalogMapping();

            return await new Task<BoardAnalogMapping>(() =>
                (BoardAnalogMapping)((FirmataMessage)GetMessageFromQueue(MessageType.AnalogMappingResponse)).Value);
        }

        public async Task<DigitalPortState> GetDigitalPinStateAsync(int pinNumber)
        {
            RequestDigitalPinState(pinNumber);

            return await new Task<DigitalPortState>(() =>
                (DigitalPortState)((FirmataMessage)GetMessageFromQueue(MessageType.DigitalState)).Value);
        }

        /*
         * TODO:
         *    
         * 2. verzending en ontvangst van spontane messages implementeren:
         *    a. StringData
         * 
         * 3. asynchrone query's implementeren.
              a. I2C Request/Reply
         *
         * Methode bedenken om onverwerkte messages van de queue te halen.
         * Monitor-messages verwerken.
         * Async query's moeten met async/await opgehaald kunnen worden.
         * Firmata protocol als interface definiëren.
         * I2C Read event implementeren.
         * I2C protocol als aparte interface definiëren.
         * Instelbare timeout intervals implementeren.
         */

        public void Dispose()
        {
            if (!_gotOpenConnection)
            {
                _connection.Close();
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        private object GetMessageFromQueue(MessageType typeRequested)
        {
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(_receivedMessageQueue, _messageTimeout, ref lockTaken);

                while (lockTaken)
                {
                    if (_receivedMessageQueue.Count > 0
                        && _receivedMessageQueue.Peek().Type == typeRequested)
                    {
                        FirmataMessage message = _receivedMessageQueue.Dequeue();
                        Monitor.PulseAll(_receivedMessageQueue);
                        return message;
                    }

                    lockTaken = Monitor.Wait(_receivedMessageQueue, _messageTimeout);
                }

                throw new TimeoutException(string.Format("Wait condition for {0} message timed out.", typeRequested));
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_receivedMessageQueue);
                }
            }
        }

        private void SendSysExCommand(byte command)
        {
            var message = new byte[]
            {
                SysExStart,
                (byte)command,
                SysExEnd
            };
            _connection.Write(message, 0, 3);
        }

        private void I2cRead(bool continuous, int slaveAddress, int slaveRegister = -1, int bytesToRead = 0)
        {
            if (slaveAddress < 0 || slaveAddress > 0x3FF)
                throw new ArgumentOutOfRangeException("slaveAddress", "Address must be in range of 0 and 1,023.");

            if (slaveRegister < -1 || slaveRegister > 0x3FFF)
                throw new ArgumentOutOfRangeException("slaveRegister", "Value must be between 0 and 16,383.");

            if (bytesToRead < 0 || bytesToRead > 0x3FFF)
                throw new ArgumentOutOfRangeException("bytesToRead", "Value must be between 0 and 16,383.");

            byte[] command = new byte[(slaveRegister == -1 ? 7 : 9)];
            command[0] = SysExStart;
            command[1] = (byte)0x76;
            command[2] = (byte)(slaveAddress & 0x7F);
            command[3] = (byte)(((slaveAddress >> 7) & 0x03) | (continuous ? 0x30 : 0x28));

            if (slaveRegister != -1)
            {
                command[4] = (byte)(slaveRegister & 0x7F);
                command[5] = (byte)(slaveRegister >> 7);
                command[6] = (byte)(bytesToRead & 0x7F);
                command[7] = (byte)(bytesToRead >> 7);
            }
            else
            {
                command[4] = (byte)(bytesToRead & 0x7F);
                command[5] = (byte)(bytesToRead >> 7);
            }

            command[command.Length - 1] = SysExEnd;

            _connection.Write(command, 0, command.Length);
        }

        /// <summary>
        /// Processes data bytes received on the serial bus.
        /// </summary>
        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (_connection.BytesToRead > 0)
            {
                int serialByte = _connection.ReadByte();

                if (_inputBuffer.CurrentMessage == MessageHeader.Undefined)
                {
                    _inputBuffer.Prepare(serialByte);
                    continue;
                }

                switch (_inputBuffer.CurrentMessage)
                {
                    case MessageHeader.AnalogState:
                        ProcessAnalogStateMessage(serialByte);
                        break;

                    case MessageHeader.DigitalState:
                        ProcessDigitalStateMessage(serialByte);
                        break;

                    case MessageHeader.SystemExtension:
                        ProcessSysExMessage(serialByte);
                        break;

                    case MessageHeader.ProtocolVersion:
                        ProcessProtocolVersionMessage(serialByte);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void ProcessAnalogStateMessage(int messageByte)
        {
            if (_inputBuffer.DataByteIndex < 2)
            {
                _inputBuffer.WriteDataByte(messageByte);
            }
            else
            {
                var currentState = new AnalogState
                {
                    Pin = _inputBuffer.Data[0] & 0x0F,
                    Level = (ulong)(_inputBuffer.Data[1] | (messageByte << 7))
                };
                _inputBuffer.Clear();

                if (OnMessageReceived != null)
                    OnMessageReceived(this, new FirmataMessageEventArgs(new FirmataMessage(currentState, MessageType.AnalogState)));

                if (OnAnalogStateMessageReceived != null)
                    OnAnalogStateMessageReceived(this, new FirmataMessageEventArgs<AnalogState>(currentState));
            }
        }

        private void ProcessDigitalStateMessage(int messageByte)
        {
            if (_inputBuffer.DataByteIndex < 2)
            {
                _inputBuffer.WriteDataByte(messageByte);
            }
            else
            {
                var currentState = new DigitalPortState
                {
                    Port = _inputBuffer.Data[0] & 0x0F,
                    Pins = _inputBuffer.Data[1] | (messageByte << 7)
                };
                _inputBuffer.Clear();

                if (OnMessageReceived != null)
                    OnMessageReceived(this, new FirmataMessageEventArgs(new FirmataMessage(currentState, MessageType.DigitalState)));

                if (OnDigitalStateMessageReceived != null)
                    OnDigitalStateMessageReceived(this, new FirmataMessageEventArgs<DigitalPortState>(currentState));
            }
        }

        private void ProcessProtocolVersionMessage(int messageByte)
        {
            if (_inputBuffer.DataByteIndex < 2)
            {
                _inputBuffer.WriteDataByte(messageByte);
            }
            else
            {
                var version = new ProtocolVersion {
                    MajorVersion = _inputBuffer.Data[1],
                    MinorVersion = messageByte
                };

                var message = new FirmataMessage(version, MessageType.ProtocolVersion);

                _inputBuffer.Clear();

                lock (_receivedMessageQueue)
                {
                    _receivedMessageQueue.Enqueue(message);
                    Monitor.PulseAll(_receivedMessageQueue);
                }

                if (OnMessageReceived != null)
                    OnMessageReceived(this, new FirmataMessageEventArgs(message));
            }
        }

        private void ProcessSysExMessage(int messageByte)
        {
            if (messageByte != SysExEnd)
            {
                _inputBuffer.WriteDataByte(messageByte);
                return;
            }

            FirmataMessage message;

            switch (_inputBuffer.Data[1])
            {
                case 0x6A: // AnalogMappingResponse
                    message = CreateAnalogMappingResponse();
                    break;

                case 0x6C: // CapabilityResponse
                    message = CreateCapabilityResponse();
                    break;

                case 0x6E: // PinStateResponse
                    message = CreatePinStateResponse();
                    break;

                case 0x71: // StringData
                    message = CreateStringDataMessage();
                    break;

                case 0x77: // I2cReply
                    message = CreateI2cReply();
                    break;

                case 0x79: // FirmwareReport
                    message = CreateFirmwareResponse();
                    break;

                default: // Unknown or unsupported message
                    // Just ignore.
                    _inputBuffer.Clear();
                    return;
            }

            _inputBuffer.Clear();

            lock (_receivedMessageQueue)
            {
                _receivedMessageQueue.Enqueue(message);
                Monitor.PulseAll(_receivedMessageQueue);
            }

            if (OnMessageReceived != null)
                OnMessageReceived(this, new FirmataMessageEventArgs(message));
        }

        private FirmataMessage CreateI2cReply()
        {
            var message = new I2cReply
            {
                Address = _inputBuffer.Data[2] | (_inputBuffer.Data[3] << 7),
                Register = _inputBuffer.Data[4] | (_inputBuffer.Data[5] << 7)
            };

            var data = new byte[(_inputBuffer.DataByteIndex - 5) >> 2];

            for (int x = 0; x < data.Length; x++)
            {
                data[x - 5] = (byte)(_inputBuffer.Data[x * 2 + 6] | _inputBuffer.Data[x * 2 + 7] << 7);
            }
            
            message.Data = data;
            return new FirmataMessage(message, MessageType.I2CReply);
        }

        private FirmataMessage CreatePinStateResponse()
        {
            var pinState = new PinState
            {
                PinNumber = _inputBuffer.Data[2],
                Mode = (PinMode)_inputBuffer.Data[3],
                Value = (ulong)_inputBuffer.Data[_inputBuffer.DataByteIndex - 1]
            };

            for (int x = _inputBuffer.DataByteIndex - 2; x > 3; x--)
            {
                pinState.Value <<= 7;
                pinState.Value += (ulong)_inputBuffer.Data[x];
            }

            return new FirmataMessage(pinState, MessageType.PinStateResponse);
        }

        private FirmataMessage CreateAnalogMappingResponse()
        {
            var pins = new List<AnalogPinMapping>(8);

            for (int x = 2; x < _inputBuffer.DataByteIndex; x++)
            {
                if (_inputBuffer.Data[x] != 0x7F)
                {
                    pins.Add
                    (
                        new AnalogPinMapping
                        {
                            PinNumber = x - 2,
                            Channel = _inputBuffer.Data[x]
                        }
                    );
                }
            }

            var board = new BoardAnalogMapping { PinMappings = pins.ToArray() };
            return new FirmataMessage(board, MessageType.AnalogMappingResponse);
        }

        private FirmataMessage CreateCapabilityResponse()
        {
            var pins = new List<PinCapability>(16);
            int pinIndex = 0;
            int x = 2;

            while (x <= _inputBuffer.DataByteIndex)
            {
                var capability = new PinCapability { PinNumber = pinIndex };
                int pinMode = 0;

                while (x < _inputBuffer.DataByteIndex && _inputBuffer.Data[x] != 127)
                {
                    bool isCapable = (_inputBuffer.Data[x] != 0);

                    switch ((PinMode)pinMode)
                    {
                        case PinMode.Analog:
                            capability.Analog = isCapable;
                            capability.AnalogResolution = _inputBuffer.Data[x + 1];
                            break;

                        case PinMode.Input:
                            capability.Input = isCapable;
                            break;

                        case PinMode.Output:
                            capability.Output = isCapable;
                            break;

                        case PinMode.Pwm:
                            capability.Pwm = isCapable;
                            capability.PwmResolution = _inputBuffer.Data[x + 1];
                            break;

                        case PinMode.Servo:
                            capability.Servo = isCapable;
                            capability.ServoResolution = _inputBuffer.Data[x + 1];
                            break;

                        default:
                            // Ignore unsupported capability.
                            break;
                    }

                    x += 2;
                }

                pins.Add(capability);
                x++;
            }

            var board = new BoardCapability { PinCapabilities = pins.ToArray() };

            return new FirmataMessage(board, MessageType.CapabilityResponse);
        }

        private FirmataMessage CreateStringDataMessage()
        {
            var builder = new StringBuilder(_inputBuffer.DataByteIndex >> 1);

            for (int x = 2; x < _inputBuffer.DataByteIndex; x += 2)
            {
                builder.Append((char)_inputBuffer.Data[x] | (_inputBuffer.Data[x + 1] << 7));
            }

            var stringData = new StringData
            {
                Text = builder.ToString()
            };
            return new FirmataMessage(stringData, MessageType.StringData);
        }

        private FirmataMessage CreateFirmwareResponse()
        {
            var firmware = new Firmware
            {
                MajorVersion = _inputBuffer.Data[2],
                MinorVersion = _inputBuffer.Data[3]
            };

            var builder = new StringBuilder(_inputBuffer.DataByteIndex);

            for (int x = 4; x < _inputBuffer.DataByteIndex; x++ )
            {
                builder.Append((char)_inputBuffer.Data[x]);
            }

            firmware.Name = builder.ToString();
            return new FirmataMessage(firmware, MessageType.FirmwareResponse);
        }

        #endregion
    }

    public enum PinMode
    {
        Input = 0,
        Output = 1,
        Analog = 2,
        Pwm = 3,
        Servo = 4
    }
}
