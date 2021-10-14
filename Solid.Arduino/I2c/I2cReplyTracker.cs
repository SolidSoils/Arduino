namespace Solid.Arduino.I2C
{
    internal class I2CReplyTracker : ObservableEventTracker<II2CProtocol, I2CReply>
    {
        internal I2CReplyTracker(II2CProtocol ii2C): base(ii2C)
        {
            TrackingSource.I2CReplyReceived += I2CReplyReceived;
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                TrackingSource.I2CReplyReceived -= I2CReplyReceived;
                base.Dispose();
            }
        }

        private void I2CReplyReceived(object parSender, I2CEventArgs parEventArgs)
        {
            Observers.ForEach(o => o.OnNext(parEventArgs.Value));
        }
    }
}
