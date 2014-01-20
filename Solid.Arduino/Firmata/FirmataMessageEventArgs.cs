using System;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Contains event data for a <see cref="MessageReceivedHandler"/> type event.
    /// </summary>
    /// <remarks>
    /// This class is primarily implemented by the <see cref="IFirmataProtocol.MessageReceived"/> event.
    /// </remarks>
    public sealed class FirmataMessageEventArgs : EventArgs
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
