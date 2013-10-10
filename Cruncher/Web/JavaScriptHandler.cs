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
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public override void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            string path = request.QueryString["path"];
            bool fallback;
            bool minify = bool.TryParse(request.QueryString["minify"], out fallback);

            if (!string.IsNullOrWhiteSpace(path))
            {
                string combinedJavaScript = (string)CacheManager.GetItem(path.ToMd5Fingerprint());

                if (string.IsNullOrWhiteSpace(combinedJavaScript))
                {
                    string[] javaScriptFiles = path.Split('|');
                    StringBuilder stringBuilder = new StringBuilder();

                    CruncherOptions cruncherOptions = new CruncherOptions
                                                          {
                                                              MinifyCacheKey = path,
                                                              Minify = minify || CruncherConfiguration.Instance.MinifyJavaScript,
                                                              AllowRemoteFiles = CruncherConfiguration.Instance.AllowRemoteDownloads,
                                                              RemoteFileMaxBytes = CruncherConfiguration.Instance.MaxBytes,
                                                              RemoteFileTimeout = CruncherConfiguration.Instance.Timeout
                                                          };

                    cruncherOptions.CacheFiles = cruncherOptions.Minify;
                    cruncherOptions.CacheLength = cruncherOptions.Minify ? CruncherConfiguration.Instance.MaxCacheDays : 0;
                    minify = cruncherOptions.Minify;

                    JavaScriptCruncher javaScriptCruncher = new JavaScriptCruncher(cruncherOptions);

                    // Loop through and process each file.
                    foreach (string javaScriptFile in javaScriptFiles)
                    {
                        // Local files.
                        if (PreprocessorManager.Instance.AllowedExtensionsRegex.IsMatch(javaScriptFile))
                        {
                            // Get the path from the server.
                            // Loop through each possible directory.
                            List<string> files =
                                CruncherConfiguration.Instance.JavaScriptPaths.SelectMany(
                                    cssFolder =>
                                    Directory.GetFiles(
                                        HttpContext.Current.Server.MapPath(cssFolder),
                                        javaScriptFile,
                                        SearchOption.AllDirectories)).ToList();

                            // We only want the first file.
                            string first = files.FirstOrDefault();
                            cruncherOptions.RootFolder = Path.GetDirectoryName(first);
                            stringBuilder.Append(javaScriptCruncher.Crunch(first));
                        }
                        else
                        {
                            // Remote files.
                            string remoteFile = this.GetUrlFromToken(javaScriptFile).ToString();
                            stringBuilder.Append(javaScriptCruncher.Crunch(remoteFile));
                        }
                    }

                    // Minify and fix any missing semicolons between IIFE's
                    combinedJavaScript = javaScriptCruncher.Minify(stringBuilder.ToString())
                        .Replace(")(function(", ");(function(");
                }

                if (!string.IsNullOrWhiteSpace(combinedJavaScript))
                {
                    // Configure response headers
                    this.SetHeaders(combinedJavaScript.GetHashCode(), context, ResponseType.JavaScript, minify);
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
