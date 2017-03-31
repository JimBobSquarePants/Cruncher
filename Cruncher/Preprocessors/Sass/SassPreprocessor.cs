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
    #region Using
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using Cruncher.Extensions;
    #endregion

    /// <summary>
    /// Provides methods to convert SASS and SCSS into CSS.
    /// </summary>
    public class SassPreprocessor : IPreprocessor
    {
        /// <summary>
        /// Gets the extensions that this filter processes.
        /// </summary>
        public string[] AllowedExtensions => new[] { ".SASS", ".SCSS" };

        /// <summary>
        /// Transforms the content of the given string.
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the given input string to transform.</param>
        /// <param name="cruncher">The cruncher that is running the transform.</param>
        /// <returns>The transformed string.</returns>
        public string Transform(string input, string path, CruncherBase cruncher)
        {
            try
            {
                var options = new LibSass.Compiler.Options.SassOptions 
                {
                    InputPath = path,
                    OutputStyle = LibSass.Compiler.Options.SassOutputStyle.Expanded,
                    Precision = 5,
                    IsIndentedSyntax = System.IO.Path.GetExtension(path).Equals(".sass", StringComparison.OrdinalIgnoreCase)
                };
                var compiler = new LibSass.Compiler.SassCompiler(options);
                var result = compiler.Compile();

                foreach (var file in result.IncludedFiles) 
                {
                    cruncher.AddFileMonitor(file, "not empty");
                }

                return result.Output;
            }
            catch (Exception ex)
            {
                throw new SassAndScssCompilingException(ex.Message, ex.InnerException);
            }
        }
    }
}
