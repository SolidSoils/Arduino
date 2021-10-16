namespace Solid.Arduino
{
    /// <summary>
    /// Event arguments passed to a <see cref="StringReceivedHandler"/> type event.
    /// </summary>
    /// <see cref="StringReceivedHandler"/>
    /// <see cref="ArduinoSession.StringReceived"/>
    public class StringEventArgs
    {
        private readonly string _text;

        internal StringEventArgs(string text)
        {
            _text = text;
        }

        /// <summary>
        /// Gets the string value being received.
        /// </summary>
        public string Text => _text;
    }
}
