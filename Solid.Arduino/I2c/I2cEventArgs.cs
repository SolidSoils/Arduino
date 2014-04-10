using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.I2C
{
    /// <summary>
    /// Event arguments passed to a <see cref="I2CReplyReceivedHandler"/> type event.
    /// </summary>
    public class I2CEventArgs : EventArgs
    {
        private readonly I2CReply _value;

        internal I2CEventArgs(I2CReply value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the I2C message value being received.
        /// </summary>
        public I2CReply Value { get { return _value; } }

    }
}
