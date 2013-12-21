using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    internal sealed class DigitalStateTracker : ObservableEventTracker<IFirmataProtocol, DigitalPortState>
    {
        #region Constructors

        internal DigitalStateTracker(IFirmataProtocol source)
            : base(source)
        {
            _trackingSource.DigitalStateReceived += Firmata_DigitalStateReceived;
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            if (!_isDisposed)
            {
                _trackingSource.DigitalStateReceived -= Firmata_DigitalStateReceived;
                base.Dispose();
            }
        }

        #endregion

        #region Private Methods

        void Firmata_DigitalStateReceived(object par_Sender, FirmataEventArgs<DigitalPortState> par_EventArgs)
        {
            _observers.ForEach(o => o.OnNext(par_EventArgs.Value));
        }

        #endregion
    }
}
