using System;
using System.IO;
using ImgDiff.Exceptions;
using ImgDiff.Monads;

namespace ImgDiff.Models
{
    public class ComparisonRequest
    {
        public Option<string> DirectoryPath { get; }
        public Option<string> FirstImagePath { get; }
        public Option<string> SecondImagePath { get; }
        public ComparisonWith With { get; }

        public ComparisonRequest(
            Option<string> directoryPath,
            Option<string> firstImagePath,
            Option<string> secondImagePath,
            ComparisonWith with)
        {
            DirectoryPath = directoryPath;
            FirstImagePath = firstImagePath;
            SecondImagePath = secondImagePath;
            With = with;
        }

        /// <summary>
        /// Performs validation on this <see cref="ComparisonRequest"/> object. Returns `true`
        /// if the object is valid for the request type. Otherwise, throws an exception. 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidDirectoryComparisonRequestedException">
        ///     Thrown if there are invalid values to request a directory comparison.</exception>
        /// <exception cref="InvalidPairComparisonRequestedException">
        ///     Thrown if there are invalid values to request a pair comparison.</exception>
        /// <exception cref="InvalidSingleComparisonRequestedException">
        ///     Thrown if there are invalid values to request a single comparison.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if we attempt to validate a <see cref="ComparisonWith"/> that either doesn't
        ///     exist, or is not supported.</exception>
        public ComparisonRequest Validate()
        {
            switch (With)
            {
                case ComparisonWith.All:
                    ValidateDirectory();
                    break;

                case ComparisonWith.Pair :
                    ValidatePair();
                    break;
                
                case ComparisonWith.Single :
                    ValidateSingle();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(
                        $"The comparison of type {With} is either not know or not supported at this time.");
            }
            
            return this;
        }

        void ValidateDirectory()
        {
            switch (DirectoryPath)
            {
                case None<string> none:
                    throw new InvalidDirectoryComparisonRequestedException(
                        "Cannot request a directory comparison without a directory.");
                case Some<string> some:
                    if (!Directory.Exists(DirectoryPath.Value))
                        throw new InvalidDirectoryComparisonRequestedException(
                            "The given directory does not appear to exist on the disk.",
                            new DirectoryNotFoundException(DirectoryPath.Value));
                    break;
            }
        }

        void ValidatePair()
        {
            if (FirstImagePath.IsNone
            ||  SecondImagePath.IsNone)
                throw new InvalidPairComparisonRequestedException(
                    "Cannot request a pair comparison without 2 images.");
            
            if (!File.Exists(FirstImagePath.Value))
                throw new InvalidPairComparisonRequestedException(
                    "The requested image does not appear to exist on the disk.",
                    new FileNotFoundException(FirstImagePath.Value));
                        
            if (!File.Exists(SecondImagePath.Value))
                throw new InvalidPairComparisonRequestedException(
                    "The requested image does not appear to exist on the disk.",
                    new FileNotFoundException(SecondImagePath.Value));
        }

        void ValidateSingle()
        {
            if (FirstImagePath.IsNone
            ||  DirectoryPath.IsNone)
                throw new InvalidSingleComparisonRequestedException(
                    "Cannot perform a single comparison without an image to compare, or a directory to check against.");
            
            if (!File.Exists(FirstImagePath.Value))
                throw new InvalidSingleComparisonRequestedException(
                    "The given file does not appear to exist on the disk.",
                    new FileNotFoundException(FirstImagePath.Value));
                    
            if (!Directory.Exists(DirectoryPath.Value))
                throw new InvalidSingleComparisonRequestedException(
                    "Either the directory or the file supplied to not appear to exist on the disk.",
                    new DirectoryNotFoundException(DirectoryPath.Value));
        }
    }
}
