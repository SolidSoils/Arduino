using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Solid.Arduino.Firmata;
using Solid.Arduino.I2c;

namespace Solid.Arduino
{
    public class FirmataSession: IFirmataProtocol, IStringProtocol, IDisposable
    {
        #region Type declarations

        private delegate void ProcessMessageHandler(int messageByte);

        private enum MessageHeader
        {
            Undefined = 0x00,
            AnalogState = 0xE0, // 224
            DigitalState = 0x90, // 144
            ReportAnalog = 0xC0,
            ReportDigital = 0xD0,
            SystemExtension = 0xF0,
            SetPinMode = 0xF4,
            ProtocolVersion = 0xF9,
            SystemReset = 0xFF
        }

        private enum StringReadMode {
            ReadLine,
            ReadToTerminator,
            ReadBlock
        }

        private sealed class StringRequest
        {
            private readonly StringReadMode _mode;
            private readonly int _blockLength;
            private readonly char _terminator;

            public static StringRequest CreateReadLineRequest()
            {
                return new StringRequest(StringReadMode.ReadLine, '\\', 0);
            }

            public static StringRequest CreateReadRequest(int blockLength)
            {
                return new StringRequest(StringReadMode.ReadBlock, '\\', blockLength);
            }

            public static StringRequest CreateReadRequest(char terminator)
            {
                return new StringRequest(StringReadMode.ReadBlock, terminator, 0);
            }

            private StringRequest(StringReadMode mode, char terminator, int blockLength)
            {
                _mode = mode;
                _blockLength = blockLength;
                _terminator = terminator;
            }

            public char Terminator { get { return _terminator; } }
            public int BlockLength { get { return _blockLength; } }
            public StringReadMode Mode { get { return _mode; } }
        }

        #endregion

        #region Fields

        private const byte AnalogMessage = 0xE0;
        private const byte DigitalMessage = 0x90;
        private const byte VersionReportHeader = 0xF9;
        private const byte SysExStart = 0xF0;
        private const byte SysExEnd = 0xF7;

        private const int BUFFERSIZE = 512;
        private const int MAXQUEUELENGTH = 100;

        private readonly ISerialConnection _connection;
        private readonly bool _gotOpenConnection;
        private readonly Queue<FirmataMessage> _receivedMessageQueue = new Queue<FirmataMessage>();
        private readonly Queue<string> _receivedStringQueue = new Queue<string>();
        private ConcurrentQueue<FirmataMessage> _awaitedMessagesQueue = new ConcurrentQueue<FirmataMessage>();
        private ConcurrentQueue<StringRequest> _awaitedStringsQueue = new ConcurrentQueue<StringRequest>();
        private StringRequest _currentStringRequest;

        private int _messageTimeout = -1;
        private ProcessMessageHandler _processMessage;
        private int _messageBufferIndex, _stringBufferIndex;
        private readonly int[] _messageBuffer = new int[BUFFERSIZE];
        private readonly char[] _stringBuffer = new char[BUFFERSIZE];

        #endregion

        #region Constructors

        public FirmataSession(ISerialConnection connection)
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

        public void Clear()
        {
            lock (_receivedMessageQueue)
            {
                _connection.BaseStream.Flush();
                _connection.DiscardInBuffer();
                _receivedMessageQueue.Clear();
                _processMessage = null;
                _awaitedMessagesQueue = new ConcurrentQueue<FirmataMessage>();
                _awaitedStringsQueue = new ConcurrentQueue<StringRequest>();
            }
        }

        #region IStringProtocol

        public event StringReceivedHandler StringReceived;

        public string NewLine
        {
            get { return _connection.NewLine; }
            set { _connection.NewLine = value; }
        }

        public void Write(string value)
        {
            if (!string.IsNullOrEmpty(value))// TODO: ASCII schrijven! (Althans...)
                _connection.Write(value);
        }

        public void WriteLine(string value)
        {
            if (!string.IsNullOrEmpty(value))
                _connection.WriteLine(value);
        }

        public string ReadLine()
        {
            return GetStringFromQueue(StringRequest.CreateReadLineRequest());
        }

        public async Task<string> ReadLineAsync()
        {
            return await Task.Run<string>(() => GetStringFromQueue(StringRequest.CreateReadLineRequest()));
        }

        public string Read(int length = 1)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", Messages.ArgumentEx_PositiveValue);

            return GetStringFromQueue(StringRequest.CreateReadRequest(length));
        }

        public async Task<string> ReadAsync(int length = 1)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", Messages.ArgumentEx_PositiveValue);

            return await Task.Run<string>(() => GetStringFromQueue(StringRequest.CreateReadRequest(length)));
        }

        public string ReadTo(char terminator)
        {
            return GetStringFromQueue(StringRequest.CreateReadRequest(terminator));
        }

        public async Task<string> ReadToAsync(char terminator)
        {
            return await Task.Run<string>(() => GetStringFromQueue(StringRequest.CreateReadRequest(terminator)));
        }

        #endregion

        #region IFirmataProtocol

        public event MessageReceivedHandler OnMessageReceived;
        public event AnalogStateReceivedHandler OnAnalogStateReceived;
        public event DigitalStateReceivedHandler OnDigitalStateReceived;

        public void ResetBoard()
        {
            _connection.Write(new byte[] { (byte)0xFF }, 0, 1);
        }

        public void SetDigitalPin(int pinNumber, long level)
        {
            if (pinNumber < 0 || pinNumber > 127)
                throw new ArgumentOutOfRangeException("pinNumber", Messages.ArgumentEx_PinRange0_127);

            if (level < 0)
                throw new ArgumentOutOfRangeException("level", Messages.ArgumentEx_NoNegativeValue);

            byte[] message;

            if (pinNumber < 16 && level < 0xC000)
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

        public void SetAnalogReportMode(int channel, bool enable)
        {
            if (channel < 0 || channel > 15)
                throw new ArgumentOutOfRangeException("channel", Messages.ArgumentEx_ChannelRange0_15);

            _connection.Write(new byte[] { (byte)(0xC0 | channel), (byte)(enable ? 1 : 0) }, 0, 2);
        }

        public void SetDigitalPort(int portNumber, int pins)
        {
            if (portNumber < 0 || portNumber > 15)
                throw new ArgumentOutOfRangeException("portNumber", Messages.ArgumentEx_PortRange0_15);

            _connection.Write(new byte[] { (byte)(DigitalMessage | portNumber), (byte)(pins & 0x7F), (byte)((pins >> 7) & 0x7F) }, 0, 3);
        }

        public void SetDigitalReportMode(int portNumber, bool enable)
        {
            if (portNumber < 0 || portNumber > 15)
                throw new ArgumentOutOfRangeException("portNumber", Messages.ArgumentEx_PortRange0_15);

            _connection.Write(new byte[] { (byte)(0xD0 | portNumber), (byte)(enable ? 1 : 0) }, 0, 2);
        }

        public void SetDigitalPinMode(int pinNumber, PinMode mode)
        {
            if (pinNumber < 0 || pinNumber > 127)
                throw new ArgumentOutOfRangeException("pinNumber", Messages.ArgumentEx_PinRange0_127);

            _connection.Write(new byte[] { 0xF4, (byte)pinNumber, (byte)mode }, 0, 3);
        }

        public void SetSamplingInterval(int milliseconds)
        {
            if (milliseconds < 1 || milliseconds > 0x3FFF)
                throw new ArgumentOutOfRangeException("milliseconds", Messages.ArgumentEx_SamplingInterval);

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
                throw new ArgumentOutOfRangeException("pinNumber", Messages.ArgumentEx_PinRange0_127);

            if (minPulse < 0 || minPulse > 0x3FFF)
                throw new ArgumentOutOfRangeException("minPulse", Messages.ArgumentEx_MinPulseWidth);

            if (maxPulse < 0 || maxPulse > 0x3FFF)
                throw new ArgumentOutOfRangeException("maxPulse", Messages.ArgumentEx_MaxPulseWidth);

            if (minPulse > maxPulse)
                throw new ArgumentException(Messages.ArgumentEx_MinMaxPulse);

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

        public void SendStringData(string data)
        {
            if (data == null)
                data = string.Empty;

            byte[] command = new byte[data.Length * 2 + 5];
            command[0] = SysExStart;
            command[1] = (byte)0x71;

            for (int x = 0; x < data.Length; x++)
            {
                short c = Convert.ToInt16(data[x]);
                command[x * 2 + 2] = (byte)(c & 0x7F);
                command[x * 2 + 3] = (byte)((c >> 7) & 0x7F);
            }

            command[command.Length - 3] = (byte)0x00;
            command[command.Length - 2] = (byte)0x00;
            command[command.Length - 1] = SysExEnd;

            _connection.Write(command, 0, command.Length);
        }

        public void RequestFirmware()
        {
            SendSysExCommand(0x79);
        }

        public Firmware GetFirmware()
        {
            RequestFirmware();
            return (Firmware)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.FirmwareResponse))).Value;
        }

        public async Task<Firmware> GetFirmwareAsync()
        {
            RequestFirmware();
            return await Task.Run<Firmware>(() =>
                (Firmware)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.FirmwareResponse))).Value);
        }

        public void RequestProtocolVersion()
        {
            _connection.Write(new byte[] { 0xF9 }, 0, 1);
        }

        public ProtocolVersion GetProtocolVersion()
        {
            RequestProtocolVersion();
            return (ProtocolVersion)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.ProtocolVersion))).Value;
        }

        public async Task<ProtocolVersion> GetProtocolVersionAsync()
        {
            RequestProtocolVersion();
            return await Task.Run<ProtocolVersion>(() =>
                (ProtocolVersion)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.ProtocolVersion))).Value);
        }

        public void RequestBoardCapability()
        {
            SendSysExCommand(0x6B);
        }

        public BoardCapability GetBoardCapability()
        {
            RequestBoardCapability();
            return (BoardCapability)((FirmataMessage) GetMessageFromQueue(new FirmataMessage(MessageType.CapabilityResponse))).Value;
        }

        public async Task<BoardCapability> GetBoardCapabilityAsync()
        {
            RequestBoardCapability();
            return await Task.Run<BoardCapability>(() =>
                (BoardCapability)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.CapabilityResponse))).Value);
        }

        public void RequestBoardAnalogMapping()
        {
            SendSysExCommand(0x69);
        }

        public BoardAnalogMapping GetBoardAnalogMapping()
        {
            RequestBoardAnalogMapping();
            return (BoardAnalogMapping)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.AnalogMappingResponse))).Value;
        }

        public async Task<BoardAnalogMapping> GetBoardAnalogMappingAsync()
        {
            RequestBoardAnalogMapping();
            return await Task.Run<BoardAnalogMapping>(() =>
                (BoardAnalogMapping)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.AnalogMappingResponse))).Value);
        }

        public void RequestPinState(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 127)
                throw new ArgumentOutOfRangeException("pinNumber", Messages.ArgumentEx_PinRange0_127);

            var command = new byte[]
            {
                SysExStart,
                (byte)0x6D,
                (byte)pinNumber,
                SysExEnd
            };
            _connection.Write(command, 0, 4);
        }

        public PinState GetPinState(int pinNumber)
        {
            RequestPinState(pinNumber);
            return (PinState)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.PinStateResponse))).Value;
        }

        public async Task<PinState> GetPinStateAsync(int pinNumber)
        {
            RequestPinState(pinNumber);
            return await Task.Run<PinState>(() =>
                (PinState)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.PinStateResponse))).Value);
        }

        #endregion

        #region II2cProtocol

        public event I2cReplyReceivedHandler I2cReplyReceived;

        public void I2cSetInterval(int microseconds)
        {
            if (microseconds < 0 || microseconds > 0x3FFF)
                throw new ArgumentOutOfRangeException("microseconds", Messages.ArgumentEx_I2cInterval);

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
                throw new ArgumentOutOfRangeException("slaveAddress", Messages.ArgumentEx_I2cAddressRange);

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

        public I2cReply GetI2cReply(int slaveAddress, int bytesToRead)
        {
            I2cReadOnce(slaveAddress, bytesToRead);
            _awaitedMessagesQueue.Enqueue(new FirmataMessage(MessageType.I2CReply));

            return (I2cReply)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.I2CReply))).Value;
        }

        public async Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int bytesToRead)
        {
            I2cReadOnce(slaveAddress, bytesToRead);
            _awaitedMessagesQueue.Enqueue(new FirmataMessage(MessageType.I2CReply));

            return await Task.Run<I2cReply>(() =>
                (I2cReply)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.I2CReply))).Value);
        }

        public void I2cReadOnce(int slaveAddress, int slaveRegister, int bytesToRead)
        {
            I2cRead(false, slaveAddress, slaveRegister, bytesToRead);
        }

        public I2cReply GetI2cReply(int slaveAddress, int slaveRegister, int bytesToRead)
        {
            I2cReadOnce(slaveAddress, slaveRegister, bytesToRead);
            _awaitedMessagesQueue.Enqueue(new FirmataMessage(MessageType.I2CReply));

            return (I2cReply)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.I2CReply))).Value;
        }

        public async Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int slaveRegister, int bytesToRead)
        {
            I2cReadOnce(slaveAddress, slaveRegister, bytesToRead);
            _awaitedMessagesQueue.Enqueue(new FirmataMessage(MessageType.I2CReply));

            return await Task.Run<I2cReply>(() =>
                (I2cReply)((FirmataMessage)GetMessageFromQueue(new FirmataMessage(MessageType.I2CReply))).Value);
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

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_gotOpenConnection)
            {
                _connection.Close();
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #region Private Methods

        private void WriteMessageByte(int dataByte)
        {
            if (_messageBufferIndex == BUFFERSIZE)
                throw new OverflowException(Messages.OverflowEx_CmdBufferFull);

            _messageBuffer[_messageBufferIndex] = dataByte;
            _messageBufferIndex++;
        }

        private string GetStringFromQueue(StringRequest request)
        {
            _awaitedStringsQueue.Enqueue(request);
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(_receivedStringQueue, _messageTimeout, ref lockTaken);

                while (lockTaken)
                {
                    if (_receivedStringQueue.Count > 0)
                    {
                        string message = _receivedStringQueue.Dequeue();
                        Monitor.PulseAll(_receivedStringQueue);
                        return message;
                    }

                    lockTaken = Monitor.Wait(_receivedStringQueue, _messageTimeout);
                }

                throw new TimeoutException(string.Format(Messages.TimeoutEx_WaitStringRequest, request.Mode));
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_receivedStringQueue);
                }
            }
        }

        private object GetMessageFromQueue(FirmataMessage awaitedMessage)
        {
            _awaitedMessagesQueue.Enqueue(awaitedMessage);
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(_receivedMessageQueue, _messageTimeout, ref lockTaken);

                while (lockTaken)
                {
                    if (_receivedMessageQueue.Count > 0
                        && _receivedMessageQueue.Peek().Type == awaitedMessage.Type)
                    {
                        FirmataMessage message = _receivedMessageQueue.Dequeue();
                        Monitor.PulseAll(_receivedMessageQueue);
                        return message;
                    }

                    lockTaken = Monitor.Wait(_receivedMessageQueue, _messageTimeout);
                }

                throw new TimeoutException(string.Format(Messages.TimeoutEx_WaitMessage, awaitedMessage.Type));
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
                throw new ArgumentOutOfRangeException("slaveAddress", Messages.ArgumentEx_I2cAddressRange);

            if (slaveRegister < -1 || slaveRegister > 0x3FFF)
                throw new ArgumentOutOfRangeException("slaveRegister", Messages.ArgumentEx_ValueRange0_16383);

            if (bytesToRead < 0 || bytesToRead > 0x3FFF)
                throw new ArgumentOutOfRangeException("bytesToRead", Messages.ArgumentEx_ValueRange0_16383);

            byte[] command = new byte[(slaveRegister == -1 ? 7 : 9)];
            command[0] = SysExStart;
            command[1] = (byte)0x76;
            command[2] = (byte)(slaveAddress & 0x7F);
            command[3] = (byte)(((slaveAddress >> 7) & 0x07) | (continuous ? 0x10 : 0x08));

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

#if DEBUG
                if (_messageBufferIndex > 0 && _messageBufferIndex % 8 == 0)
                    Console.WriteLine();

                Console.Write("{0:x2} ", serialByte);
#endif

                if (_processMessage != null)
                {
                    _processMessage(serialByte);
                }
                else
                {
                    if ((serialByte & 0x80) != 0)
                    {
                        // Process Firmata command byte.
                        ProcessCommand(serialByte);
                    }
                    else
                    {
                        // Process ASCII character.
                        ProcessAsciiString(serialByte);
                    }
                }
            }
        }

        private void ProcessAsciiString(int serialByte)
        {
            char c = Convert.ToChar(serialByte);

            if (_currentStringRequest == null)
                _awaitedStringsQueue.TryDequeue(out _currentStringRequest);

            if (_currentStringRequest == null)
            {
                if (c == _connection.NewLine[_connection.NewLine.Length - 1] || serialByte == 0x1A) // NewLine or EOF?
                {
                    if (StringReceived != null)
                        StringReceived(this, new StringEventArgs(new string(_stringBuffer, 0, _stringBufferIndex)));

                    _stringBufferIndex = 0;
                    return;
                }
            }
            else
            {
                switch (_currentStringRequest.Mode)
                {
                    case StringReadMode.ReadLine:
                        if (c == _connection.NewLine[0] || serialByte == 0x1A)
                        {
                            EnqueueString(new string(_stringBuffer, 0, _stringBufferIndex));
                            return;
                        }
                        if (c == '\r')
                            return;

                        break;

                    case StringReadMode.ReadBlock:
                        if (_stringBufferIndex == _currentStringRequest.BlockLength - 1)
                        {
                            _stringBuffer[_stringBufferIndex] = c;
                            EnqueueString(new string(_stringBuffer, 0, _stringBufferIndex + 1));
                            return;
                        }
                        break;

                    case StringReadMode.ReadToTerminator:
                        if (c == _currentStringRequest.Terminator)
                        {
                            EnqueueString(new string(_stringBuffer, 0, _stringBufferIndex));
                            return;
                        }
                        break;
                }
            }

            if (_stringBufferIndex == BUFFERSIZE)
                throw new OverflowException(Messages.OverflowEx_StringBufferFull);

            _stringBuffer[_stringBufferIndex] = c;
            _stringBufferIndex++;
        }

        private void EnqueueString(string value)
        {
            lock (_receivedStringQueue)
            {
                if (_receivedStringQueue.Count >= MAXQUEUELENGTH)
                    throw new OverflowException(Messages.OverflowEx_StringBufferFull);

                _receivedStringQueue.Enqueue(value);
                _currentStringRequest = null;
                _stringBufferIndex = 0;
                Monitor.PulseAll(_receivedStringQueue);
            }
        }

        private void ProcessCommand(int serialByte)
        {
            _messageBuffer[0] = serialByte;
            _messageBufferIndex = 1;
            MessageHeader header = (MessageHeader)(serialByte & 0xF0);

            switch (header)
            {
                case MessageHeader.AnalogState:
                    _processMessage = ProcessAnalogStateMessage;
                    break;

                case MessageHeader.DigitalState:
                    _processMessage = ProcessDigitalStateMessage;
                    break;

                case MessageHeader.SystemExtension:
                    header = (MessageHeader)serialByte;

                    switch (header)
                    {
                        case MessageHeader.SystemExtension:
                            _processMessage = ProcessSysExMessage;
                            break;

                        case MessageHeader.ProtocolVersion:
                            _processMessage = ProcessProtocolVersionMessage;
                            break;

                        case MessageHeader.SetPinMode:
                        case MessageHeader.SystemReset:
                        default:
                            // 0xF? command not supported.
                            throw new NotImplementedException(string.Format(Messages.NotImplementedEx_Command, serialByte));
                    }
                    break;

                default:
                    // Command not supported.
                    throw new NotImplementedException(string.Format(Messages.NotImplementedEx_Command, serialByte));
            }
        }

        private void ProcessAnalogStateMessage(int messageByte)
        {
            if (_messageBufferIndex < 2)
            {
                WriteMessageByte(messageByte);
            }
            else
            {
                var currentState = new AnalogState
                {
                    Channel = _messageBuffer[0] & 0x0F,
                    Level = (ulong)(_messageBuffer[1] | (messageByte << 7))
                };
                _processMessage = null;

                if (OnMessageReceived != null)
                    OnMessageReceived(this, new FirmataMessageEventArgs(new FirmataMessage(currentState, MessageType.AnalogState)));

                if (OnAnalogStateReceived != null)
                    OnAnalogStateReceived(this, new FirmataEventArgs<AnalogState>(currentState));
            }
        }

        private void ProcessDigitalStateMessage(int messageByte)
        {
            if (_messageBufferIndex < 2)
            {
                WriteMessageByte(messageByte);
            }
            else
            {
                var currentState = new DigitalPortState
                {
                    Port = _messageBuffer[0] & 0x0F,
                    Pins = _messageBuffer[1] | (messageByte << 7)
                };
                _processMessage = null;

                if (OnMessageReceived != null)
                    OnMessageReceived(this, new FirmataMessageEventArgs(new FirmataMessage(currentState, MessageType.DigitalPortState)));

                if (OnDigitalStateReceived != null)
                    OnDigitalStateReceived(this, new FirmataEventArgs<DigitalPortState>(currentState));
            }
        }

        private void ProcessProtocolVersionMessage(int messageByte)
        {
            if (_messageBufferIndex < 2)
            {
                WriteMessageByte(messageByte);
            }
            else
            {
                var version = new ProtocolVersion
                {
                    Major = _messageBuffer[1],
                    Minor = messageByte
                };
                DeliverMessage(new FirmataMessage(version, MessageType.ProtocolVersion));
            }
        }

        private void ProcessSysExMessage(int messageByte)
        {
            if (messageByte != SysExEnd)
            {
                WriteMessageByte(messageByte);
                return;
            }

            switch (_messageBuffer[1])
            {
                case 0x6A: // AnalogMappingResponse
                    DeliverMessage(CreateAnalogMappingResponse());
                    return;

                case 0x6C: // CapabilityResponse
                    DeliverMessage(CreateCapabilityResponse());
                    return;

                case 0x6E: // PinStateResponse
                    DeliverMessage(CreatePinStateResponse());
                    return;

                case 0x71: // StringData
                    DeliverMessage(CreateStringDataMessage());
                    return;

                case 0x77: // I2cReply
                    DeliverMessage(CreateI2cReply());
                    return;

                case 0x79: // FirmwareResponse
                    DeliverMessage(CreateFirmwareResponse());
                    return;

                default: // Unknown or unsupported message
                    throw new NotImplementedException();
            }
        }

        private void DeliverMessage(FirmataMessage message)
        {
            _processMessage = null;

            lock (_receivedMessageQueue)
            {
                if (_receivedMessageQueue.Count >= MAXQUEUELENGTH)
                    throw new OverflowException(Messages.OverflowEx_MsgBufferFull);

                _receivedMessageQueue.Enqueue(message);
                Monitor.PulseAll(_receivedMessageQueue);
            }

            if (OnMessageReceived != null && message.Type != MessageType.I2CReply)
                OnMessageReceived(this, new FirmataMessageEventArgs(message));
        }

        private FirmataMessage CreateI2cReply()
        {
            var reply = new I2cReply
            {
                Address = _messageBuffer[2] | (_messageBuffer[3] << 7),
                Register = _messageBuffer[4] | (_messageBuffer[5] << 7)
            };

            var data = new byte[(_messageBufferIndex - 5) / 2];

            for (int x = 0; x < data.Length; x++)
            {
                data[x] = (byte)(_messageBuffer[x * 2 + 6] | _messageBuffer[x * 2 + 7] << 7);
            }
            
            reply.Data = data;

            if (I2cReplyReceived != null)
                I2cReplyReceived(this, new I2cEventArgs(reply));

            return new FirmataMessage(reply, MessageType.I2CReply);
        }

        private FirmataMessage CreatePinStateResponse()
        {
            if (_messageBufferIndex < 5)
                throw new InvalidOperationException(Messages.InvalidOpEx_PinNotSupported);

            int value = 0;

            for (int x = _messageBufferIndex - 1; x > 3; x--)
            {
                value = (value << 7) | _messageBuffer[x];
            }

            var pinState = new PinState
            {
                PinNumber = _messageBuffer[2],
                Mode = (PinMode)_messageBuffer[3],
                Value = (ulong)value
            };
            return new FirmataMessage(pinState, MessageType.PinStateResponse);
        }

        private FirmataMessage CreateAnalogMappingResponse()
        {
            var pins = new List<AnalogPinMapping>(8);

            for (int x = 2; x < _messageBufferIndex; x++)
            {
                if (_messageBuffer[x] != 0x7F)
                {
                    pins.Add
                    (
                        new AnalogPinMapping
                        {
                            PinNumber = x - 2,
                            Channel = _messageBuffer[x]
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

            while (x < _messageBufferIndex)
            {
                var capability = new PinCapability { PinNumber = pinIndex };

                while (x < _messageBufferIndex && _messageBuffer[x] != 127)
                {
                    PinMode pinMode = (PinMode)_messageBuffer[x];
                    bool isCapable = (_messageBuffer[x + 1] != 0);

                    switch (pinMode)
                    {
                        case PinMode.AnalogInput:
                            capability.Analog = isCapable;
                            capability.AnalogResolution = _messageBuffer[x + 1];
                            break;

                        case PinMode.DigitalInput:
                            capability.Input = isCapable;
                            break;

                        case PinMode.DigitalOutput:
                            capability.Output = isCapable;
                            break;

                        case PinMode.PwmOutput:
                            capability.Pwm = isCapable;
                            capability.PwmResolution = _messageBuffer[x + 1];
                            break;

                        case PinMode.ServoControl:
                            capability.Servo = isCapable;
                            capability.ServoResolution = _messageBuffer[x + 1];
                            break;

                        default:
                            // Ignore unsupported capability.
                            break;
                    }

                    x += 2;
                }

                pins.Add(capability);
                pinIndex++;
                x++;
            }

            var board = new BoardCapability { PinCapabilities = pins.ToArray() };

            return new FirmataMessage(board, MessageType.CapabilityResponse);
        }

        private FirmataMessage CreateStringDataMessage()
        {
            var builder = new StringBuilder(_messageBufferIndex >> 1);

            for (int x = 2; x < _messageBufferIndex; x += 2)
            {
                builder.Append((char)(_messageBuffer[x] | (_messageBuffer[x + 1] << 7)));
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
                MajorVersion = _messageBuffer[2],
                MinorVersion = _messageBuffer[3]
            };

            var builder = new StringBuilder(_messageBufferIndex);

            for (int x = 4; x < _messageBufferIndex; x += 2 )
            {
                builder.Append((char)(_messageBuffer[x] | (_messageBuffer[x + 1] << 7)));
            }

            firmware.Name = builder.ToString();
            return new FirmataMessage(firmware, MessageType.FirmwareResponse);
        }

        #endregion

    }
}
