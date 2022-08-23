using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFGames_Util_4._0.Math
{
    static class MathConverter
    {
        public static Int32 MathToInt32(byte[] bytes, Int32 offset, bool isLittleEndian)
        {
            Int32 num = 0;
            int exponent = 0;

            if (!isLittleEndian)
            {
                List<byte> tmp = bytes.ToList();
                tmp.Reverse();
                bytes = tmp.ToArray();
            }

            for (int i = offset; i < offset + 4; i++)
            {
                num += Convert.ToInt32((Int32)Convert.ToInt32(bytes[i]) * (Int32)System.Math.Pow(256, exponent));
                exponent++;
            }

            return num;
        }

        public static byte[] MathToBytes(Int32 num, bool isLittleEndian)
        {
            byte[] bytes = new byte[4];
            bool isNegative = false;

            if (num < 0)
            {
                num *= -1;
                isNegative = true;
                num -= 1;
            }

            for (int i = 0; i < 4; i++)
            {
                bytes[i] = Convert.ToByte(num % 256);

                if (isNegative)
                {
                    bytes[i] = (byte)~bytes[i];
                }

                num = num / 256;
            }

            /*if (isNegative)
            {
                bytes[0] += 1;
            }*/

            if (!isLittleEndian)
            {
                List<byte> tmp = bytes.ToList();
                tmp.Reverse();
                bytes = tmp.ToArray();
            }

            return bytes;
        }

        public static Int32 Int32Converter(byte[] bytes, bool isLittleEndian, ref bool is16BitUnicodeBlock, ref bool was16BitUnicodeBlockAtLeastOnceBefore)
        {
            if (MathToInt32(bytes, 0, (isLittleEndian ? true : false)) < 0)
            {
                is16BitUnicodeBlock = true;
                was16BitUnicodeBlockAtLeastOnceBefore = true;
                return MathToInt32(bytes, 0, (isLittleEndian ? true : false)) * -2;
            }
            else
            {
                return MathToInt32(bytes, 0, (isLittleEndian ? true : false));
            }
        }
    }
}
