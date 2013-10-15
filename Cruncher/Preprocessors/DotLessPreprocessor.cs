#region Licence
// -----------------------------------------------------------------------
// <copyright file="DotLessPreprocessor.cs" company="James South">
//     Copyright (c) James South.
//     Licensed under the Apache License, Version 2.0.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.Preprocessors
{
    #region Using

    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using dotless.Core;
    using dotless.Core.configuration;
    #endregion

    /// <summary>
    /// Provides methods to convert LESS into CSS.
    /// </summary>
    public class DotLessPreprocessor : IPreprocessor
    {
        #region Fields
        /// <summary>
        /// The regex for matching import statements.
        /// </summary>
        private static readonly Regex ImportsRegex = new Regex(@"@import(-once|)\s*[\""'](?<filename>[^.\""']+(\.CSS|\.LESS))[\""'];", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// An instance of the <see cref="DotlessConfiguration"/>.
        /// </summary>
        private static readonly DotlessConfiguration Config = new DotlessConfiguration
        {
            CacheEnabled = false,
            MinifyOutput = false
        };

        /// <summary>
        /// The Engine Factory that will perform the preprocessing.
        /// </summary>
        private static readonly EngineFactory EngineFactory = new EngineFactory(Config);
        #endregion

        #region Properties
        /// <summary>
        /// Gets the extension that this filter processes.
        /// </summary>
        public string AllowedExtension
        {
            get
            {
                return ".LESS";
            }
        }
        #endregion

        /// <summary>
        /// Transforms the content of the given string from Less into CSS. 
        /// </summary>
        /// <param name="input">The input Less string to transform.</param>
        /// <param name="path">The path to the given input Less string to transform.</param>
        /// <returns>The transformed CSS string.</returns>
        public string Transform(string input, string path)
        {
            try
            {
                return EngineFactory.GetEngine().TransformToCss(input, path);
            }
            catch
            {
                // Replace the Imports statements as they cause an error as the Preprocessor
                // tries to import them when in native .less format. They will get processed
                // down the line in the CSS handler.
                foreach (Match match in ImportsRegex.Matches(input))
                {
                    // Parse the CSS for imports.
                    GroupCollection groups = match.Groups;
                    Capture fileName = groups["filename"].Captures[0];
                    string normalizedCss = string.Format(CultureInfo.InvariantCulture, "@import({0});", fileName);

                    input = input.Replace(match.Value, normalizedCss);
                }

                return input;
            }
        }
    }
}
