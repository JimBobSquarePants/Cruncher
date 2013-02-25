#region Licence
// -----------------------------------------------------------------------
// <copyright file="IPreProcessor.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.PreProcessors
{
    /// <summary>
    /// Defines methods to preprocess the file before compression.
    /// </summary>
    public interface IPreProcessor
    {
        /// <summary>
        /// Transforms the content of the given string. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the given input string to transform.</param>
        /// <returns>The transformed string.</returns>
        string Transform(string input, string path);
    }
}
