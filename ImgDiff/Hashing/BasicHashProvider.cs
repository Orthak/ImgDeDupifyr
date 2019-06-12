using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Extensions;
using ImgDiff.Interfaces;

namespace ImgDiff.Hashing
{
    public class BasicHashProvider : IHashProvider
    {
        /// <summary>
        /// As basic as it gets. Reverse the bytes, then get the
        /// base64 string from the reversed array. Ideally, there
        /// would be a much better implementation here.
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <returns></returns>
        public Task<string> CreateHash(byte[] inputBytes) =>
            Task.Run(() =>
            {
                var reversedBinary = 
                    new BitArray(inputBytes
                        .Reverse()
                        .ToArray());
                var hash = reversedBinary.GetBytes();

                return Convert.ToBase64String(hash);
            });
    }
}
