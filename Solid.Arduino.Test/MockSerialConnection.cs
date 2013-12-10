using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Test
{
    internal class MockSerialConnection: ISerialConnection
    {
        private static readonly ConstructorInfo _serialDataReceivedEventArgsConstructor = typeof(SerialDataReceivedEventArgs)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SerialData) }, null);
        
        private bool _isOpen;
        private string _newLine = "\n";

        private readonly Queue<byte[]> _expectedRequestQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> _responseQueue = new Queue<byte[]>();

        private byte[] _currentRequest, _currentResponse;
        private int _responseByteCount, _currentResponseIndex, _currentRequestIndex;
        
        #region ISerialConnection Members

        public event SerialDataReceivedEventHandler DataReceived;

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public string NewLine
        {
            get { return _newLine; }
            set { _newLine = value; }
        }

        public int BytesToRead
        {
            get { return _responseByteCount; }
        }

        public void Open()
        {
            if (_isOpen)
                throw new InvalidOperationException("Connection is already open.");

            _isOpen = true;
        }

        public void Close()
        {
            _isOpen = false;
        }

        public int ReadByte()
        {
            if (_responseByteCount == 0)
                throw new InvalidOperationException("No data.");

            if (_currentResponse == null || _currentResponseIndex >= _currentResponse.Length)
            {
                _currentResponse = _responseQueue.Dequeue();
                _currentResponseIndex = 0;
            }

            _responseByteCount--;
            return _currentResponse[_currentResponseIndex++];
        }

        public void Write(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (text.Length == 0)
                throw new ArgumentException("Text can not be empty.");

            foreach (char c in text.ToCharArray())
            {
                AssertEqualToExpectedRequestByte(Convert.ToByte(c));
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentException("offset");

            if (count < 1)
                throw new ArgumentException("count");

            if (count - offset > buffer.Length)
                throw new InvalidOperationException("Out of range");


            for (int x = 0; x < count; x++)
            {
                AssertEqualToExpectedRequestByte(buffer[x]);
            }
        }

        public void WriteLine(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (text.Length == 0)
                throw new ArgumentException("Text can not be empty.");

            foreach (char c in string.Concat(text, NewLine).ToCharArray())
            {
                AssertEqualToExpectedRequestByte(Convert.ToByte(c));
            }
        }

        #endregion

        #region Public Testmethods

        public void EnqueueResponse(params byte[] data)
        {
            _responseQueue.Enqueue(data);
            _responseByteCount += data.Length;
        }

        public void Enqueue14bitIsoResponse(string data)
        {
            byte[] dataBytes = new byte[data.Length * 2];

            for (int x = 0; x < data.Length; x++)
            {
                short c = Convert.ToInt16(data[x]);
                dataBytes[x * 2] = (byte)(c & 0x7F);
                dataBytes[x * 2 + 1] = (byte)((c >> 7) & 0x7F);
            }

            _responseQueue.Enqueue(dataBytes);
            _responseByteCount += dataBytes.Length;
        }

        public void EnqueueRequestAndResponse(byte[] request, params byte[] response)
        {
            _expectedRequestQueue.Enqueue(request);
            _responseQueue.Enqueue(response);
            _responseByteCount += response.Length;
        }

        #endregion

        #region Private Methods

        private void AssertEqualToExpectedRequestByte(byte p)
        {
            if (_currentRequest == null || _currentRequestIndex == _currentRequest.Length)
            {
                if (_expectedRequestQueue.Count < 1)
                    throw new InvalidOperationException("No request data expected.");

                _currentRequest = _expectedRequestQueue.Dequeue();
                _currentRequestIndex = 0;
            }

            if (p != _currentRequest[_currentRequestIndex])
                throw new InvalidOperationException(string.Format("Issued request byte {0:X} not equal to expected request byte {1:X}.", p, _currentRequest[_currentRequestIndex]));

            _currentRequestIndex++;

            if (_currentRequestIndex == _currentRequest.Length)
            {
                // Current request has been fully sent; now deliver response.
                while (_responseQueue.Count > 0)
                    ReceiveData(_responseQueue.Peek());
            }
        }

        private void ReceiveData(byte[] response)
        {
            if (this.DataReceived == null)
                return;

            ConstructorInfo _serialDataReceivedEventArgsConstructor = typeof(SerialDataReceivedEventArgs)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SerialData) }, null);

            var charReceivedEventArgs = (SerialDataReceivedEventArgs)_serialDataReceivedEventArgsConstructor.Invoke(new object[] { SerialData.Chars });

            for (int x = 0; x < response.Length; x++)
            {
                if (response[x] == 26)
                    DataReceived(this, (SerialDataReceivedEventArgs)_serialDataReceivedEventArgsConstructor.Invoke(new object[] { SerialData.Eof }));
                else
                    DataReceived(this, charReceivedEventArgs);
            }
        }

        #endregion
    }
}
