using System;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents a Firmata message received from an Arduino or Arduino compatible system.
    /// </summary>
    public sealed class FirmataMessage
    {
        private readonly MessageType _type;
        private readonly object _value;
        private readonly DateTime _time;

        /// <summary>
        /// Initializes a new <see cref="FirmataMessage"/> instance.
        /// </summary>
        /// <param name="type">The type of message to be created.</param>
        internal FirmataMessage(MessageType type)
            : this(null, type, DateTime.UtcNow) { }

        /// <summary>
        /// Initializes a new <see cref="FirmataMessage"/> instance.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        internal FirmataMessage(object value, MessageType type)
            : this(value, type, DateTime.UtcNow) { }

        /// <summary>
        /// Initializes a new <see cref="FirmataMessage"/> instance.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="time"></param>
        internal FirmataMessage(object value, MessageType type, DateTime time)
        {
            _value = value;
            _type = type;
            _time = time;
        }

        /// <summary>
        /// Gets the specific value delivered by the message.
        /// </summary>
        public object Value => _value;

        /// <summary>
        /// Gets the type enumeration of the message.
        /// </summary>
        public MessageType Type => _type;

        /// <summary>
        /// Gets the time of the delivered message.
        /// </summary>
        public DateTime Time => _time;
    }

    /// <summary>
    /// Indicates the type of a Firmata Message.
    /// </summary>
    public enum MessageType
    {
        AnalogState,
        DigitalPortState,
        ProtocolVersion,
        FirmwareResponse,
        CapabilityResponse,
        AnalogMappingResponse,
        PinStateResponse,
        StringData,
        I2CReply
    }
}
