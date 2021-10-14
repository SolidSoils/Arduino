namespace Solid.Arduino
{
    internal class ReceivedStringTracker : ObservableEventTracker<IStringProtocol, string>
    {
        #region Constructors

        internal ReceivedStringTracker(IStringProtocol source)
            : base(source)
        {
            TrackingSource.StringReceived += TrackingSource_StringReceived;
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                TrackingSource.StringReceived -= TrackingSource_StringReceived;
                base.Dispose();
            }
        }

        #endregion

        #region Private Methods

        void TrackingSource_StringReceived(object parSender, StringEventArgs parEventArgs)
        {
            Observers.ForEach(o => o.OnNext(parEventArgs.Text));
        }

        #endregion
    }
}
