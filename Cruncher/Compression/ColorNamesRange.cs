#region Licence
// -----------------------------------------------------------------------
// <copyright file="ColorNamesRange.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.Compression
{
    /// <summary>
    /// Represents the colour range to be used by the Minifier instance.
    /// </summary>
    public enum ColorNamesRange
    {
        /// <summary>
        /// W3C-strict colour names will be used if they are 
        /// shorter than the equivalent RGB values.
        /// </summary>
        W3CStrict = 0,

        /// <summary>
        /// No Colour names will be used.
        /// </summary>
        HexadecimalOnly = 1,

        /// <summary>
        ///  A set of colours recognized by all major browser is okay to use 
        ///  (W3C-strict validation is not required)
        /// </summary>
        AllMajorColors = 2
    }
}
