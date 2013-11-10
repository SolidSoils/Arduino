using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public static class ByteArrayExtensions
    {
        public static string ConvertBinaryCodedDecimalToString(this byte[] data, bool isLittleEndian = false)
        {
            if (data == null)
                throw new ArgumentNullException();

            if (data.Length == 0)
                return string.Empty;

            char[] chars = new char[data.Length * 2];
            int charIndex = 0;

            for (int x = 0; x < data.Length; x++)
            {
                int mostSignificant = data[x] & (isLittleEndian ? 0x0F : 0xF0);
                int leastSignificant = data[x] & (isLittleEndian ? 0xF0 : 0x0F);

                if (mostSignificant > 9 | leastSignificant > 9)
                    throw new ArgumentOutOfRangeException("Cannot convert non-BCD data.");

                chars[charIndex++] = Convert.ToChar(mostSignificant);
                chars[charIndex++] = Convert.ToChar(leastSignificant);
            }

            return chars.ToString();
        }
    }
}
