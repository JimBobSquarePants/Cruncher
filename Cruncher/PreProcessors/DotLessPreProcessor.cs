#region Licence
// -----------------------------------------------------------------------
// <copyright file="DotLessPreProcessor.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.PreProcessors
{
    #region Using
    using dotless.Core;
    using dotless.Core.configuration;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// Provides methods to convert LESS into CSS.
    /// </summary>
    public class DotLessPreProcessor : IPreProcessor
    {
        #region Properties
        private readonly Regex ImportsRegex = new Regex(@"@import(-once|)\s*[\""'](?<filename>[^.\""']+(\.css|\.less))[\""'];", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// An instance of the DotlessConfiguration.
        /// </summary>
        private static readonly DotlessConfiguration Config = new DotlessConfiguration
        {
            CacheEnabled = false,
            MinifyOutput = false,
        };

        /// <summary>
        /// The Engine Factory that will perform the preprocessing.
        /// </summary>
        private static readonly EngineFactory EngineFactory = new EngineFactory(Config);
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
                // Replace the Imports statements as they cause an error as the preprocessor
                // tries to import them when in native .less format.
                foreach (Match match in ImportsRegex.Matches(input))
                {
                    // Recursivly parse the css for imports.
                    GroupCollection groups = match.Groups;
                    Capture fileName = groups["filename"].Captures[0];
                    string normalizedCss = string.Format("@import({0});", fileName.ToString());

                    input = input.Replace(match.Value, normalizedCss);

                }

                return EngineFactory.GetEngine().TransformToCss(input, null);
            }
            catch
            {
                // This is 
                return input;
            }
        }
    }
}
