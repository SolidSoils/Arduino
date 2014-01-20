using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino
{
    public sealed class StringEventArgs
    {
        private readonly string _text;

        internal StringEventArgs(string text)
        {
            _text = text;
        }

        public string Text { get { return _text; } }
    }
}
