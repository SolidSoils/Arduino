using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solid.Arduino.Test
{
    internal class MockSerialConnection: ISerialConnection
    {
        private static readonly ConstructorInfo SerialDataReceivedEventArgsConstructor = typeof(SerialDataReceivedEventArgs)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SerialData) }, null);
        
        private bool _isOpen;
        private string _newLine = "\n";

        private readonly Queue<byte[]> _expectedRequestQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> _responseQueue = new Queue<byte[]>();

        private byte[] _currentRequest, _currentResponse;
        private int _responseByteCount, _currentResponseIndex, _currentRequestIndex;
        
        #region ISerialConnection Members

        public int BaudRate
        {
            get { return 9600; }
            set {}
        }

        public string PortName
        {
            get { return "COM3"; }
            set {}
        }

        public event SerialDataReceivedEventHandler DataReceived;

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        internal int QueuedRequestCount
        {
            get { return _expectedRequestQueue.Count; }
        }

        public string NewLine
        {
            get { return _newLine; }
            set { _newLine = value; }
        }

        public int BytesToRead
        {
            get
            {
                return _responseByteCount;
            }
        }

        public void Open()
        {
            if (_isOpen)
                throw new InvalidOperationException("MOCK VALIDATION: Connection is already open.");

            _isOpen = true;
        }

        public void Close()
        {
            _isOpen = false;
        }

        public int ReadByte()
        {
            if (_responseByteCount == 0)
                throw new InvalidOperationException("MOCK VALIDATION: No data.");

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
                throw new ArgumentNullException("MOCK VALIDATION: text");

            if (text.Length == 0)
                throw new ArgumentException("MOCK VALIDATION: Text can not be empty.");

            foreach (char c in text.ToCharArray())
            {
                AssertEqualToExpectedRequestByte(Convert.ToByte(c));
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("MOCK VALIDATION: buffer");

            if (offset < 0)
                throw new ArgumentException("MOCK VALIDATION: offset");

            if (count < 1)
                throw new ArgumentException("MOCK VALIDATION: count");

            if (count - offset > buffer.Length)
                throw new InvalidOperationException("MOCK VALIDATION: Out of range");


            for (int x = 0; x < count; x++)
            {
                AssertEqualToExpectedRequestByte(buffer[x]);
            }
        }

        public void WriteLine(string text)
        {
            if (text == null)
                text = string.Empty;

            foreach (char c in string.Concat(text, NewLine).ToCharArray())
            {
                AssertEqualToExpectedRequestByte(Convert.ToByte(c));
            }
        }

        public void Dispose()
        {

        }

        #endregion

        #region Public Testmethods

        public void EnqueueResponse(params byte[] data)
        {
            _responseQueue.Enqueue(data);
            _responseByteCount += data.Length;
        }

        public void EnqueueRequest(params byte[] request)
        {
            _expectedRequestQueue.Enqueue(request);
        }

        public void EnqueueRequestAndResponse(byte[] request, params byte[] response)
        {
            _expectedRequestQueue.Enqueue(request);

            if (response.Length > 0)
            {
                _responseQueue.Enqueue(response);
                _responseByteCount += response.Length;
            }
        }

        public void EnqueueStringResponse(string data)
        {
            _responseQueue.Enqueue(Encoding.ASCII.GetBytes(data));
            _responseByteCount += data.Length;
        }

        public void EnqueueStringRequest(string data)
        {
            _expectedRequestQueue.Enqueue(Encoding.ASCII.GetBytes(data));
        }

        public void HandleStringResponse(string response, Action handler)
        {
            EnqueueStringResponse(response);

            Task t = Task.Run(handler);

            while (t.Status != TaskStatus.Running)
                Thread.Sleep(1);

            Thread.Sleep(3);

            while (_responseQueue.Count > 0)
                ReceiveData(_responseQueue.Peek());

            try
            {
                t.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public void MockReceiveDelayed(string data)
        {
            Thread.Sleep(20);

            _responseQueue.Enqueue(Encoding.ASCII.GetBytes(data));
            _responseByteCount += data.Length;

            while (_responseQueue.Count > 0)
                ReceiveData(_responseQueue.Peek());
        }

        #endregion

        #region Private Methods

        private void AssertEqualToExpectedRequestByte(byte p)
        {
            if (_currentRequest == null || _currentRequestIndex == _currentRequest.Length)
            {
                if (_expectedRequestQueue.Count < 1)
                    throw new InvalidOperationException("MOCK VALIDATION: No request data expected.");

                _currentRequest = _expectedRequestQueue.Dequeue();
                _currentRequestIndex = 0;
            }

            if (p != _currentRequest[_currentRequestIndex])
                throw new InvalidOperationException(string.Format("MOCK VALIDATION: Issued request byte {0:X} not equal to expected request byte {1:X}.", p, _currentRequest[_currentRequestIndex]));

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
            if (this.DataReceived == null
                || response.Length == 0)
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
