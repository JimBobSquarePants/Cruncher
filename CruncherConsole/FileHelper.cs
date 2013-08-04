// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileHelper.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   FileHelper class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CruncherConsole
{
    #region Using
    using System;
    using System.IO; 
    #endregion

    /// <summary>FileHelper class.</summary>
    internal static class FileHelper
    {
        /// <summary>
        /// Writes the file to hard drive.
        /// </summary>
        /// <param name="path">Path of file</param>
        /// <param name="content">The contents of file</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either path or content are null.
        /// </exception>
        internal static void WriteFile(string path, string content)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content);
        }
    }
}