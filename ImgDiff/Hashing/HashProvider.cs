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
        public Task<string> Generate(byte[] inputBytes) =>
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