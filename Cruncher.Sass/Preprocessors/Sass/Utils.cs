// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Utility methods for the sass pre-processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Sass
{
    #region Using
    using System.IO;
    using System.Reflection;
    using System.Text; 
    #endregion

    /// <summary>
    /// Utility methods for the sass pre-processor.
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Converts an assembly resource into a string.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <param name="encoding">
        /// The character encoding to return the resource in.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ResourceAsString(string resource, Assembly assembly = null, Encoding encoding = null)
        {
            assembly = assembly ?? Assembly.GetExecutingAssembly();
            encoding = encoding ?? Encoding.UTF8;

            using (var ms = new MemoryStream())
            {
                using (Stream manifestResourceStream = assembly.GetManifestResourceStream(resource))
                {
                    if (manifestResourceStream != null)
                    {
                        manifestResourceStream.CopyTo(ms);
                    }
                }

                return encoding.GetString(ms.GetBuffer()).Replace('\0', ' ').Trim();
            }
        }
    }
}
