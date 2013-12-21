using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino
{
    internal sealed class ReceivedStringTracker : ObservableEventTracker<IStringProtocol, string>
    {
        #region Constructors

        internal ReceivedStringTracker(IStringProtocol source)
            : base(source)
        {
            _trackingSource.StringReceived += TrackingSource_StringReceived;
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            if (!_isDisposed)
            {
                _trackingSource.StringReceived -= TrackingSource_StringReceived;
                base.Dispose();
            }
        }

        #endregion

        #region Private Methods

        void TrackingSource_StringReceived(object par_Sender, StringEventArgs par_EventArgs)
        {
            _observers.ForEach(o => o.OnNext(par_EventArgs.Text));
        }

        #endregion
    }
}
