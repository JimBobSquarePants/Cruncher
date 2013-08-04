// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleConfiguration.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The console configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CruncherConsole
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    /// <summary>
    /// The console configuration.
    /// </summary>
    public class ConsoleConfiguration
    {
        /// <summary>
        /// Gets or sets the target type to crunch.
        /// </summary>
        public CrunchTargetType TargetType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to minify the file.
        /// </summary>
        public bool Minify { get; set; }

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        public string InputPath { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        public string OutputPath { get; set; }
    }
}
