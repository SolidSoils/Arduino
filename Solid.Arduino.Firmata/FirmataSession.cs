using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public class FirmataSession: IDisposable
    {
        #region Type declarations

        private enum MessageCommand
        {
            Undefined,
            AnalogState,
            DigitalState,
            ProtocolVersion,
            SysExStart
        }

        private struct MessageBuffer
        {
            public MessageCommand CurrentMessage { get; private set; }
            public int DataByteIndex { get; private set; }
            public int[] DataBuffer;

            public void Prepare(int messageByte)
            {
                const int analogState = 0xE0;
                const int digitalState = 0x90;
                const int sysExStart = 0xF0;
                const int protocolVersion = 0xF9;

                switch (messageByte & 0xF0)
                {
                    case analogState:
                        CurrentMessage = MessageCommand.AnalogState;
                        break;

                    case digitalState:
                        CurrentMessage = MessageCommand.DigitalState;
                        break;

                    case sysExStart:
                        if (messageByte == sysExStart)
                            CurrentMessage = MessageCommand.SysExStart;
                        else if (messageByte == protocolVersion)
                            CurrentMessage = MessageCommand.ProtocolVersion;
                        break;

                    default:
                        // No message header or message not supported.
                        return;
                }

                DataBuffer[0] = messageByte;
                DataByteIndex = 1;
            }

            public void WriteDataByte(int dataByte)
            {
                if (DataByteIndex == BUFFERSIZE)
                    throw new OverflowException("The command parsing buffer is full.");

                DataBuffer[DataByteIndex] = dataByte;
                DataByteIndex++;
            }

            public void Clear()
            {
                CurrentMessage = MessageCommand.Undefined;
                DataByteIndex = 0;
            }
        }        

        public delegate void MessageReceivedHandler(object par_Sender, FirmataMessageEventArgs par_EventArgs);
        public delegate void AnalogStateMessageReceivedHandler(object par_Sender, FirmataMessageEventArgs<AnalogState> par_EventArgs);
        public delegate void DigitalStateMessageReceivedHandler(object par_Sender, FirmataMessageEventArgs<DigitalState> par_EventArgs);

        #endregion

        #region Fields

        private const byte AnalogMessage = 0xE0;
        private const byte DigitalMessage = 0x90;
        private const byte ReportAnalog = 0xC0;
        private const byte ReportDigital = 0xD0;
        private const byte SysExStart = 0xF0;
        private const byte SysExEnd = 0xF7;
        private const byte ProtocolVersion = 0xF9;
        private const byte SystemReset = 0xFF;

        private const int BUFFERSIZE = 512;

        private readonly SerialConnection _connection;
        private readonly bool _gotOpenConnection;
        private readonly Queue<FirmataMessage> _receivedMessageQueue = new Queue<FirmataMessage>();

        private MessageBuffer _inputBuffer = new MessageBuffer { DataBuffer = new int[BUFFERSIZE] };

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

        public event MessageReceivedHandler OnMessageReceived;
        public event AnalogStateMessageReceivedHandler OnAnalogStateMessageReceived;
        public event DigitalStateMessageReceivedHandler OnDigitalStateMessageReceived;

        public void SetAnalogLevel(int pinNumber, ulong level)
        {
            if (pinNumber < 0 || pinNumber > 127U)
                throw new ArgumentOutOfRangeException("pinNumber", "Pin number must be between 0 and 127.");

            byte[] message;

            if (pinNumber < 16U && level < 0xC000)
            {
                // Send value in a conventional Analog Message.
                message = new byte[] {
                    (byte)(AnalogMessage | pinNumber),
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
            message[2] = (byte)pinNumber;
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

        public void SetDigitalPinStates(int portNumber, uint pins)
        {
            if (portNumber < 0 || portNumber > 15)
                throw new ArgumentOutOfRangeException("portNumber", "Port number must be in range 0 - 15.");

            _connection.Write(new byte[] { (byte)(AnalogMessage | portNumber), (byte)(pins & 0x7F), (byte)((pins >> 7) & 0x7F) }, 0, 3);
        }

        public void SetAnalogReportMode(int pinNumber, bool enable)
        {
            if (pinNumber < 0 || pinNumber > 15)
                throw new ArgumentOutOfRangeException("pinNumber", "Pin number must be in range 0 - 15.");

            const int ReportAnalogPinCommand = 0xC0;
            _connection.Write(new byte[] { (byte)(ReportAnalogPinCommand | pinNumber), (byte)(enable ? 1 : 0) }, 0, 2);
        }

        public void SetDigitalReportMode(int portNumber, bool enable)
        {
            if (portNumber < 0 || portNumber > 15)
                throw new ArgumentOutOfRangeException("portNumber", "Port number must be in range 0 - 15.");

            const int ReportDigitalPinCommand = 0xD0;
            _connection.Write(new byte[] { (byte)(ReportDigitalPinCommand | portNumber), (byte)(enable ? 1 : 0) }, 0, 2);
        }

        public void SetDigitalPinMode(int pinNumber, PinMode mode)
        {
            if (pinNumber < 0 || pinNumber > 127)
                throw new ArgumentOutOfRangeException("pinNumber", "Pin number must be in range 0 - 127.");

            const byte SetPinModeCommand = 0xF4;
            _connection.Write(new byte[] { SetPinModeCommand, (byte)pinNumber, (byte)mode }, 0, 3);
        }

        public void SetSamplingInterval(int milliseconds)
        {
            if (milliseconds < 1 || milliseconds > 0x3FFF)
                throw new ArgumentOutOfRangeException("milliseconds", "Interval must be between 1 and 16,383 milliseconds.");

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

        public void SetServoConfig(int pinNumber, int minPulse, int maxPulse)
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

        public void SendText(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            byte[] body = Encoding.Unicode.GetBytes(text);
            byte[] command = new byte[body.Length + 3];
            command[0] = SysExStart;
            command[1] = (byte)0x71;
            body.CopyTo(command, 2);
            command[body.Length + 2] = SysExEnd;

            _connection.Write(command, 0, command.Length);
        }

        public async Task<int> GetDigitalPinStateAsync(int pinNumber)
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

            throw new NotImplementedException();
        }

        public void GetFirmware()
        {
            var command = new byte[]
            {
                SysExStart,
                (byte)0x79,
                SysExEnd
            };
            _connection.Write(command, 0, 3);

        }

        /*
         * TODO:
         * 1. void commands implementeren:
         *    a. I2CConfig
         *    
         * 2. verzending en ontvangst van spontane messages implementeren:
         *    a. StringData (Gedaan, maar wordt dit ondersteund door de Arduino?)
         *    b. ShiftData
         * 
         * 3. asynchrone query's implementeren.
              a. Firmware Request/Response,
              b. Capability Request/Response,
              c. AnalogMapping Request/Response,
              d. PinState Request/Response,
              e. I2C Request/Reply
         *
         * Async query's moeten met async/await opgehaald kunnen worden.
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

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Deze methode kan de volgende berichten verwerken:
            // - DigitalIoMessage (3 bytes)
            // - AnalogIoMessage (3 bytes)
            // - VersionReportMessage (3 bytes)
            // - SysEx response messages (variabel aantal bytes)  

            while (_connection.BytesToRead > 0)
            {
                int serialByte = _connection.ReadByte();

                if (_inputBuffer.CurrentMessage == MessageCommand.Undefined)
                {
                    _inputBuffer.Prepare(serialByte);
                    continue;
                }

                switch (_inputBuffer.CurrentMessage)
                {
                    case MessageCommand.AnalogState:
                        ProcessAnalogStateMessage(serialByte);
                        break;

                    case MessageCommand.DigitalState:
                        ProcessDigitalStateMessage(serialByte);
                        break;

                    case MessageCommand.SysExStart:
                        ProcessSysExMessage(serialByte);
                        break;

                    case MessageCommand.ProtocolVersion:
                        ProcessProtocolVersionMessage(serialByte);
                        break;

                    default:
                        // No message identified. Therefore ignore received data.
                        break;
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
                    Pin = _inputBuffer.DataBuffer[0] & 0x0F,
                    Level = (ulong)(_inputBuffer.DataBuffer[1] | (messageByte << 7))
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
                var currentState = new DigitalState
                {
                    Port = _inputBuffer.DataBuffer[0] & 0x0F,
                    Pins = _inputBuffer.DataBuffer[1] | (messageByte << 7)
                };
                _inputBuffer.Clear();

                if (OnMessageReceived != null)
                    OnMessageReceived(this, new FirmataMessageEventArgs(new FirmataMessage(currentState, MessageType.DigitalState)));

                if (OnDigitalStateMessageReceived != null)
                    OnDigitalStateMessageReceived(this, new FirmataMessageEventArgs<DigitalState>(currentState));
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
                    MajorVersion = _inputBuffer.DataBuffer[1],
                    MinorVersion = messageByte
                };

                var message = new FirmataMessage(version, MessageType.ProtocolVersion);

                _inputBuffer.Clear();
                _receivedMessageQueue.Enqueue(message);

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

            switch (_inputBuffer.DataBuffer[1])
            {
                case 0x6A: // AnalogMappingResponse

                    return;

                case 0x6C: // CapabilityResponse

                    return;

                case 0x6E: // PinStateResponse

                    return;

                case 0x71: // StringData

                    return;

                case 0x77: // I2cReply

                    return;

                case 0x79: // FirmwareReport
                    message = CreateFirmwareMessage();
                    break;

                default: // Unknown or unsupported message
                    // Just ignore.
                    _inputBuffer.Clear();
                    return;
            }

            _inputBuffer.Clear();
            _receivedMessageQueue.Enqueue(message);

            if (OnMessageReceived != null)
                OnMessageReceived(this, new FirmataMessageEventArgs(message));

        }

        private FirmataMessage CreateFirmwareMessage()
        {
            var firmware = new Firmware
            {
                MajorVersion = _inputBuffer.DataBuffer[2],
                MinorVersion = _inputBuffer.DataBuffer[3]
            };

            var builder = new StringBuilder(_inputBuffer.DataByteIndex);

            for (int x = 4; x <= _inputBuffer.DataByteIndex; x++ )
            {
                builder.Append((char)_inputBuffer.DataBuffer[x]);
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
        PWM = 3,
        Servo = 4
    }

    public enum MessageType
    {
        AnalogState,
        DigitalState,
        ProtocolVersion,
        FirmwareResponse,
        CapabilityResponse,
        AnalogMappingResponse,
        PinStateResponse,
        StringMessage,
        I2CReply
    }

    public sealed class FirmataMessage
    {
        private readonly MessageType _type;
        private readonly ValueType _value;

        public FirmataMessage(ValueType value, MessageType type)
        {
            _value = value;
            _type = type;
        }

        public ValueType Value { get { return _value; } }
        public MessageType Type { get { return _type; } }
    }

    public class FirmataMessageEventArgs : EventArgs
    {

        private readonly FirmataMessage _value;

        public FirmataMessageEventArgs(FirmataMessage value)
        {
            _value = value;
        }

        public FirmataMessage Value { get { return _value; } }
    }

    public class FirmataMessageEventArgs<T> : EventArgs
        where T: struct
    {
        private readonly T _value;

        public FirmataMessageEventArgs(T value)
        {
            _value = value;
        }

        public T Value { get { return _value; } }
    }

    public struct AnalogState
    {
        public int Pin { get; set; }
        public ulong Level { get; set; }
    }

    public struct DigitalState
    {
        public int Port { get; set; }
        public int Pins { get; set; }

        public bool IsHigh(int pin)
        {
            if (pin < 0 || pin > 7)
              throw new ArgumentOutOfRangeException("pin", "Pin must be in range 0 - 7.");

            return (Pins & 1 << pin) > 0;
        }
    }

    public struct ProtocolVersion
    {
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
    }

    public struct Firmware
    {
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public string Name { get; set; }
    }
}
