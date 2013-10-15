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
        #region Fields

        /// <summary>
        /// The imports regex.
        /// </summary>
        private static readonly Regex ImportsRegex = new Regex(@"@import\s*[""'](?<filename>[^""']+)[""']\s*;", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

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
        /// <param name="cruncher">The cruncher that is running the transform.</param>
        /// <returns>The transformed string.</returns>
        public string Transform(string input, string path, CruncherBase cruncher)
        {
            // Unfortunately there's no way I know of pulling a list of imports out of
            // Sass so we have to do a manual loop to get the list of file dependencies.
            // This should only happen once though.
            if (cruncher.Options.CacheFiles)
            {
                this.AddImportsToFileMonitors(input, cruncher);
            }

            SassCompiler compiler = new SassCompiler();
            return compiler.CompileSass(input, path);
        }

        /// <summary>
        /// Parses the string for CSS imports and replaces them with the referenced CSS.
        /// </summary>
        /// <param name="sass">
        /// The SASS to parse for import statements.
        /// </param>
        /// <param name="cruncher">The cruncher that is running the transform.</param>
        private void AddImportsToFileMonitors(string sass, CruncherBase cruncher)
        {
            // Check for imports and parse if necessary.
            if (!sass.Contains("@import", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Recursively parse the sass for imports.
            foreach (Match match in ImportsRegex.Matches(sass))
            {
                // Recursively parse the sass for imports.
                GroupCollection groups = match.Groups;
                CaptureCollection fileCaptures = groups["filename"].Captures;

                if (fileCaptures.Count > 0)
                {
                    string fileName = fileCaptures[0].ToString();

                    if (!fileName.Contains("://"))
                    {
                        // Check and add the @import the match.
                        FileInfo fileInfo =
                            new FileInfo(Path.GetFullPath(Path.Combine(cruncher.Options.RootFolder, fileName)));

                        // Read the file.
                        if (fileInfo.Exists)
                        {
                            string file = fileInfo.FullName;

                            using (StreamReader reader = new StreamReader(file))
                            {
                                // Recursively check the children.
                                this.AddImportsToFileMonitors(reader.ReadToEnd(), cruncher);
                            }

                            // Cache if applicable.
                            cruncher.AddFileMonitor(file, "not empty");
                        }
                    }
                }
            }
        }
        #endregion
    }
}
