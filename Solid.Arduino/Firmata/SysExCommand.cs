namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// SysEx message command bytes
    /// </summary>
    public enum SysExCommand: byte
    {
        /// <summary>
        /// User defined ID 0x01
        /// </summary>
        UserDefined_0x01 = 0x01,

        /// <summary>
        /// User defined ID 0x02
        /// </summary>
        UserDefined_0x02 = 0x02,

        /// <summary>
        /// User defined ID 0x03
        /// </summary>
        UserDefined_0x03 = 0x03,

        /// <summary>
        /// User defined ID 0x04
        /// </summary>
        UserDefined_0x04 = 0x04,

        /// <summary>
        /// User defined ID 0x05
        /// </summary>
        UserDefined_0x05 = 0x05,

        /// <summary>
        /// User defined ID 0x06
        /// </summary>
        UserDefined_0x06 = 0x06,

        /// <summary>
        /// User defined ID 0x07
        /// </summary>
        UserDefined_0x07 = 0x07,

        /// <summary>
        /// User defined ID 0x08
        /// </summary>
        UserDefined_0x08 = 0x08,

        /// <summary>
        /// User defined ID 0x09
        /// </summary>
        UserDefined_0x09 = 0x09,

        /// <summary>
        /// User defined ID 0x0A
        /// </summary>
        UserDefined_0x0A = 0x0A,

        /// <summary>
        /// User defined ID 0x0B
        /// </summary>
        UserDefined_0x0B = 0x0B,

        /// <summary>
        /// User defined ID 0x0C
        /// </summary>
        UserDefined_0x0C = 0x0C,

        /// <summary>
        /// User defined ID 0x0D
        /// </summary>
        UserDefined_0x0D = 0x0D,

        /// <summary>
        /// User defined ID 0x0E
        /// </summary>
        UserDefined_0x0E = 0x0E,

        /// <summary>
        /// User defined ID 0x0F
        /// </summary>
        UserDefined_0x0F = 0x0F,

        /// <summary>
        /// Analog mapping query
        /// </summary>
        AnalogMappingQuery = 0x69,

        /// <summary>
        /// Analog mapping response
        /// </summary>
        AnalogMappingResponse = 0x6A,

        /// <summary>
        /// Capability query
        /// </summary>
        CapabilityQuery = 0x6B,

        /// <summary>
        /// Capability response
        /// </summary>
        CapabilityResponse = 0x6C,

        /// <summary>
        /// Pin state query
        /// </summary>
        PinStateQuery = 0x6D,

        /// <summary>
        /// Pin state response
        /// </summary>
        PinStateResponse = 0x6E,

        /// <summary>
        /// Extended analog
        /// </summary>
        ExtendedAnalog = 0x6F,

        /// <summary>
        /// String data
        /// </summary>
        StringData = 0x71,

        /// <summary>
        /// I2C request
        /// </summary>
        I2cRequest = 0x76,

        /// <summary>
        /// I2C reply
        /// </summary>
        I2cReply = 0x77,

        /// <summary>
        /// Report firmware
        /// </summary>
        ReportFirmware = 0x79,

        /// <summary>
        /// Sampling interval
        /// </summary>
        SamplingInterval = 0x7A,

        /// <summary>
        /// SysEx not-realtime
        /// </summary>
        SysExNonRealtime = 0x7E,

        /// <summary>
        /// SysEx realtime
        /// </summary>
        SysExRealtime = 0x7F,
    }
}
