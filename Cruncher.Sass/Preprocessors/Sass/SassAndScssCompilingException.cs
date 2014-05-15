// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SassAndScssCompilingException.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The exception that is thrown when a compiling of Sass- and SCSS-code is failed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Sass
{
    using System;

    /// <summary>
    /// The exception that is thrown when a compiling of Sass- and SCSS-code is failed.
    /// </summary>
    internal sealed class SassAndScssCompilingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SassAndScssCompilingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SassAndScssCompilingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SassAndScssCompilingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SassAndScssCompilingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
