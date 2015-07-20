// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JavaScriptProcessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The JavaScript processor for processing JavaScript files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;

    using Cruncher.Caching;
    using Cruncher.Configuration;
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    using Cruncher.Preprocessors;

    /// <summary>
    /// The JavaScript processor for processing JavaScript files.
    /// </summary>
    public class JavaScriptProcessor : ProcessorBase
    {
        /// <summary>
        /// Ensures processing is atomic.
        /// </summary>
        private static readonly AsyncDuplicateLock Locker = new AsyncDuplicateLock();

        /// <summary>
        /// Processes the JavaScript request using cruncher and returns the result.
        /// </summary>
        /// <param name="context">
        /// The current context.
        /// </param>
        /// <param name="minify">
        /// Whether to minify the output.
        /// </param>
        /// <param name="paths">
        /// The paths to the resources to crunch.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> representing the processed result.
        /// </returns>
        public async Task<string> ProcessJavascriptCrunchAsync(HttpContext context, bool minify, params string[] paths)
        {
            string combinedJavaScript = string.Empty;

            if (paths != null)
            {
                string key = string.Join(string.Empty, paths).ToMd5Fingerprint();

                using (await Locker.LockAsync(key))
                {
                    combinedJavaScript = (string)CacheManager.GetItem(key);

                    if (string.IsNullOrWhiteSpace(combinedJavaScript))
                    {
                        StringBuilder stringBuilder = new StringBuilder();

                        CruncherOptions cruncherOptions = new CruncherOptions
                        {
                            MinifyCacheKey = key,
                            Minify = minify,
                            CacheFiles = true,
                            AllowRemoteFiles = CruncherConfiguration.Instance.AllowRemoteDownloads,
                            RemoteFileMaxBytes = CruncherConfiguration.Instance.MaxBytes,
                            RemoteFileTimeout = CruncherConfiguration.Instance.Timeout
                        };

                        JavaScriptCruncher javaScriptCruncher = new JavaScriptCruncher(cruncherOptions, context);

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
                                    string javaScriptFilePath = ResourceHelper.GetFilePath(
                                        path,
                                        cruncherOptions.RootFolder,
                                        context);

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
                                        if (!string.IsNullOrWhiteSpace(javaScriptFolder)
                                            && javaScriptFolder.Trim().StartsWith("~/"))
                                        {
                                            DirectoryInfo directoryInfo =
                                                new DirectoryInfo(context.Server.MapPath(javaScriptFolder));

                                            if (directoryInfo.Exists)
                                            {
                                                files.AddRange(
                                                    Directory.GetFiles(
                                                        directoryInfo.FullName,
                                                        path,
                                                        SearchOption.AllDirectories));
                                            }
                                        }
                                    }
                                }

                                if (files.Any())
                                {
                                    // We only want the first file.
                                    string first = files.FirstOrDefault();
                                    cruncherOptions.RootFolder = Path.GetDirectoryName(first);
                                    stringBuilder.Append(await javaScriptCruncher.CrunchAsync(first));
                                }
                            }
                            else
                            {
                                // Remote files.
                                string remoteFile = this.GetUrlFromToken(path).ToString();
                                stringBuilder.Append(await javaScriptCruncher.CrunchAsync(remoteFile));
                            }
                        }

                        combinedJavaScript = stringBuilder.ToString();

                        if (minify)
                        {
                            // Minify and fix any missing semicolons between function expressions.
                            combinedJavaScript = javaScriptCruncher.Minify(combinedJavaScript);

                            if (!combinedJavaScript.EndsWith(";"))
                            {
                                combinedJavaScript += ";";
                            }
                        }

                        this.AddItemToCache(key, combinedJavaScript, javaScriptCruncher.FileMonitors);
                    }
                }
            }

            return combinedJavaScript;
        }
    }
}
