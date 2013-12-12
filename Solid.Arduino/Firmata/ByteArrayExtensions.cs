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

            if (isLittleEndian)
            {
                for (int x = data.Length - 1; x >= 0; x--)
                {
                    chars[charIndex++] = ConvertToChar(data[x] & 0x0F);
                    chars[charIndex++] = ConvertToChar(data[x] >> 4);
                }
            }
            else
            {
                for (int x = 0; x < data.Length; x++)
                {
                    chars[charIndex++] = ConvertToChar(data[x] >> 4);
                    chars[charIndex++] = ConvertToChar(data[x] & 0x0F);
                }
            }

            return new string(chars);
        }

        private static char ConvertToChar(int code)
        {
            if (code > 9)
                throw new ArgumentException(Messages.ArgumentEx_CannotConvertBcd);

            return Convert.ToChar(code | 0x30);
        }
    }
}
