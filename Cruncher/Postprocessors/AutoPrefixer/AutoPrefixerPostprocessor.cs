// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoPrefixerPostprocessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The auto prefixer postprocessor.
//   by using Andrey Sitnik's Autoprefixer
//   <see href="https://bundletransformer.codeplex.com" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Postprocessors.AutoPrefixer
{
    using System.Diagnostics.CodeAnalysis;

    using Cruncher.Configuration;

    /// <summary>
    /// The auto prefixer postprocessor.
    /// by using Andrey Sitnik's Autoprefixer
    /// <see href="https://bundletransformer.codeplex.com"/>
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class AutoPrefixerPostprocessor
    {
        /// <summary>
        /// Transforms the content of the given string. 
        /// </summary>
        /// <param name="input">
        /// The input string to transform.
        /// </param>
        /// <param name="options">
        /// The <see cref="AutoPrefixerOptions"/>.
        /// </param>
        /// <returns>
        /// The transformed string.
        /// </returns>
        public string Transform(string input, AutoPrefixerOptions options)
        {
            if (!options.Enabled)
            {
                return input;
            }

            using (AutoPrefixerProcessor processor = new AutoPrefixerProcessor(CruncherConfiguration.Instance.JsEngineFunc))
            {
                input = processor.Process(input, options);
            }

            return input;
        }
    }
}
