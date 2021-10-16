using System;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Contains event data for a <see cref="AnalogStateReceivedHandler"/> and <see cref="DigitalStateReceivedHandler"/> type events.
    /// </summary>
    /// <typeparam name="T">Type of the event data</typeparam>
    /// <remarks>
    /// This class is primarily implemented by the <see cref="IFirmataProtocol.AnalogStateReceived"/> and <see cref="IFirmataProtocol.DigitalStateReceived"/> events.
    /// </remarks>
    /// <seealso cref="AnalogStateReceivedHandler"/>
    /// <seealso cref="DigitalStateReceivedHandler"/>
    public sealed class FirmataEventArgs<T> : EventArgs
        where T : class
    {
        private readonly T _value;

        internal FirmataEventArgs(T value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the received message.
        /// </summary>
        public T Value => _value;
    }
}
