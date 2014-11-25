// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JavaScriptHandler.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The css handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Web
{
    #region Using

    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using Cruncher.Caching;
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    using Cruncher.Preprocessors;
    using Cruncher.Web.Configuration;
    #endregion

    /// <summary>
    /// The JavaScript handler.
    /// </summary>
    public class JavaScriptHandler : HandlerBase
    {
        /// <summary>
        /// The JavaScript cruncher.
        /// </summary>
        private JavaScriptCruncher javaScriptCruncher;

        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public override bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Processes the JavaScript request using cruncher and returns the result.
        /// </summary>
        /// <param name="path">
        /// The path to the resources to crunch.
        /// </param>
        /// <param name="minify">
        /// Whether to minify the output.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> representing the processed result.
        /// </returns>
        public string ProcessJavascriptCrunch(string path, bool minify)
        {
            string key = path.ToMd5Fingerprint();
            string combinedJavaScript = string.Empty;

            if (!string.IsNullOrWhiteSpace(path))
            {
                minify = minify || CruncherConfiguration.Instance.MinifyJavaScript;
                combinedJavaScript = (string)CacheManager.GetItem(key);

                if (string.IsNullOrWhiteSpace(combinedJavaScript))
                {
                    string[] javaScriptFiles = path.Split('|');
                    StringBuilder stringBuilder = new StringBuilder();

                    CruncherOptions cruncherOptions = new CruncherOptions
                                                          {
                                                              MinifyCacheKey = path,
                                                              Minify = minify,
                                                              AllowRemoteFiles = CruncherConfiguration.Instance.AllowRemoteDownloads,
                                                              RemoteFileMaxBytes = CruncherConfiguration.Instance.MaxBytes,
                                                              RemoteFileTimeout = CruncherConfiguration.Instance.Timeout
                                                          };

                    cruncherOptions.CacheFiles = cruncherOptions.Minify;
                    cruncherOptions.CacheLength = cruncherOptions.Minify ? CruncherConfiguration.Instance.MaxCacheDays : 0;

                    this.javaScriptCruncher = new JavaScriptCruncher(cruncherOptions);

                    // Loop through and process each file.
                    foreach (string javaScriptFile in javaScriptFiles)
                    {
                        // Local files.
                        if (PreprocessorManager.Instance.AllowedExtensionsRegex.IsMatch(javaScriptFile))
                        {
                            List<string> files = new List<string>();

                            // Try to get the file using absolute/relative path
                            if (!ResourceHelper.IsResourceFilenameOnly(javaScriptFile))
                            {
                                string javaScriptFilePath = ResourceHelper.GetFilePath(javaScriptFile, cruncherOptions.RootFolder);

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
                                            files.AddRange(Directory.GetFiles(directoryInfo.FullName, javaScriptFile, SearchOption.AllDirectories));
                                        }
                                    }
                                }
                            }

                            if (files.Any())
                            {
                                // We only want the first file.
                                string first = files.FirstOrDefault();
                                cruncherOptions.RootFolder = Path.GetDirectoryName(first);
                                stringBuilder.Append(this.javaScriptCruncher.Crunch(first));
                            }
                        }
                        else
                        {
                            // Remote files.
                            string remoteFile = this.GetUrlFromToken(javaScriptFile).ToString();
                            stringBuilder.Append(this.javaScriptCruncher.Crunch(remoteFile));
                        }
                    }

                    // Minify and fix any missing semicolons between IIFE's
                    combinedJavaScript = this.javaScriptCruncher.Minify(stringBuilder.ToString())
                        .Replace(")(function(", ");(function(");
                }
            }

            return combinedJavaScript;
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public override void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            string path = request.QueryString["path"];
            string key = path.ToMd5Fingerprint();
            bool fallback;
            bool minify = bool.TryParse(request.QueryString["minify"], out fallback);

            if (!string.IsNullOrWhiteSpace(path))
            {
                string combinedJavaScript = this.ProcessJavascriptCrunch(path, minify);

                if (!string.IsNullOrWhiteSpace(combinedJavaScript))
                {
                    ConcurrentBag<string> fileMonitors;

                    // Configure response headers
                    if (this.javaScriptCruncher == null)
                    {
                        // There should always be a valid list of monitors in the cache.
                        fileMonitors = (ConcurrentBag<string>)CacheManager.GetItem(key + "_FILE_MONITORS");
                    }
                    else
                    {
                        fileMonitors = this.javaScriptCruncher.FileMonitors;
                    }

                    this.SetHeaders(path, context, ResponseType.JavaScript, minify, fileMonitors);
                    context.Response.Write(combinedJavaScript);

                    // Compress the response if applicable.
                    if (CruncherConfiguration.Instance.CompressResources)
                    {
                        CompressionModule.CompressResponse(context);
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.StatusDescription = HttpStatusCode.NotFound.ToString();
                }
            }
        }
        #endregion
    }
}
