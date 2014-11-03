// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SassCompiler.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The sass compiler.
//   Much thanks here to Paul Betts' SassAndCoffee project (https://github.com/xpaulbettsx/SassAndCoffee)
//   and Justin Etheridge's SquishIt project https://github.com/jetheredge/SquishIt
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Sass
{
    #region Using
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.RegularExpressions;

    using LibSassNet;
    #endregion

    /// <summary>
    /// The sass compiler.
    /// Much thanks here to Paul Betts' SassAndCoffee project (https://github.com/xpaulbettsx/SassAndCoffee)
    /// and Justin Etheridge's SquishIt project https://github.com/jetheredge/SquishIt
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    internal sealed class SassCompiler
    {
        #region Fields
        /// <summary>
        /// The sass compiler.
        /// </summary>
        private readonly ISassCompiler compiler = new LibSassNet.SassCompiler();

        /// <summary>
        /// The sass to scss converter.
        /// </summary>
        private readonly ISassToScssConverter converter = new SassToScssConverter();
        #endregion

        #region Properties
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
        #endregion

        #region Methods
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
                return this.compiler.Compile(processedInput, OutputStyle.Nested, false, 5, new[] { Path.GetDirectoryName(fileName) });
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
        #endregion
    }
}
