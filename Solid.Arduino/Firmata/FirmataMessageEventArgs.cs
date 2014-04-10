using System;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Event arguments passed to a <see cref="MessageReceivedHandler"/> type event.
    /// </summary>
    /// <see cref="MessageReceivedHandler"/>
    public class FirmataMessageEventArgs : EventArgs
    {

        private readonly FirmataMessage _value;

        internal FirmataMessageEventArgs(FirmataMessage value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the received message.
        /// </summary>
        public FirmataMessage Value { get { return _value; } }
    }
}
