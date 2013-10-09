// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SassPreprocessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides methods to convert SASS and SCSS into CSS.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Sass
{
    /// <summary>
    /// Provides methods to convert SASS and SCSS into CSS.
    /// </summary>
    public class SassPreprocessor : IPreprocessor
    {
        #region Fields
        /// <summary>
        /// Gets the extensions that this filter processes.
        /// </summary>
        public string[] AllowedExtensions
        {
            get { return new[] { ".SASS", ".SCSS" }; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Transforms the content of the given string.
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the given input string to transform.</param>
        /// <returns>
        /// The transformed string.
        /// </returns>
        public string Transform(string input, string path)
        {
            SassCompiler compiler = new SassCompiler(string.Empty);
            return compiler.CompileSass(input, path);
        }
        #endregion
    }
}
