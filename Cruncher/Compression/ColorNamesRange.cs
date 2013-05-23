#region Licence
// -----------------------------------------------------------------------
// <copyright file="ColorNamesRange.cs" company="James South">
//     Copyright (c) James South.
//     Licensed under the Apache License, Version 2.0.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.Compression
{
    /// <summary>
    /// Represents the color range to be used by the Minifier instance.
    /// </summary>
    public enum ColorNamesRange
    {
        /// <summary>
        /// W3C-strict color names will be used if they are 
        /// shorter than the equivalent RGB values.
        /// </summary>
        W3CStrict = 0,

        /// <summary>
        /// No Color names will be used.
        /// </summary>
        HexadecimalOnly = 1,

        /// <summary>
        ///  A set of colors recognized by all major browser is okay to use 
        ///  (W3C-strict validation is not required)
        /// </summary>
        AllMajorColors = 2
    }
}
