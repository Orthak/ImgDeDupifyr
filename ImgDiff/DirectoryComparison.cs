using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ImgDiff
{
    public class DirectoryComparison : ICompareImages
    {
        readonly List<DuplicateResult> duplicateResults = new List<DuplicateResult>();
            
        public async Task<List<DuplicateResult>> Run(string request)
        {
            var directoryString = request;
            if (Directory.Exists(directoryString) == false)
                throw new DirectoryNotFoundException(
                    $"Directory '{directoryString}' was not found on the disk.");

            var files = Directory.GetFiles(directoryString);
            var images = new List<LocalImage>();
            for (int index = 0; index < files.Count(); index++)
            {
                var ext = await GetExtension(files[index]);
                if (!Extensions.ValidForImage.Contains(ext))
                    continue;

                var img = await BuildLocalImage(files[index]);
                images.Add(img);
            }

            Console.WriteLine($"Checking {images.Count()} files in '{directoryString}'...");
            await CheckForDuplicates(images);

            return duplicateResults;
        }

        async Task CheckForDuplicates(List<LocalImage> inDirectory)
        {
            var visited = new List<string>();
            foreach (var localImage in inDirectory)
            {
                if (visited.Contains(localImage.Name))
                    continue;
                
                var duplicateResult = new DuplicateResult(localImage);
                await VisitOthers(inDirectory, localImage, duplicateResult, visited);

                duplicateResults.Add(duplicateResult);
                visited.Add(localImage.Name);
            }

            visited.Clear();
        }

        static Task VisitOthers(
            List<LocalImage> inDirectory,
            LocalImage localImage,
            DuplicateResult duplicateResult,
            List<string> visited)
        {
            return Task.Run(() =>
            {
                foreach (var toCheck in inDirectory)
                {
                    if (toCheck.Name == localImage.Name)
                        continue;

                    if (localImage.Hash == toCheck.Hash)
                    {
                        duplicateResult.Duplicates.Add(toCheck);
                        visited.Add(toCheck.Name);
                    }
                }
            });
        }

        /// <summary>
        /// Instantiate a new custom class, to hold some useful data about
        /// each image that we want to look at. This helps reduce complex
        /// computation later, when we loop over everything.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        async Task<LocalImage> BuildLocalImage(string filePath)
        {
            var ext = await GetExtension(filePath);
            var hash = await GetFileHash(filePath);

            return new LocalImage(
                filePath,
                ext,
                hash);
        }
        
        /// <summary>
        /// Returns the extension of the given file path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task<string> GetExtension(string filePath)
        {
            return Task.Run(() => 
                Path.GetExtension(filePath).ToLowerInvariant());
        }
        
        Task<byte[]> ToByteArray(Bitmap bitMap)
        {
            return Task.Run(() =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    bitMap.Save(memoryStream, ImageFormat.Bmp);
                    return memoryStream.ToArray();
                }
            });
        }

        Task<string> GetHash(byte[] imageBytes)
        {
            return Task.Run(() =>
            {
                using (var sha = new SHA1CryptoServiceProvider())
                {
                    return Convert.ToBase64String(sha.ComputeHash(imageBytes));
                }
            });
        }

        Task<string> GetFileHash(string filePath)
        {
            return Task.Run(async () =>
                await GetHash(
                    await ToByteArray(
                        new Bitmap(filePath))));
        }
    }
}