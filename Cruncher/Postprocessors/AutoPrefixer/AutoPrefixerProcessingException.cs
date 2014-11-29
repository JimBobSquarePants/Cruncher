// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoPrefixerProcessingException.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The exception that is thrown when post processing by the <see cref="AutoPrefixerPostprocessor" />  has failed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Postprocessors.AutoPrefixer
{
    using System;

    /// <summary>
    /// The exception that is thrown when post processing by the <see cref="AutoPrefixerPostprocessor"/>  has failed.
    /// </summary>
    internal sealed class AutoPrefixerProcessingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPrefixerProcessingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AutoPrefixerProcessingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPrefixerProcessingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AutoPrefixerProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
