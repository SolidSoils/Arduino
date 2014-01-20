using System;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents a Firmata message received from an Arduino or Arduino compatible system.
    /// </summary>
    public sealed class FirmataMessage
    {
        private readonly MessageType _type;
        private readonly ValueType _value;

        /// <summary>
        /// Initializes a new <see cref="FirmataMessage"/> instance.
        /// </summary>
        /// <param name="type">The type of message to be created.</param>
        internal FirmataMessage(MessageType type)
        {
            _type = type;
        }

        /// <summary>
        /// Initializes a new <see cref="FirmataMessage"/> instance.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        internal FirmataMessage(ValueType value, MessageType type)
        {
            _value = value;
            _type = type;
        }

        /// <summary>
        /// Gets the specific value delivered by the message.
        /// </summary>
        public ValueType Value { get { return _value; } }

        /// <summary>
        /// Gets the type enumeration of the message.
        /// </summary>
        public MessageType Type { get { return _type; } }
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
