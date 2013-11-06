using System;

namespace Solid.Arduino.Firmata
{
    public sealed class FirmataMessage
    {
        private readonly MessageType _type;
        private readonly ValueType _value;

        public FirmataMessage(ValueType value, MessageType type)
        {
            _value = value;
            _type = type;
        }

        public ValueType Value { get { return _value; } }
        public MessageType Type { get { return _type; } }
    }

    public enum MessageType
    {
        AnalogState,
        DigitalState,
        ProtocolVersion,
        FirmwareResponse,
        CapabilityResponse,
        AnalogMappingResponse,
        PinStateResponse,
        StringData,
        I2CReply
    }
}
