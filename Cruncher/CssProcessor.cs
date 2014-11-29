// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CssProcessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The CSS processor for processing CSS files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using Cruncher.Caching;
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    using Cruncher.Postprocessors.AutoPrefixer;
    using Cruncher.Preprocessors;
    using Cruncher.Configuration;

    /// <summary>
    /// The CSS processor for processing CSS files.
    /// </summary>
    public class CssProcessor : ProcessorBase
    {
        /// <summary>
        /// Processes the css request using cruncher and returns the result.
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
        public string ProcessCssCrunch(bool minify, params string[] paths)
        {
            string combinedCSS = string.Empty;

            if (paths != null)
            {
                string key = string.Join(string.Empty, paths).ToMd5Fingerprint();
                combinedCSS = (string)CacheManager.GetItem(key);

                if (string.IsNullOrWhiteSpace(combinedCSS))
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

                    cruncherOptions.CacheFiles = cruncherOptions.Minify;

                    CssCruncher cssCruncher = new CssCruncher(cruncherOptions);

                    AutoPrefixerOptions autoPrefixerOptions = CruncherConfiguration.Instance.AutoPrefixerOptions;

                    // Loop through and process each file.
                    foreach (string path in paths)
                    {
                        // Local files.
                        if (PreprocessorManager.Instance.AllowedExtensionsRegex.IsMatch(path))
                        {
                            List<string> files = new List<string>();

                            // Try to get the file by absolute/relative path
                            if (!ResourceHelper.IsResourceFilenameOnly(path))
                            {
                                string cssFilePath = ResourceHelper.GetFilePath(path, cruncherOptions.RootFolder);

                                if (File.Exists(cssFilePath))
                                {
                                    files.Add(cssFilePath);
                                }
                            }
                            else
                            {
                                // Get the path from the server.
                                // Loop through each possible directory.
                                foreach (string cssPath in CruncherConfiguration.Instance.CSSPaths)
                                {
                                    if (!string.IsNullOrWhiteSpace(cssPath) && cssPath.Trim().StartsWith("~/"))
                                    {
                                        DirectoryInfo directoryInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath(cssPath));

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
                                stringBuilder.Append(cssCruncher.Crunch(first));
                            }
                        }
                        else
                        {
                            // Remote files.
                            string remoteFile = this.GetUrlFromToken(path).ToString();
                            stringBuilder.Append(cssCruncher.Crunch(remoteFile));
                        }
                    }

                    combinedCSS = stringBuilder.ToString();

                    // Apply autoprefixer
                    combinedCSS = cssCruncher.AutoPrefix(combinedCSS, autoPrefixerOptions);

                    if (minify)
                    {
                        combinedCSS = cssCruncher.Minify(combinedCSS);
                        this.AddItemToCache(key, combinedCSS, cssCruncher.FileMonitors);
                    }
                }
            }

            return combinedCSS;
        }
    }
}
