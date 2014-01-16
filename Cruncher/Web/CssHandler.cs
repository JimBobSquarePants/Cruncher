// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CssHandler.cs" company="James South">
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
    /// The CSS handler.
    /// </summary>
    public class CssHandler : HandlerBase
    {
        /// <summary>
        /// The css cruncher.
        /// </summary>
        private CssCruncher cssCruncher;

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
            string key = path.ToMd5Fingerprint();
            bool fallback;
            bool minify = bool.TryParse(request.QueryString["minify"], out fallback);

            if (!string.IsNullOrWhiteSpace(path))
            {
                minify = minify || CruncherConfiguration.Instance.MinifyCSS;
                string combinedCSS = (string)CacheManager.GetItem(key);

                if (string.IsNullOrWhiteSpace(combinedCSS))
                {
                    string[] cssFiles = path.Split('|');
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
                    minify = cruncherOptions.Minify;

                    this.cssCruncher = new CssCruncher(cruncherOptions);

                    // Loop through and process each file.
                    foreach (string cssFile in cssFiles)
                    {
                        // Local files.
                        if (PreprocessorManager.Instance.AllowedExtensionsRegex.IsMatch(cssFile))
                        {
                            // Get the path from the server.
                            // Loop through each possible directory.
                            List<string> files = new List<string>();

                            foreach (string cssFolder in CruncherConfiguration.Instance.CSSPaths)
                            {
                                if (!string.IsNullOrWhiteSpace(cssFolder) && cssFolder.Trim().StartsWith("~/"))
                                {
                                    DirectoryInfo directoryInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath(cssFolder));

                                    if (directoryInfo.Exists)
                                    {
                                        files.AddRange(Directory.GetFiles(directoryInfo.FullName, cssFile, SearchOption.AllDirectories));
                                    }
                                }
                            }

                            if (files.Any())
                            {
                                // We only want the first file.
                                string first = files.FirstOrDefault();
                                cruncherOptions.RootFolder = Path.GetDirectoryName(first);
                                stringBuilder.Append(this.cssCruncher.Crunch(first));
                            }
                        }
                        else
                        {
                            // Remote files.
                            string remoteFile = this.GetUrlFromToken(cssFile).ToString();
                            stringBuilder.Append(this.cssCruncher.Crunch(remoteFile));
                        }
                    }

                    combinedCSS = this.cssCruncher.Minify(stringBuilder.ToString());
                }

                if (!string.IsNullOrWhiteSpace(combinedCSS))
                {
                    IList<string> fileMonitors;

                    // Configure response headers
                    if (this.cssCruncher == null)
                    {
                        // There should always be a valid list of monitors in the cache.
                        fileMonitors = (List<string>)CacheManager.GetItem(key + "_FILE_MONITORS");
                    }
                    else
                    {
                        fileMonitors = this.cssCruncher.FileMonitors;
                    }

                    // Configure response headers
                    this.SetHeaders(path, context, ResponseType.Css, minify, fileMonitors);
                    context.Response.Write(combinedCSS);

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
