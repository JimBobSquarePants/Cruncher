// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPreprocessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Defines methods to pre-process the file before compression.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors
{
    /// <summary>
    /// Defines methods to pre-process the file before compression.
    /// </summary>
    public interface IPreprocessor
    {
        /// <summary>
        /// Gets the extensions that this filter processes.
        /// </summary>
        string[] AllowedExtensions { get; }

        /// <summary>
        /// Transforms the content of the given string. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the given input string to transform.</param>
        /// <param name="cruncher">The cruncher that is running the transform.</param>
        /// <returns>The transformed string.</returns>
        string Transform(string input, string path, CruncherBase cruncher);
    }
}
