using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.I2c
{
    /// <summary>
    /// Signature of event handlers capable of processing I2C_REPLY messages.
    /// </summary>
    /// <param name="par_Sender">The object raising the event</param>
    /// <param name="par_EventArgs">Event arguments holding an <see cref="I2cReply"/></param>
    public delegate void I2cReplyReceivedHandler(object par_Sender, I2cEventArgs par_EventArgs);

    /// <summary>
    /// Defines a comprehensive set of members supporting the I2C Protocol.
    /// </summary>
    public interface II2cProtocol
    {
        /// <summary>
        /// Event, raised for every SYSEX I2C message not handled by an <see cref="II2cProtocol"/>'s Get method.
        /// </summary>
        /// <remarks>
        /// When i.e. method <see cref="ReadI2cOnce"/> is invoked, the party system's response message raises this event.
        /// However, when method <see cref="GetI2cReply"/> or <see cref="GetI2cReplyAsync"/> is invoked, the response is returned
        /// to the respective methods and event <see cref="I2cReplyReceived"/> is not raised.
        /// </remarks>
        event I2cReplyReceivedHandler I2cReplyReceived;

        /// <summary>
        /// Sets the frequency at which data is read in the continuous mode.
        /// </summary>
        /// <param name="microseconds">The interval, expressed in microseconds</param>
        void SetI2cReadInterval(int microseconds);

        /// <summary>
        /// Writes an arbitrary array of bytes to the given memory address.
        /// </summary>
        /// <param name="slaveAddress">The target address</param>
        /// <param name="data">The data array</param>
        void WriteI2c(int slaveAddress, byte[] data);

        /// <summary>
        /// Requests the party system to send bytes read from the given memory address.
        /// </summary>
        /// <param name="slaveAddress">The source address</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <remarks>
        /// The party system is expected to return a single I2C_REPLY message.
        /// This message triggers the <see cref="I2cReplyReceived"/> event. The capabilities
        /// are passed in the <see cref="FirmataEventArgs<I2cReply>"/> in an <see cref="I2cReply"/> object.
        /// </remarks>
        void ReadI2cOnce(int slaveAddress, int bytesToRead);
        void ReadI2cOnce(int slaveAddress, int slaveRegister, int bytesToRead);
        void ReadI2cContinuous(int slaveAddress, int bytesToRead);
        void ReadI2cContinuous(int slaveAddress, int slaveRegister, int bytesToRead);
        void StopI2cReading();
        I2cReply GetI2cReply(int slaveAddress, int bytesToRead);
        Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int bytesToRead);
        I2cReply GetI2cReply(int slaveAddress, int slaveRegister, int bytesToRead);
        Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int slaveRegister, int bytesToRead);
    }
}
