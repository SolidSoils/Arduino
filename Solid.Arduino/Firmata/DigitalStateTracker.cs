using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    internal class DigitalStateTracker : ObservableEventTracker<IFirmataProtocol, DigitalPortState>
    {
        #region Constructors

        internal DigitalStateTracker(IFirmataProtocol source)
            : base(source)
        {
            TrackingSource.DigitalStateReceived += Firmata_DigitalStateReceived;
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                TrackingSource.DigitalStateReceived -= Firmata_DigitalStateReceived;
                base.Dispose();
            }
        }

        #endregion

        #region Private Methods

        void Firmata_DigitalStateReceived(object parSender, FirmataEventArgs<DigitalPortState> parEventArgs)
        {
            Observers.ForEach(o => o.OnNext(parEventArgs.Value));
        }

        #endregion
    }
}
