#region Licence
// -----------------------------------------------------------------------
// <copyright file="ResponseType.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.Helpers
{
    #region Using
    using System.ComponentModel;
    #endregion

    /// <summary>
    /// Globally available enumeration which specifies the correct HTTP MIME type of
    /// the output stream for different response types.
    /// <para>
    /// <see cref="http://en.wikipedia.org/wiki/Internet_media_type"/>
    /// </para>
    /// </summary>
    public enum ResponseType
    {
        /// <summary>
        /// The correct HTTP MIME type of the output stream for Css.
        /// </summary>
        [Description("text/css")]
        Css,

        /// <summary>
        /// The correct HTTP MIME type of the output stream for JavaScript.
        /// <remarks>
        /// <para>
        ///  Defined in and obsoleted by RFC 4329 in order to discourage its usage in favour of 
        ///  application/javascript. However, text/javascript is allowed in HTML 4 and 5 and,
        ///  unlike application/javascript, has cross-browser support
        /// </para>
        /// </remarks>
        /// </summary>
        [Description("text/javascript")]
        JavaScript
    }
}
