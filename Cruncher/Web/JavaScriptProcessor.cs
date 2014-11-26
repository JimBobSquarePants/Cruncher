// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JavaScriptProcessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The JavaScript processor for processing JavaScript files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Web
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using Cruncher.Caching;
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    using Cruncher.Preprocessors;
    using Cruncher.Web.Configuration;

    /// <summary>
    /// The JavaScript processor for processing JavaScript files.
    /// </summary>
    public class JavaScriptProcessor : ProcessorBase
    {
        /// <summary>
        /// Processes the JavaScript request using cruncher and returns the result.
        /// </summary>
        /// <param name="minify">
        /// Whether to minify the output.
        /// </param>
        /// <param name="paths">
        /// The paths to the resources to crunch.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> representing the processed result.
        /// </returns>
        public string ProcessJavascriptCrunch(bool minify, params string[] paths)
        {
            string combinedJavaScript = string.Empty;

            if (paths != null)
            {
                string key = string.Join(string.Empty, paths).ToMd5Fingerprint();
                combinedJavaScript = (string)CacheManager.GetItem(key);

                if (string.IsNullOrWhiteSpace(combinedJavaScript))
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    CruncherOptions cruncherOptions = new CruncherOptions
                    {
                        MinifyCacheKey = key,
                        Minify = minify,
                        CacheFiles = minify,
                        AllowRemoteFiles = CruncherConfiguration.Instance.AllowRemoteDownloads,
                        RemoteFileMaxBytes = CruncherConfiguration.Instance.MaxBytes,
                        RemoteFileTimeout = CruncherConfiguration.Instance.Timeout
                    };

                    JavaScriptCruncher javaScriptCruncher = new JavaScriptCruncher(cruncherOptions);

                    // Loop through and process each file.
                    foreach (string path in paths)
                    {
                        // Local files.
                        if (PreprocessorManager.Instance.AllowedExtensionsRegex.IsMatch(path))
                        {
                            List<string> files = new List<string>();

                            // Try to get the file using absolute/relative path
                            if (!ResourceHelper.IsResourceFilenameOnly(path))
                            {
                                string javaScriptFilePath = ResourceHelper.GetFilePath(path, cruncherOptions.RootFolder);

                                if (File.Exists(javaScriptFilePath))
                                {
                                    files.Add(javaScriptFilePath);
                                }
                            }
                            else
                            {
                                // Get the path from the server.
                                // Loop through each possible directory.
                                foreach (string javaScriptFolder in CruncherConfiguration.Instance.JavaScriptPaths)
                                {
                                    if (!string.IsNullOrWhiteSpace(javaScriptFolder) && javaScriptFolder.Trim().StartsWith("~/"))
                                    {
                                        DirectoryInfo directoryInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath(javaScriptFolder));

                                        if (directoryInfo.Exists)
                                        {
                                            files.AddRange(Directory.GetFiles(directoryInfo.FullName, path, SearchOption.AllDirectories));
                                        }
                                    }
                                }
                            }

                            if (files.Any())
                            {
                                // We only want the first file.
                                string first = files.FirstOrDefault();
                                cruncherOptions.RootFolder = Path.GetDirectoryName(first);
                                stringBuilder.Append(javaScriptCruncher.Crunch(first));
                            }
                        }
                        else
                        {
                            // Remote files.
                            string remoteFile = this.GetUrlFromToken(path).ToString();
                            stringBuilder.Append(javaScriptCruncher.Crunch(remoteFile));
                        }
                    }

                    combinedJavaScript = stringBuilder.ToString();

                    if (minify)
                    {
                        // Minify and fix any missing semicolons between function expressions.
                        combinedJavaScript = javaScriptCruncher.Minify(combinedJavaScript)
                            .Replace(")(function(", ");(function(");
                        
                        this.AddItemToCache(key, combinedJavaScript, javaScriptCruncher.FileMonitors);
                    }
                }
            }

            return combinedJavaScript;
        }
    }
}
