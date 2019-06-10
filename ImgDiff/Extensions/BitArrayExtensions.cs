using System.Collections;
using System.Text;

namespace ImgDiff.Extensions
{
    public static class BitArrayExtensions
    {
        public static BitArray Append(this BitArray current, bool toAppend)
        {
            var bools = new bool[current.Count + 1];
            current.CopyTo(bools, 0);
            bools[bools.Length - 1] = toAppend;

            return new BitArray(bools);
        }

        public static byte[] GetBytes(this BitArray bitArray)
        {
            var targetLength = bitArray.Length / 8;
            var bytes = new byte[targetLength];
            for (var index = 0; index < targetLength; index++)
            {
                for (int b = index * 8, m = 1; b < index * 8 + 8; b++, m *= 2) 
                {
                    bytes[index] += bitArray.Get(b)
                        ? (byte)m
                        : (byte)0;
                }
            }

            return bytes;
        }
        
        public static string GetString(this BitArray bitArray) 
        {
            var encoding = new ASCIIEncoding();
            var bytes = GetBytes(bitArray);
            
            return encoding.GetString(bytes);
        }
    }
}
