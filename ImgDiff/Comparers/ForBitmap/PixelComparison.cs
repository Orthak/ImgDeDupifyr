using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ImgDiff.Extensions;
using ImgDiff.Interfaces;
using ImgDiff.Models;

namespace ImgDiff.Comparers.ForBitmap
{
    public class PixelComparison : ICalculateDifference<Bitmap>
    {
        ComparisonOptions comparisonOptions;
        
        public PixelComparison(ComparisonOptions options)
        {
            comparisonOptions = options;
        }
        
        public async Task<double> CalculatePercentage(Bitmap source, Bitmap target)
        {
            var sourceRgbTask = PullRgbValues(source);
            var targetRgbTask = PullRgbValues(target);

            await Task.WhenAll(sourceRgbTask, targetRgbTask);
            var differenceCount = await BatchDifferenceCount(sourceRgbTask.Result, targetRgbTask.Result);

            return (1.0 - (differenceCount / (double) Math.Max(sourceRgbTask.Result.Length, targetRgbTask.Result.Length)));
        }

        Task<byte[]> PullRgbValues(Bitmap bmp) =>
            Task.Run(() =>
            {
                // Lock source bits.
                var sourceRectangle = new Rectangle(
                    0, 0, 
                    bmp.Width, bmp.Height);
                BitmapData bmpData = bmp.LockBits(
                    sourceRectangle,
                    ImageLockMode.ReadOnly,
                    bmp.PixelFormat);

                // Get address of the first line.
                IntPtr intPtr = bmpData.Scan0;

                // Declare array to hold the bytes.
                var bmpBytes = Math.Abs(bmpData.Stride) * bmp.Height;
                var rgbValues = new byte[bmpBytes];

                // Copy the RGB values into the array.
                Marshal.Copy(intPtr, rgbValues, 0, bmpBytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);

                return rgbValues;
            });
        
        async Task<int> BatchDifferenceCount(IReadOnlyList<byte> sourceRgb, IReadOnlyList<byte> targetRgb)
        {
            var sourceLength = sourceRgb.Count;
            var targetLength = targetRgb.Count;

            var differenceCount = 0;
            if (sourceLength != targetLength)
                differenceCount += Math.Abs(sourceLength - targetLength);
            
            var batchedTasks = new List<Task<int>>();
            var maxLength = Math.Max(sourceLength, targetLength);
            var batchSize = maxLength / 128;
            var batchCount = 0;
            while (batchCount * batchSize < maxLength)
            {
                var iterationCount = batchCount * batchSize;
                var sourceBatch = sourceRgb
                    .Skip(iterationCount)
                    .Take(batchSize)
                    .ToArray();

                var targetBatch = targetRgb
                    .Skip(iterationCount)
                    .Take(batchSize)
                    .ToArray();
                
                var minLength = Math.Min(sourceBatch.Length, targetBatch.Length);
                batchedTasks.Add(CountDifferences(minLength, sourceBatch, targetBatch));

                batchCount++;
            }

            var finishedBatches = await Task.WhenAll(batchedTasks);
            var batchedCount = finishedBatches.Aggregate((total, next) => total + next);
            var totalDifferences = differenceCount + batchedCount;

            return totalDifferences;
        }

        Task<int> CountDifferences(int minLength, IReadOnlyList<byte> source, IReadOnlyList<byte> target) =>
            Task.Run(() =>
            {
                var count = 0;
                for (var index = 0; index < minLength; index++)
                    if (!target[index].InRange(source[index], (int)comparisonOptions.ColorStrictness))
                        count++;

                return count;
            });
    }
}
