using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Signature of event handlers capable of processing I2C_REPLY messages.
    /// </summary>
    /// <param name="par_Sender">The object raising the event</param>
    /// <param name="par_EventArgs">Event arguments holding an <see cref="I2cReply"/></param>
    public delegate void I2cReplyReceivedHandler(object par_Sender, FirmataEventArgs<I2cReply> par_EventArgs);

    /// <summary>
    /// Defines a comprehensive set of members supporting the I2C Protocol.
    /// </summary>
    public interface II2cProtocol
    {
        /// <summary>
        /// Event, raised for every SYSEX I2C message not handled by an <see cref="II2cProtocol"/>'s Get method.
        /// </summary>
        /// <remarks>
        /// When i.e. method <see cref="I2cReadOnce"/> is invoked, the party system's response message raises this event.
        /// However, when method <see cref="GetI2cReply"/> or <see cref="GetI2cReplyAsync"/> is invoked, the response is returned
        /// to the respective methods and event <see cref="OnI2cReplyReceived"/> is not raised.
        /// </remarks>
        event I2cReplyReceivedHandler OnI2cReplyReceived;

        void I2cSetInterval(int microseconds);
        void I2cWrite(int slaveAddress, byte[] data);
        void I2cReadOnce(int slaveAddress, int bytesToRead);
        void I2cReadOnce(int slaveAddress, int slaveRegister, int bytesToRead);
        void I2cReadContinuous(int slaveAddress, int bytesToRead);
        void I2cReadContinuous(int slaveAddress, int slaveRegister, int bytesToRead);
        void I2cStopReading();
        I2cReply GetI2cReply(int slaveAddress, int bytesToRead);
        Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int bytesToRead);
        I2cReply GetI2cReply(int slaveAddress, int slaveRegister, int bytesToRead);
        Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int slaveRegister, int bytesToRead);
    }
}
