using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert the argument string into its binary-coded decimal (BCD) representation, e.g.
        ///  "0110" -> { 0x01, 0x10 } (for Big Endian byte order)
        ///  "0110" -> { 0x10, 0x01 } (for Little Endian byte order)
        /// </summary>
        /// <param name="isLittleEndian">True if the byte order is "little end first (leftmost)".</param>
        /// <param name="o">String representation of BCD bytes.</param>
        /// <returns>Byte array representation of the string as BCD.</returns>
        /// <exception cref="ArgumentException">Thrown if the argument string isn't entirely made up of BCD pairs.</exception>

        public static byte[] ToBinaryCodedDecimal(this string o, bool isLittleEndian = false)
        {
            if (o == null)
                throw new ArgumentNullException();

            if (o.Length == 0)
                return new byte[0];

            if (o.Length % 2 == 1)
                o = "0" + o;

            char[] chars = o.ToCharArray();

            if (!chars.All(c => Char.IsDigit(c)))
                throw new ArgumentException("String must contain digits only.");

            byte[] bytes = new byte[o.Length >> 1];

            if (isLittleEndian)
            {
                int byteIndex = bytes.Length;

                for (int x = 0; x < o.Length; x += 2)
                {
                    bytes[--byteIndex] = (byte)(((Convert.ToInt32(chars[x - 1]) - 48) << 4) | (Convert.ToInt32(chars[x]) - 48));
                }
            }
            else
            {
                int byteIndex = 0;

                for (int x = 0; x < o.Length; x += 2)
                {
                    bytes[byteIndex++] = (byte)(((Convert.ToInt32(chars[x]) - 48) << 4) | (Convert.ToInt32(chars[x + 1]) - 48));
                }
            }
            return bytes;
        }
    }
}