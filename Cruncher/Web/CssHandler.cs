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
    using Cruncher.Helpers;
    using Cruncher.Preprocessors;
    using Cruncher.Web.Configuration;
    #endregion

    /// <summary>
    /// The css handler.
    /// </summary>
    public class CssHandler : HandlerBase
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
            string minify = request.QueryString["minify"];

            if (!string.IsNullOrWhiteSpace(path))
            {
                string[] cssFiles = path.Split('|');
                StringBuilder stringBuilder = new StringBuilder();

                CruncherOptions cruncherOptions = new CruncherOptions
                                                      {
                                                          Minify = !string.IsNullOrWhiteSpace(minify) || CruncherConfiguration.Instance.MinifyCSS,
                                                          AllowRemoteFiles = CruncherConfiguration.Instance.AllowRemoteDownloads,
                                                          RemoteFileMaxBytes = CruncherConfiguration.Instance.MaxBytes,
                                                          RemoteFileTimeout = CruncherConfiguration.Instance.Timeout
                                                      };

                cruncherOptions.CacheFiles = cruncherOptions.Minify;
                cruncherOptions.CacheLength = cruncherOptions.Minify ? CruncherConfiguration.Instance.MaxCacheDays : 0;

                // Loop through and process each file.
                foreach (string cssFile in cssFiles)
                {
                    CssCruncher cssCruncher = new CssCruncher(cruncherOptions);

                    // Local files.
                    if (PreprocessorManager.Instance.AllowedExtensionsRegex.IsMatch(cssFile))
                    {
                        // Get the path from the server.
                        // Loop through each possible directory.
                        List<string> files =
                            CruncherConfiguration.Instance.CSSPaths.SelectMany(
                                cssFolder =>
                                Directory.GetFiles(
                                    HttpContext.Current.Server.MapPath(cssFolder),
                                    cssFile,
                                    SearchOption.AllDirectories)).ToList();

                        // We only want the first file.
                        string first = files.FirstOrDefault();

                        stringBuilder.Append(cssCruncher.Crunch(first));
                    }
                    else
                    {
                        // Remote files.
                        string remoteFile = this.GetUrlFromToken(cssFile).ToString();
                        stringBuilder.Append(cssCruncher.Crunch(remoteFile));
                    }
                }

                string combinedCSS = stringBuilder.ToString();

                if (!string.IsNullOrWhiteSpace(combinedCSS))
                {
                    // Configure response headers
                    this.SetHeaders(combinedCSS.GetHashCode(), context, ResponseType.Css, cruncherOptions.Minify);
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
                    context.Response.Status = HttpStatusCode.NotFound.ToString();
                }
            }
        }
        #endregion
    }
}
