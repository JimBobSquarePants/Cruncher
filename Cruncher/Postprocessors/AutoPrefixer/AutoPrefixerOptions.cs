// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoPrefixerOptions.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   AutoPrefixer options
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Postprocessors.AutoPrefixer
{
    using System.Collections.Generic;

    /// <summary>
    /// AutoPrefixer options
    /// </summary>
    public sealed class AutoPrefixerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPrefixerOptions"/> class.
        /// </summary>
        public AutoPrefixerOptions()
        {
            this.Browsers = new List<string>();
            this.Cascade = true;
            this.Safe = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the AutoPrefixer is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a list of browser conditional expressions.
        /// </summary>
        public IList<string> Browsers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to create nice visual cascade of prefixes.
        /// </summary>
        public bool Cascade
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to enable the special safe mode to parse broken CSS.
        /// </summary>
        public bool Safe
        {
            get;
            set;
        }
    }
}
