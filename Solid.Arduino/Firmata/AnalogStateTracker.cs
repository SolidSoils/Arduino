using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    internal sealed class AnalogStateTracker : ObservableEventTracker<IFirmataProtocol, AnalogState>
    {
        #region Constructors

        internal AnalogStateTracker(IFirmataProtocol source): base(source)
        {
            _trackingSource.AnalogStateReceived += Firmata_AnalogStateReceived;
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            if (!_isDisposed)
            {
                _trackingSource.AnalogStateReceived -= Firmata_AnalogStateReceived;
                base.Dispose();
            }
        }

        #endregion

        #region Private Methods

        void Firmata_AnalogStateReceived(object par_Sender, FirmataEventArgs<AnalogState> par_EventArgs)
        {
            _observers.ForEach(o => o.OnNext(par_EventArgs.Value));
        }

        #endregion
    }
}
