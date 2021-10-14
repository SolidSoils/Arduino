namespace Solid.Arduino.Firmata
{
    internal class AnalogStateTracker : ObservableEventTracker<IFirmataProtocol, AnalogState>
    {
        private readonly int _channel;

        internal AnalogStateTracker(IFirmataProtocol source, int channel = -1)
            : base(source)
        {
            _channel = channel;
            TrackingSource.AnalogStateReceived += Firmata_AnalogStateReceived;
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                TrackingSource.AnalogStateReceived -= Firmata_AnalogStateReceived;
                base.Dispose();
            }
        }

        void Firmata_AnalogStateReceived(object parSender, FirmataEventArgs<AnalogState> parEventArgs)
        {
            if (_channel >= 0 && _channel != parEventArgs.Value.Channel)
                return;

            Observers.ForEach(o => o.OnNext(parEventArgs.Value));
        }
    }
}
