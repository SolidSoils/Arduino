using System;
using System.Linq;

namespace Solid.Arduino
{
    /// <summary>
    /// Provides extension methods for <see cref="string"/> objects.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the argument string into its binary-coded decimal (BCD) representation, e.g.
        ///  "1234" -> { 0x12, 0x34 } (for Big Endian byte order)
        ///  "1234" -> { 0x43, 0x21 } (for Little Endian byte order)
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
                return Array.Empty<byte>();

            if (o.Length % 2 == 1)
                o = "0" + o;

            char[] chars = o.ToCharArray();

            if (!chars.All(char.IsDigit))
                throw new ArgumentException(Messages.ArgumentEx_DigitStringOnly);

            byte[] bytes = new byte[o.Length >> 1];

            if (isLittleEndian)
            {
                int byteIndex = bytes.Length - 1;

                for (int x = 0; x < o.Length; x += 2)
                {
                    bytes[byteIndex--] = (byte)(((Convert.ToInt32(chars[x + 1]) - 48) << 4) | (Convert.ToInt32(chars[x]) - 48));
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

        /// <summary>
        /// Converts a <see cref="string"/> to a 14 bit bigendian <see cref="byte"/> array.
        /// </summary>
        /// <param name="o">The string being converted</param>
        /// <returns>A <see cref="byte"/> array.</returns>
        /// <remarks>
        /// Every character in the string is converted into two 7-bit bytes, starting with the most significant byte.
        /// </remarks>
        public static byte[] To14BitIso(this string o)
        {
            if (o == null)
                throw new ArgumentNullException();

            if (o.Length == 0)
                return Array.Empty<byte>();

            byte[] dataBytes = new byte[o.Length * 2];

            for (int x = 0; x < o.Length; x++)
            {
                short c = Convert.ToInt16(o[x]);
                dataBytes[x * 2] = (byte)(c & 0x7F);
                dataBytes[x * 2 + 1] = (byte)((c >> 7) & 0x7F);
            }
            return dataBytes;
        }
    }
}