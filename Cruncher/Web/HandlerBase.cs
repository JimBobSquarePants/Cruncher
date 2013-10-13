// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerBase.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Defines the HandlerBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Web
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    using Cruncher.Web.Configuration;
    #endregion

    /// <summary>
    /// The handler base.
    /// </summary>
    public abstract class HandlerBase : IHttpHandler
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
        public abstract bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get;
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public abstract void ProcessRequest(HttpContext context);
        #endregion

        /// <summary>
        /// This will make the browser and server keep the output
        /// in its cache and thereby improve performance.
        /// See http://en.wikipedia.org/wiki/HTTP_ETag
        /// </summary>
        /// <param name="path">
        /// The combined path to the items.
        /// </param>
        /// <param name="context">
        /// the <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides 
        /// references to the intrinsic server objects 
        /// </param>
        /// <param name="responseType">
        /// The HTTP MIME type to to send.
        /// </param>
        /// <param name="futureExpire">
        /// Whether the response headers should be set to expire in the future.
        /// </param>
        /// <param name="fileMonitors">
        /// The file Monitors.
        /// </param>
        protected void SetHeaders(string path, HttpContext context, ResponseType responseType, bool futureExpire, IList<string> fileMonitors)
        {
            // Generate a hash from the combined last write times of any monitors and
            // the path.
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(path);

            foreach (string fileMonitor in fileMonitors)
            {
                FileInfo fileInfo = new FileInfo(fileMonitor);
                stringBuilder.AppendFormat("{0}", fileInfo.LastWriteTimeUtc);
            }

            int hash = stringBuilder.ToString().GetHashCode();

            HttpResponse response = context.Response;
            HttpCachePolicy cache = response.Cache;
            response.ContentType = responseType.ToDescription();
            cache.VaryByHeaders["Accept-Encoding"] = true;

            if (futureExpire)
            {
                int maxCacheDays = CruncherConfiguration.Instance.MaxCacheDays;

                cache.SetExpires(DateTime.UtcNow.AddDays(maxCacheDays));
                cache.SetMaxAge(new TimeSpan(maxCacheDays, 0, 0, 0));
                response.AddFileDependencies(fileMonitors.ToArray());
                cache.SetLastModifiedFromFileDependencies();
                cache.SetValidUntilExpires(false);
            }
            else
            {
                cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                cache.SetMaxAge(new TimeSpan(0, 0, 0, 0));
            }

            cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            string etag = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", hash);
            string incomingEtag = context.Request.Headers["If-None-Match"];

            cache.SetETag(etag);
            cache.SetCacheability(HttpCacheability.Public);

            if (string.Compare(incomingEtag, etag, StringComparison.Ordinal) != 0)
            {
                return;
            }

            response.Clear();
            response.StatusCode = (int)HttpStatusCode.NotModified;
            response.SuppressContent = true;
        }

        /// <summary>
        /// Returns a Uri representing the url from the given token from the whitelist in the web.config.
        /// </summary>
        /// <param name="token">The token to look up.</param>
        /// <returns>A Uri representing the url from the given token from the whitelist in the web.config.</returns>
        protected Uri GetUrlFromToken(string token)
        {
            Uri url = null;
            CruncherSecuritySection.WhiteListElementCollection remoteFileWhiteList = CruncherConfiguration.Instance.RemoteFileWhiteList;
            CruncherSecuritySection.SafeUrl safeUrl = remoteFileWhiteList.Cast<CruncherSecuritySection.SafeUrl>()
                                                                         .FirstOrDefault(item => item.Token.ToUpperInvariant()
                                                                         .Equals(token.ToUpperInvariant()));

            if (safeUrl != null)
            {
                // Url encode any value here as we cannot store them encoded in the web.config.
                url = safeUrl.Url;
            }

            return url;
        }
    }
}
