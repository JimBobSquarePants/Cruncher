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
        private static readonly Regex ImportsRegex = new Regex(@"((?:@import\s*(url\([""']?)\s*(?<filename>[^.]+\.\w+ss)(\s*[""']?)\s*\))((?<media>([^;@]+))?);)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
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
        #region Public
        /// <summary>
        /// Minifies the specified resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The minified resource.</returns>
        public override string Minify(string resource)
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

            string result = minifier.Minify(resource);

            if (this.Options.CacheFiles)
            {
                this.AddItemToCache(this.Options.MinifyCacheKey, result);
            }

            return result;
        }
        #endregion

        #region Protected
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

            contents = this.ParseImports(contents);

            contents = this.PreProcessInput(contents, file);

            // Cache if applicable.
            this.AddFileMonitor(file, contents);

            return contents;
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
                .First(preprocessor => preprocessor.AllowedExtensions == null)
                .Transform(input, path, this);

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
                CaptureCollection fileCaptures = groups["filename"].Captures;

                if (fileCaptures.Count > 0)
                {
                    string fileName = fileCaptures[0].ToString();
                    CaptureCollection mediaQueries = groups["media"].Captures;
                    Capture mediaQuery = null;

                    if (mediaQueries.Count > 0)
                    {
                        mediaQuery = mediaQueries[0];
                    }

                    string importedCSS = string.Empty;

                    if (!fileName.Contains("://"))
                    {
                        // Check and add the @import the match.
                        FileInfo fileInfo = new FileInfo(Path.GetFullPath(Path.Combine(Options.RootFolder, fileName)));

                        // Read the file.
                        if (fileInfo.Exists)
                        {
                            string file = fileInfo.FullName;

                            using (StreamReader reader = new StreamReader(file))
                            {
                                // Parse the children.
                                importedCSS = mediaQuery != null
                                                  ? string.Format(
                                                      CultureInfo.InvariantCulture,
                                                      "@media {0}{{{1}{2}{1}}}",
                                                      mediaQuery,
                                                      Environment.NewLine,
                                                      this.ParseImports(reader.ReadToEnd()))
                                                  : this.ParseImports(reader.ReadToEnd());
                            }

                            // Cache if applicable.
                            this.AddFileMonitor(file, importedCSS);
                        }

                        // Replace the regex match with the full qualified css.
                        css = css.Replace(match.Value, importedCSS);
                    }
                }
            }

            return css;
        }
        #endregion
        #endregion
    }
}
