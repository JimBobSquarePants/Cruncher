// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CssCruncher.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The css cruncher.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    #region Using
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Cruncher.Compression;
    using Cruncher.Extensions;
    using Cruncher.Preprocessors;
    #endregion

    /// <summary>
    /// The css cruncher.
    /// </summary>
    public class CssCruncher : CruncherBase
    {
        #region Fields
        /// <summary>
        /// The regular expression to search files for.
        /// </summary>
        private static readonly Regex ImportsRegex = new Regex(@"((?:@import\s*(url\([""']?)\s*(?<filename>[^.]+\.\w+ss)(\s*[""']?)\s*\)\s*;?)((?<media>([^;]+)\s*;)?))", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CssCruncher"/> class.
        /// </summary>
        /// <param name="options">The options containing instructions for the cruncher.</param>
        public CssCruncher(CruncherOptions options)
            : base(options)
        {
        }
        #endregion

        #region Methods
        #region Protected
        /// <summary>
        /// Minifies the specified resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The minified resource.</returns>
        protected override string Minify(string resource)
        {
            CssMinifier minifier;

            if (this.Options.Minify)
            {
                minifier = new CssMinifier
                {
                    RemoveWhiteSpace = true
                };
            }
            else
            {
                minifier = new CssMinifier
                {
                    RemoveWhiteSpace = false
                };
            }

            return minifier.Minify(resource);
        }

        /// <summary>
        /// Loads the local file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <returns>
        /// The contents of the local file as a string.
        /// </returns>
        protected override string LoadLocalFile(string file)
        {
            string contents = base.LoadLocalFile(file);

            return this.ParseImports(contents);
        }

        /// <summary>
        /// Transforms the content of the given string using the correct PreProcessor. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>The transformed string.</returns>
        protected override string PreProcessInput(string input, string path)
        {
            // Do the base processing then process any specific code here. 
            input = base.PreProcessInput(input, path);

            // Run the last filter. This should be the resourcePreprocessor.
            input = PreprocessorManager.Instance.PreProcessors
                .First(p => string.IsNullOrWhiteSpace(p.AllowedExtension))
                .Transform(input, path);

            return input;
        }
        #endregion

        #region Private
        /// <summary>
        /// Parses the string for CSS imports and replaces them with the referenced CSS.
        /// </summary>
        /// <param name="css">
        /// The CSS to parse for import statements.
        /// </param>
        /// <returns>The CSS file parsed for imports.</returns>
        private string ParseImports(string css)
        {
            // Check for imports and parse if necessary.
            if (!css.Contains("@import", StringComparison.OrdinalIgnoreCase))
            {
                return css;
            }

            // Recursively parse the css for imports.
            foreach (Match match in ImportsRegex.Matches(css))
            {
                // Recursively parse the css for imports.
                GroupCollection groups = match.Groups;
                Capture fileName = groups["filename"].Captures[0];
                CaptureCollection mediaQueries = groups["media"].Captures;
                Capture mediaQuery = null;

                if (mediaQueries.Count > 0)
                {
                    mediaQuery = mediaQueries[0];
                }

                // Check and add the @import the match.
                DirectoryInfo directoryInfo = new DirectoryInfo(this.Options.RootFolder);

                FileInfo fileInfo = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                                 .FirstOrDefault(
                                     f =>
                                     f.Name.Equals(fileName.ToString(), StringComparison.InvariantCultureIgnoreCase));

                string importedCSS = string.Empty;

                // Read the file.
                if (fileInfo != null)
                {
                    string file = fileInfo.FullName;

                    using (StreamReader reader = new StreamReader(file))
                    {
                        importedCSS = mediaQuery != null
                                      ? string.Format(
                                          CultureInfo.InvariantCulture,
                                          "@media {0}{{{1}{2}{1}}}",
                                          mediaQuery,
                                          Environment.NewLine,
                                          this.ParseImports(this.PreProcessInput(reader.ReadToEnd(), file)))
                                      : this.ParseImports(this.PreProcessInput(reader.ReadToEnd(), file));
                    }

                    // Cache if applicable.
                    this.AddFileMonitor(file, importedCSS);
                }

                // Replace the regex match with the full qualified css.
                css = css.Replace(match.Value, importedCSS);
            }

            return css;
        }
        #endregion
        #endregion
    }
}
