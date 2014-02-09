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
            TrackingSource.AnalogStateReceived += Firmata_AnalogStateReceived;
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                TrackingSource.AnalogStateReceived -= Firmata_AnalogStateReceived;
                base.Dispose();
            }
        }

        #endregion

        #region Private Methods

        void Firmata_AnalogStateReceived(object parSender, FirmataEventArgs<AnalogState> parEventArgs)
        {
            Observers.ForEach(o => o.OnNext(parEventArgs.Value));
        }

        #endregion
    }
}
