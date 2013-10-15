// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates a series of time saving extension methods to <see cref="T:System.String">String</see>s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Extensions
{
    #region Using
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
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

        #region Cryptography
        /// <summary>
        /// Creates an MD5 fingerprint of the String.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>An MD5 fingerprint of the String.</returns>
        public static string ToMd5Fingerprint(this string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return string.Empty;
            }

            byte[] bytes = Encoding.Unicode.GetBytes(expression.ToCharArray());

            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] hash = md5.ComputeHash(bytes);

                // Concatenate the hash bytes into one long String.
                return hash.Aggregate(
                    new StringBuilder(32),
                    (sb, b) => sb.Append(b.ToString("X2", CultureInfo.InvariantCulture)))
                    .ToString().ToLowerInvariant();
            }
        }
        #endregion

        #region Files and Paths
        /// <summary>
        /// Checks the string to see whether the value is a valid virtual path name.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>True if the given string is a valid virtual path name</returns>
        public static bool IsValidVirtualPathName(this string expression)
        {
            Uri uri;

            return Uri.TryCreate(expression, UriKind.Relative, out uri) && uri.IsWellFormedOriginalString();
        }
        #endregion
    }
}
