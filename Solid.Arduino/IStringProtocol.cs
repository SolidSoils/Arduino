using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Solid.Arduino
{
    /// <summary>
    /// Signature of event handlers capable of processing received strings.
    /// </summary>
    /// <param name="sender">The object raising the event</param>
    /// <param name="eventArgs">Event arguments holding a <see cref="string"/> message</param>
    public delegate void StringReceivedHandler(object sender, StringEventArgs eventArgs);

    /// <summary>
    /// Defines members for sending and receiving ASCII string messages.
    /// </summary>
    public interface IStringProtocol
    {
        /// <summary>
        /// Event, raised for every ASCII stringmessage not handled by an <see cref="IStringProtocol"/>'s
        /// Read, ReadAsync, ReadLine, ReadLineAsync, ReadTo or ReadToAsync method
        /// </summary>
        /// <remarks>
        /// Any spontaneous received string message, terminated with a newline or eof character raises this event.
        /// </remarks>
        event StringReceivedHandler StringReceived;

        /// <summary>
        /// Creates an observable object tracking received ASCII <see cref="string"/> messages.
        /// </summary>
        /// <returns>An <see cref="IObservable{String}"/> interface</returns>
        IObservable<string> CreateReceivedStringMonitor();

        /// <summary>
        /// Gets or sets the value used to interpret the end of strings received and sent.
        /// </summary>
        string NewLine { get; set; }

        /// <summary>
        /// Writes a string to the serial output data stream.
        /// </summary>
        /// <param name="value">A string to be written</param>
        void Write(string value = null);

        /// <summary>
        /// Writes the specified string and the <see cref="SerialPort.NewLine"/> value to the serial output stream.
        /// </summary>
        /// <param name="value">The string to write</param>
        void WriteLine(string value = null);

        /// <summary>
        /// Reads a string up to the next <see cref="NewLine"/> character.
        /// </summary>
        /// <returns>The string read</returns>
        string ReadLine();

        /// <summary>
        /// Reads a string asynchronous up to the next <see cref="NewLine"/> character.
        /// </summary>
        /// <returns>An awaitable <see cref="Task{String}"/> returning the string read</returns>
        Task<string> ReadLineAsync();

        /// <summary>
        /// Reads a specified number of characters.
        /// </summary>
        /// <param name="length">The number of characters to be read (default is 1)</param>
        /// <returns>The string read</returns>
        string Read(int length = 1);

        /// <summary>
        /// Reads a specified number of characters asynchronous.
        /// </summary>
        /// <param name="length">The number of characters to be read (default is 1)</param>
        /// <returns>An awaitable <see cref="Task{String}"/> returning the string read</returns>
        Task<string> ReadAsync(int length = 1);

        /// <summary>
        /// Reads a string up to the first terminating character.
        /// </summary>
        /// <param name="terminator">The character identifying the end of the string</param>
        /// <returns>The string read</returns>
        string ReadTo(char terminator);

        /// <summary>
        /// Reads a string asynchronous up to the first terminating character.
        /// </summary>
        /// <param name="terminator">The character identifying the end of the string</param>
        /// <returns>An awaitable <see cref="Task{String}"/> returning the string read</returns>
        Task<string> ReadToAsync(char terminator);
    }
}
