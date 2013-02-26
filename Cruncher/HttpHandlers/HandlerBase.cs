#region Licence
// -----------------------------------------------------------------------
// <copyright file="HandlerBase.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.HttpHandlers
{
    #region Using
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Web;
    using System.Web.Caching;
    using Cruncher.Config;
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    using Cruncher.Preprocessors;
    #endregion

    /// <summary>
    /// Provides the base methods for synchronously processing Http Web requests.
    /// </summary>
    public abstract class HandlerBase : IHttpHandler
    {
        #region Fields
        /// <summary>
        /// The maximum number of days to store the resource in the browser cache.
        /// </summary>
        protected static readonly int MaxCacheDays = CruncherConfiguration.Instance.MaxCacheDays;

        /// <summary>
        /// The white-list of urls from which to download remote files.
        /// </summary>
        private static readonly CruncherSecuritySection.WhiteListElementCollection RemoteFileWhiteList = CruncherConfiguration.Instance.RemoteFileWhiteList;

        /// <summary>
        /// Whether the system is adding a file notifier.
        /// </summary>
        private bool addingNotifier;
        #endregion

        #region Properties
        #region IHttpHander Members
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref = "T:System.Web.IHttpHandler"></see> instance.
        /// </summary>
        /// <returns>true if the <see cref = "T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
        public abstract bool IsReusable
        {
            get;
        }
        #endregion

        /// <summary>
        /// Gets or sets a key for storing the combined processed script in the context cache.
        /// </summary>
        /// <returns>
        /// A key for storing the combined processed script in the context cache.
        /// </returns>
        protected abstract string CombinedFilesCacheKey { get; set; }
        #endregion

        #region Methods
        #region Public
        #region IHttpHander Members
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom 
        /// HttpHandler that implements the <see cref="T:System.Web.IHttpHandler">IHttpHandler</see> interface.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides 
        /// references to the intrinsic server objects 
        /// <example>Request, Response, Session, and Server</example> used to service HTTP requests.
        /// </param>
        public abstract void ProcessRequest(HttpContext context);
        #endregion
        #endregion

        #region Protected
        /// <summary>
        /// Transforms the content of the given string using the correct Preprocessor. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>The transformed string.</returns>
        protected virtual string PreProcessInput(string input, string path)
        {

            string extension = path.Substring(path.LastIndexOf('.')).ToUpperInvariant();

            input = CruncherConfiguration.Instance.Preprocessors
                .Where(Preprocessor => extension.Equals(Preprocessor.AllowedExtension, StringComparison.OrdinalIgnoreCase))
                .Aggregate(input, (current, Preprocessor) => Preprocessor.Transform(current, path));

            return input;

        }

        /// <summary>
        /// Retrieves the local file from the disk
        /// </summary>
        /// <param name="file">The file name of the file to retrieve.</param>
        /// <param name="minify">Whether or not the local file should be minified</param>
        /// <returns>
        /// The retrieved and processed local file contents.
        /// </returns>
        protected abstract string RetrieveLocalFile(string file, bool minify);

        /// <summary>
        /// Retrieves and caches the specified remote file.
        /// </summary>
        /// <param name="token">The token representing the path of the remote file to retrieve.</param>
        /// <param name="minify">Whether or not the remote file should be minified.</param>
        /// <returns>
        /// The retrieved and processed remote file as a string.
        /// </returns>
        protected string RetrieveRemoteFile(string token, bool minify)
        {
            Uri url = this.GetUrlFromToken(token);
            string contents = string.Empty;

            if (url != null)
            {
                try
                {
                    // Try and pull it from the cache.
                    if (minify)
                    {
                        contents = (string)HttpRuntime.Cache[token];
                    }

                    if (string.IsNullOrWhiteSpace(contents))
                    {
                        RemoteFile remoteFile = new RemoteFile(url, false);

                        // Return the preprocessed css.
                        contents = this.PreProcessInput(remoteFile.GetFileAsString(), url.ToString());
                    }

                    if (!string.IsNullOrWhiteSpace(contents))
                    {
                        if (minify)
                        {
                            // Insert into cache
                            this.RemoteFileNotifier(token, contents);
                        }
                    }

                    return contents;
                }
                catch (SocketException)
                {
                    // A SocketException is thrown by the Socket and Dns classes when an error occurs with 
                    // the network.
                    // The remote site is currently down. Try again next time.
                    return string.Empty;
                }
                catch
                {
                    throw;
                }
            }

            return contents;
        }


        /// <summary>
        /// This will make the browser and server keep the output
        /// in its cache and thereby improve performance.
        /// See http://en.wikipedia.org/wiki/HTTP_ETag
        /// </summary>
        /// <param name="hash">The hash number to apply to the eTag.</param>
        /// <param name="context">
        /// the <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides 
        /// references to the intrinsic server objects 
        /// </param>
        /// <param name="responseType">The HTTP MIME type to to send.</param>
        /// <param name="futureExpire">Whether the response headers should be set to expire in the future.</param>
        protected void SetHeaders(int hash, HttpContext context, ResponseType responseType, bool futureExpire)
        {
            HttpResponse response = context.Response;

            response.ContentType = responseType.ToDescription();

            HttpCachePolicy cache = response.Cache;

            cache.VaryByHeaders["Accept-Encoding"] = true;

            if (futureExpire)
            {
                cache.SetExpires(DateTime.Now.ToUniversalTime().AddDays(MaxCacheDays));
                cache.SetMaxAge(new TimeSpan(MaxCacheDays, 0, 0, 0));
            }
            else
            {
                cache.SetExpires(DateTime.Now.ToUniversalTime());
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

            CruncherSecuritySection.SafeUrl safeUrl = RemoteFileWhiteList.Cast<CruncherSecuritySection.SafeUrl>()
                                             .FirstOrDefault(item => item.Token.ToUpperInvariant()
                                             .Equals(token.ToUpperInvariant()));

            if (safeUrl != null)
            {
                // Url encode any value here as we cannot store them encoded in the web.config.
                url = safeUrl.Url;
            }

            return url;
        }

        /// <summary>
        /// Adds a remote file to the cache. If any of these files are removed from the cache then the combined 
        /// file is also removed from the cache.
        /// </summary>
        /// <param name="key">The key that is added to the cache.</param>
        /// <param name="value">The <see cref="T:System.Object"/>item associated with the key added to the cache.</param>
        protected void RemoteFileNotifier(string key, object value)
        {
            this.addingNotifier = true;

            HttpRuntime.Cache.Insert(
                key,
                value,
                null,
                Cache.NoAbsoluteExpiration,
                new TimeSpan(MaxCacheDays, 0, 0, 0),
                CacheItemPriority.High,
                this.OnFileChanged);

            this.addingNotifier = false;
        }

        /// <summary>
        /// Defines a callback method for notifying applications when a cached item is removed from the <see cref="T:System.Web.Caching.Cache"/>.
        /// </summary>
        /// <param name="key">The key that is removed from the cache.</param>
        /// <param name="value">The <see cref="T:System.Object"/>item associated with the key removed from the cache.</param>
        /// <param name="reason">The reason the item was removed from the cache, as specified by the <see cref="T:System.Web.Caching.CacheItemRemovedReason"/> enumeration.</param>
        protected void OnFileChanged(string key, object value, CacheItemRemovedReason reason)
        {
            if (!this.addingNotifier)
            {
                this.ClearCachedResources(key);
            }
        }

        /// <summary>
        /// Clears the cached resources that match the given parameters. 
        /// </summary>
        /// <param name="key">The key that is removed from the cache.</param>
        protected void ClearCachedResources(string key)
        {
            List<string> itemsToRemove = new List<string>();
            Cache cache = HttpRuntime.Cache;

            IDictionaryEnumerator enumerator = cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string cacheKey = enumerator.Key.ToString();
                if (cacheKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    itemsToRemove.Add(cacheKey);
                }
            }

            foreach (string item in itemsToRemove)
            {
                cache.Remove(item);
            }
        }
        #endregion
        #endregion
    }
}
