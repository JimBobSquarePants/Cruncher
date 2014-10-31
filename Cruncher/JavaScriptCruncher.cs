// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JavaScriptCruncher.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The JavaScript cruncher.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    #region Using
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    using Cruncher.Compression;
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    #endregion

    /// <summary>
    /// The JavaScript cruncher.
    /// </summary>
    public class JavaScriptCruncher : CruncherBase
    {
        #region Fields
        /// <summary>
        /// The regular expression to search files for.
        /// </summary>
        private static readonly Regex ImportsRegex = new Regex(@"(?:import\s*([""']?)\s*(?<filename>.+\.js)(\s*[""']?)\s*);", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptCruncher"/> class.
        /// </summary>
        /// <param name="options">The options containing instructions for the cruncher.</param>
        public JavaScriptCruncher(CruncherOptions options)
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
        /// <returns>
        /// The minified resource.
        /// </returns>
        public override string Minify(string resource)
        {
            JavaScriptMinifier minifier;

            if (this.Options.Minify)
            {
                minifier = new JavaScriptMinifier
                {
                    VariableMinification = VariableMinification.LocalVariablesAndFunctionArguments
                };
            }
            else
            {
                minifier = new JavaScriptMinifier
                {
                    VariableMinification = VariableMinification.None,
                    PreserveFunctionNames = true,
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
        #endregion

        #region Private
        /// <summary>
        /// Parses the string for Javascript imports and replaces them with the referenced Javascript.
        /// </summary>
        /// <param name="javascript">
        /// The Javascript to parse for import statements.
        /// </param>
        /// <returns>The Javascript file parsed for imports.</returns>
        private string ParseImports(string javascript)
        {
            // Check for imports and parse if necessary.
            if (!javascript.Contains("import", StringComparison.OrdinalIgnoreCase))
            {
                return javascript;
            }

            // Recursively parse the javascript for imports.
            foreach (Match match in ImportsRegex.Matches(javascript))
            {
                // Recursively parse the javascript for imports.
                GroupCollection groups = match.Groups;
                CaptureCollection fileCaptures = groups["filename"].Captures;

                if (fileCaptures.Count > 0)
                {
                    string fileName = fileCaptures[0].ToString();
                    string importedJavascript = string.Empty;

                    // Check and add the @import the match.
                    FileInfo fileInfo = null;

                    // Try to get the file by absolute/relative path
                    if (!ResourceHelper.IsResourceFilenameOnly(fileName))
                    {
                        string cssFilePath = ResourceHelper.GetFilePath(fileName);
                        if (File.Exists(cssFilePath))
                        {
                            fileInfo = new FileInfo(cssFilePath);
                        }
                    }
                    else
                    {
                        fileInfo = new FileInfo(Path.GetFullPath(Path.Combine(Options.RootFolder, fileName)));
                    }

                    // Read the file.
                    if (fileInfo != null && fileInfo.Exists)
                    {
                        string file = fileInfo.FullName;

                        using (StreamReader reader = new StreamReader(file))
                        {
                            // Parse the children.
                            importedJavascript = this.ParseImports(reader.ReadToEnd());
                        }

                        // Cache if applicable.
                        this.AddFileMonitor(file, importedJavascript);
                    }

                    // Replace the regex match with the full qualified javascript.
                    javascript = javascript.Replace(match.Value, importedJavascript);
                }
            }

            return javascript;
        }
        #endregion
        #endregion
    }
}
