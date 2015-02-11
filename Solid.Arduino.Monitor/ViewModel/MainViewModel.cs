using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using System.Linq;
using Solid.Arduino;

namespace Solid.Arduino.Monitor.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IObserver<string>
    {
        #region Fields

        private string _lastSerialMessageReceived;
        private readonly ObservableCollection<string> _serialMessages = new ObservableCollection<string>(); 

        #endregion

        #region Constructors

        public MainViewModel()
        {
            var connection = new SerialConnection("COM3", SerialBaudRate.Bps_115200);
            var session = new ArduinoSession(connection);
            session.CreateReceivedStringMonitor().Subscribe(this);
        }

        #endregion

        #region Public Interface

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(string value)
        {
            _serialMessages.Add(value);
            LastSerialMessageReceived = value;
        }

        public ObservableCollection<string> SerialMessages
        {
            get { return _serialMessages; }
        }

        public string LastSerialMessageReceived
        {
            get { return _lastSerialMessageReceived; }
            private set { Set(() => LastSerialMessageReceived, ref _lastSerialMessageReceived, value); }
        }

        #endregion
    }
}