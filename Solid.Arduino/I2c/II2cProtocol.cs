using System;
using System.Threading.Tasks;
using Solid.Arduino.Firmata;

namespace Solid.Arduino.I2C
{
    /// <summary>
    /// Signature of event handlers capable of processing I2C_REPLY messages.
    /// </summary>
    /// <param name="sender">The object raising the event</param>
    /// <param name="eventArgs">Event arguments holding an <see cref="I2CReply"/></param>
    public delegate void I2CReplyReceivedHandler(object sender, I2CEventArgs eventArgs);

    /// <summary>
    /// Defines a comprehensive set of members supporting the I2C Protocol.
    /// </summary>
    /// <seealso href="http://www.i2c-bus.org/">I2C bus website by telos Systementwicklung GmbH</seealso>
    /// <seealso href="http://www.arduino.cc/en/Reference/Wire">Arduino Wire reference</seealso>
    /// <seealso href="http://playground.arduino.cc/Main/I2cScanner">I2C Scanner sample sketch for Arduino</seealso>
    public interface II2CProtocol
    {
        /// <summary>
        /// Event, raised for every SYSEX I2C message not handled by an <see cref="II2CProtocol"/>'s Get method.
        /// </summary>
        /// <remarks>
        /// When e.g. methods <see cref="ReadI2COnce(int,int)"/> and <see cref="ReadI2CContinuous(int,int)"/> are invoked,
        /// the party system's response messages raise this event.
        /// However, when method <see cref="GetI2CReply(int,int)"/> or <see cref="GetI2CReplyAsync(int,int)"/> is invoked,
        /// the response received is returned to the method that issued the command and event <see cref="I2CReplyReceived"/> is not raised.
        /// </remarks>
        event I2CReplyReceivedHandler I2CReplyReceived;

        /// <summary>
        /// Creates an observable object tracking <see cref="I2CReply"/> messages.
        /// </summary>
        /// <returns>An <see cref="IObservable{I2cReply}"/> interface</returns>
        IObservable<I2CReply> CreateI2CReplyMonitor();

        /// <summary>
        /// Sets the frequency at which data is read in the continuous mode.
        /// </summary>
        /// <param name="microseconds">The interval, expressed in microseconds</param>
        void SetI2CReadInterval(int microseconds);

        /// <summary>
        /// Writes an arbitrary array of bytes to the given memory address.
        /// </summary>
        /// <param name="slaveAddress">The slave's target address</param>
        /// <param name="data">The data array</param>
        void WriteI2C(int slaveAddress, params byte[] data);

        /// <summary>
        /// Requests the party system to send bytes read from the given memory address.
        /// </summary>
        /// <param name="slaveAddress">The slave's memory address</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <remarks>
        /// The party system is expected to return a single I2C_REPLY message.
        /// This message triggers the <see cref="I2CReplyReceived"/> event. The data
        /// are passed in the <see cref="FirmataEventArgs{T}"/> in an <see cref="I2CReply"/> object.
        /// </remarks>
        void ReadI2COnce(int slaveAddress, int bytesToRead);

        /// <summary>
        /// Requests the party system to send bytes read from the given memory address and register.
        /// </summary>
        /// <param name="slaveAddress">The slave's memory address</param>
        /// <param name="slaveRegister">The slave's register</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        void ReadI2COnce(int slaveAddress, int slaveRegister, int bytesToRead);

        /// <summary>
        /// Requests the party system to repeatedly send bytes read from the given memory address.
        /// </summary>
        /// <param name="slaveAddress">The slave's address</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <remarks>
        /// The party system is expected to return a continuous stream of I2C_REPLY messages at
        /// an interval which can be set using the <see cref="SetI2CReadInterval"/> method.
        /// Received I2C_REPLY messages trigger the <see cref="I2CReplyReceived"/> event. The data
        /// are served in the <see cref="I2CEventArgs"/>'s Value property as an <see cref="I2CReply"/> object.
        /// <para>
        /// The party system can be stopped sending I2C_REPLY messages by issuing a <see cref="StopI2CReading"/> command.
        /// </para>
        /// </remarks>
        void ReadI2CContinuous(int slaveAddress, int bytesToRead);

        /// <summary>
        /// Requests the party system to repeatedly send bytes read from the given memory address and register.
        /// </summary>
        /// <param name="slaveAddress">The slave's memory address</param>
        /// <param name="slaveRegister">The slave's register</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        void ReadI2CContinuous(int slaveAddress, int slaveRegister, int bytesToRead);

        /// <summary>
        /// Commands the party system to stop sending I2C_REPLY messages.
        /// </summary>
        void StopI2CReading();

        /// <summary>
        /// Gets byte data from the party system, read from the given memory address.
        /// </summary>
        /// <param name="slaveAddress">The slave's memory address</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <returns>An <see cref="I2CReply"/> object holding the data read</returns>
        I2CReply GetI2CReply(int slaveAddress, int bytesToRead);

        /// <summary>
        /// Asynchronously gets byte data from the party system, read from the given memory address.
        /// </summary>
        /// <param name="slaveAddress">The slave's memory address</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <returns>An awaitable <see cref="Task{I2cReply}"/> holding the data read</returns>
        Task<I2CReply> GetI2CReplyAsync(int slaveAddress, int bytesToRead);

        /// <summary>
        /// Gets byte data from the party system, read from the given memory address and register.
        /// </summary>
        /// <param name="slaveAddress">The slave's memory address and register</param>
        /// <param name="slaveRegister">The slave's register</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <returns>An <see cref="I2CReply"/> object holding the data read</returns>
        I2CReply GetI2CReply(int slaveAddress, int slaveRegister, int bytesToRead);

        /// <summary>
        /// Asynchronously gets byte data from the party system, read from the given memory address and register.
        /// </summary>
        /// <param name="slaveAddress">The slave's memory address</param>
        /// <param name="slaveRegister">The slave's register</param>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <returns>An awaitable <see cref="Task{I2cReply}"/> holding the data read</returns>
        Task<I2CReply> GetI2CReplyAsync(int slaveAddress, int slaveRegister, int bytesToRead);
    }
}
