using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.I2c
{
    /// <summary>
    /// Event arguments passed to a <see cref="I2cReplyReceivedHandler"/> type event.
    /// </summary>
    public sealed class I2cEventArgs : EventArgs
    {
        private readonly I2cReply _value;

        internal I2cEventArgs(I2cReply value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the I2C message value being received.
        /// </summary>
        public I2cReply Value { get { return _value; } }

    }
}
