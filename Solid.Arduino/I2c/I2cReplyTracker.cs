using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.I2c
{
    internal sealed class I2cReplyTracker : ObservableEventTracker<II2cProtocol, I2cReply>
    {
        #region Constructors

        internal I2cReplyTracker(II2cProtocol i2c): base(i2c)
        {
            _trackingSource.I2cReplyReceived += I2cReplyReceived;
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            if (!_isDisposed)
            {
                _trackingSource.I2cReplyReceived -= I2cReplyReceived;
                base.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private void I2cReplyReceived(object par_Sender, I2cEventArgs par_EventArgs)
        {
            _observers.ForEach(o => o.OnNext(par_EventArgs.Value));
        }

        #endregion
    }
}
