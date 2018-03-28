// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SassCompiler.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The sass compiler for processing and compiling sass and scss files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Sass
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using LibSassNet;

    /// <summary>
    /// The sass compiler for processing and compiling sass and scss files.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    internal sealed class SassCompiler
    {
        /// <summary>
        /// The sass compiler.
        /// </summary>
        private readonly ISassCompiler compiler = new LibSassNet.SassCompiler();

        /// <summary>
        /// The sass to scss converter.
        /// </summary>
        private readonly ISassToScssConverter converter = new SassToScssConverter();

        /// <summary>
        /// The compiler mode.
        /// </summary>
        public enum CompilerMode
        {
            /// <summary>
            /// Processes .sass files.
            /// </summary>
            Sass,

            /// <summary>
            /// Processes .scss files.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
            Scss
        }

        /// <summary>
        /// Gets a string containing the compiled sass output.
        /// </summary>
        /// <param name="input">
        /// The input to compile.
        /// </param>
        /// <param name="fileName">
        /// The name of the file to compile.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> containing the compiled sass output.
        /// </returns>
        public string CompileSass(string input, string fileName)
        {
            try
            {
                CompilerMode mode = CompilerMode.Scss;

                if (Regex.IsMatch(fileName, @"\.sass$"))
                {
                    mode = CompilerMode.Sass;
                }

                string processedInput = mode == CompilerMode.Scss ? input : this.ConvertToScss(input);
                var includePaths = new List<string> { Path.GetDirectoryName(fileName) };
                var includePathRaw = ConfigurationManager.AppSettings["SassIncludePaths"];
                //throw new Exception("Include Paths Found in AppSettings: " + includePathRaw);
                if (!string.IsNullOrEmpty(includePathRaw))
                {
                    var split = includePathRaw.Split(',');
                    //throw new Exception("First Path Converted Found in AppSettings: " + AppDomain.CurrentDomain.BaseDirectory + split[0]);
                    includePaths.AddRange(split.Where(i => Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + i))
                        .Select(i => AppDomain.CurrentDomain.BaseDirectory + i));
                }
                //throw new Exception("Include Paths Passed to Sass Compiler: " + string.Join(",", includePaths));
                return this.compiler.Compile(processedInput, OutputStyle.Nested, false, 5, includePaths.ToArray());
            }
            catch (Exception ex)
            {
                throw new SassAndScssCompilingException(ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Converts sass to scss.
        /// </summary>
        /// <param name="input">
        /// The input string to convert.
        /// </param>
        /// <returns>
        /// The converted <see cref="string"/>.
        /// </returns>
        internal string ConvertToScss(string input)
        {
            return this.converter.Convert(input);
        }
    }
}
