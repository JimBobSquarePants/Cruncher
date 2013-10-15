// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourcePreprocessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides methods to replace relative resource paths within a stylesheet with absolute paths.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// Provides methods to replace relative resource paths within a stylesheet with absolute paths.
    /// </summary>
    public class ResourcePreprocessor : IPreprocessor
    {
        #region Fields
        /// <summary>
        /// The regular expression for matching resources within a css file.
        /// </summary>
        private static readonly Regex ResourceRegex = new Regex(@"url\(\s*(?:[""']?)(.*?)(?:[""']?)\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        #endregion

        #region Properties
        /// <summary>
        /// Gets the extension that this filter processes.
        /// </summary>
        public string[] AllowedExtensions
        {
            get
            {
                return null;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Transforms the content of the given string by replacing relative paths. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the given input string to transform.</param>
        /// <param name="cruncher">The cruncher that is running the transform.</param>
        /// <returns>The transformed string.</returns>
        public string Transform(string input, string path, CruncherBase cruncher)
        {
            try
            {
                return this.RewritePaths(input, path);
            }
            catch
            {
                return input;
            }
        }

        /// <summary>
        /// Replaces the relative paths in the given css content.
        /// </summary>
        /// <param name="css">The css within which to replace the paths.</param>
        /// <param name="oldPath">The path to replace.</param>
        /// <param name="newPath">The path to replace the old one with.</param>
        /// <returns>The css content wit the paths replaced.</returns>
        private static string ReplaceRelativePathsIn(string css, string oldPath, string newPath)
        {
            Regex regex = new Regex(@"url\(\s*[""']{0,1}" + Regex.Escape(oldPath) + @"[""']{0,1}\s*\)", RegexOptions.IgnoreCase);

            return regex.Replace(css, match => match.Value.Replace(oldPath, newPath));
        }

        /// <summary>
        /// Rewrites the relative path as relative to the application root.
        /// </summary>
        /// <param name="input">The css within which to replace the relative paths</param>
        /// <param name="path">The path to replace.</param>
        /// <returns>The css with the relative paths replaced.</returns>
        private string RewritePaths(string input, string path)
        {
            string sourceDirectory;
            bool isExternal = path.StartsWith("http", StringComparison.OrdinalIgnoreCase);

            // Parse the source directory.
            if (!isExternal)
            {
                sourceDirectory = string.Format(CultureInfo.InvariantCulture, "{0}/", Path.GetDirectoryName(path));
            }
            else
            {
                int directoryIndex = path.LastIndexOf('/');
                sourceDirectory = path.Substring(0, directoryIndex + 1);
            }

            // Get the currently operating directory.
            // http://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Uri rootUri = new Uri(rootDirectory, UriKind.Absolute);
            IEnumerable<string> relativePaths = this.GetRelativePaths(input);

            foreach (string relativePath in relativePaths)
            {
                if (!relativePath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    // Separate hashes and query-strings.
                    int hashQueryIndex = relativePath.IndexOfAny(new[] { '?', '#' });
                    string hashQuery = hashQueryIndex >= 0 ? relativePath.Substring(hashQueryIndex) : string.Empty;

                    // Parse the relative path without the hash/query-strings.
                    string capturedRelativePath = !string.IsNullOrWhiteSpace(hashQuery)
                                                ? relativePath.Substring(0, hashQueryIndex)
                                                : relativePath;

                    // Parse the Absolute path.
                    Uri resolvedSourcePath = !isExternal
                                            ? new Uri(Path.Combine(sourceDirectory, capturedRelativePath))
                                            : new Uri(new Uri(sourceDirectory, UriKind.Absolute), new Uri(capturedRelativePath, UriKind.Relative));

                    // Make it relative.
                    string resolvedOutput = rootUri.MakeRelativeUri(resolvedSourcePath).OriginalString;

                    if (!resolvedOutput.StartsWith("/"))
                    {
                        resolvedOutput = string.Format("/{0}", resolvedOutput);
                    }

                    // Add the hash/query-string
                    string newRelativePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}", resolvedOutput, hashQuery);

                    // Replace.
                    input = ReplaceRelativePathsIn(input, relativePath, newRelativePath);
                }
                else
                {
                    // We only need to adjust for externally sourced paths.
                    if (isExternal)
                    {
                        // Get the absolute url and combine it with the path.
                        sourceDirectory = new Uri(sourceDirectory, UriKind.Absolute).GetLeftPart(UriPartial.Authority);
                        input = ReplaceRelativePathsIn(input, relativePath, string.Format(CultureInfo.InvariantCulture, "{0}{1}", sourceDirectory, relativePath));
                    }
                }
            }

            return input;
        }

        /// <summary>
        /// Returns a distinct enumerable collection of relative paths from the css file.
        /// </summary>
        /// <param name="input">The css file to parse.</param>
        /// <returns>A distinct enumerable collection of relative paths from the css file.</returns>
        private IEnumerable<string> GetRelativePaths(string input)
        {
            MatchCollection matches = ResourceRegex.Matches(input);

            // Filter the matches and return distinct ones.
            return matches.Cast<Match>()
             .Select(m => m.Groups[1].Captures[0].Value)
             .Where(p => p != "\"\""
                    && p != "''"
                    && !string.IsNullOrWhiteSpace(p)
                    && !p.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && !p.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    && !p.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
             .Distinct();
        }
        #endregion
    }
}
