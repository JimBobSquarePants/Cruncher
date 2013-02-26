#region Licence
// -----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.Extensions
{
    #region Using

    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// Encapsulates a series of time saving extension methods to <see cref="T:System.String">String</see>s.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Specifies whether a string contains another string dependent on the given comparison enumeration. 
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <param name="value">The string value to search for.</param>
        /// <param name="comparisonType">The string comparer to determine comparison rules.</param>
        /// <returns><see langword="true"/> if the targeted string contains the given string; otherwise <see langword="false"/></returns>
        public static bool Contains(this string expression, string value, StringComparison comparisonType)
        {
            return expression.IndexOf(value, comparisonType) >= 0;
        }

        #region Files and Paths
        /// <summary>
        /// Checks the string to see whether the value is a valid virtual path name.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>True if the given string is a valid virtual path name</returns>
        public static bool IsValidVirtualPathName(this string expression)
        {
            // Check the start of the string.
            if (expression.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                // Trim the first two characters and test the path.
                expression = expression.Substring(2);
                return expression.IsValidPathName();
            }

            return false;
        }

        /// <summary>
        /// Checks the string to see whether the value is a valid path name.
        /// <see cref="http://stackoverflow.com/questions/62771/how-check-if-given-string-is-legal-allowed-file-name-under-windows/"/>
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>True if the given string is a valid path name</returns>
        public static bool IsValidPathName(this string expression)
        {
            // Create a regex of invalid characters and test it.
            string invalidPathNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regFixPathName = new Regex("[" + Regex.Escape(invalidPathNameChars) + "]");

            return !regFixPathName.IsMatch(expression);
        }

        #endregion
    }
}
