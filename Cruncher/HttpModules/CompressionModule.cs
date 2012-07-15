#region Licence
// -----------------------------------------------------------------------
// <copyright file="CompressionModule.cs" company="James South">
//     Copyright (c) 2012,  James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.HttpModules
{
    #region Using
    using System;
    using System.IO.Compression;
    using System.Web;
    using System.Web.UI;
    using Cruncher.Config;
    #endregion

    /// <summary>
    /// Compresses the output using standard gzip/deflate.
    /// </summary>
    public sealed class CompressionModule : IHttpModule
    {
        #region Fields
        /// <summary>
        /// The deflate string.
        /// </summary>
        private const string Deflate = "deflate";

        /// <summary>
        /// The gzip string.
        /// </summary>
        private const string Gzip = "gzip";

        /// <summary>
        /// Whether to compress client resource files on the server.
        /// </summary>
        private static readonly bool CompressResources = CruncherConfiguration.Instance.CompressResources;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether to compress the web response.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the web response should be compressed; otherwise, <see langword="false"/>.
        /// </value>
        private static bool WillCompressResponse
        {
            get
            {
                HttpContext context = HttpContext.Current;

                if (context == null)
                {
                    return false;
                }

                return context.Items["will-compress-resource"] != null && (bool)context.Items["will-compress-resource"];
            }

            set
            {
                HttpContext context = HttpContext.Current;

                if (context == null)
                {
                    return;
                }

                context.Items["will-compress-resource"] = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Compresses the response stream using either deflate or gzip depending on the client.
        /// </summary>
        /// <param name="context">
        /// The HTTP context to compress.
        /// </param>
        public static void CompressResponse(HttpContext context)
        {
            if (IsEncodingAccepted(Deflate))
            {
                context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
                WillCompressResponse = true;
                SetEncoding(Deflate);
            }
            else if (IsEncodingAccepted(Gzip))
            {
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                WillCompressResponse = true;
                SetEncoding(Gzip);
            }
        }
        #endregion

        #region IHttpModule Members
        /// <summary>
        /// Disposes of the resources (other than memory) used by the module 
        ///     that implements <see cref="T:System.Web.IHttpModule"></see>.
        /// </summary>
        void IHttpModule.Dispose()
        {
            // Nothing to dispose; 
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpApplication"></see> 
        ///     that provides access to the methods, properties, and events common to 
        ///     all application objects within an ASP.NET application.
        /// </param>
        void IHttpModule.Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += ContextPostReleaseRequestState;
            context.Error += this.ContextError;
        }
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Checks the request headers to see if the specified
        /// encoding is accepted by the client.
        /// </summary>
        /// <param name="encoding">
        /// The encoding.
        /// </param>
        /// <returns>
        /// The is encoding accepted.
        /// </returns>
        private static bool IsEncodingAccepted(string encoding)
        {
            var context = HttpContext.Current;
            return context.Request.Headers["Accept-encoding"] != null &&
                   context.Request.Headers["Accept-encoding"].Contains(encoding);
        }

        /// <summary>
        /// Adds the specified encoding to the response headers.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        private static void SetEncoding(string encoding)
        {
            HttpContext.Current.Response.AppendHeader("Content-encoding", encoding);
        }

        /// <summary>
        /// Handles the BeginRequest event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.EventArgs">EventArgs</see> that contains the event data.</param>
        private static void ContextPostReleaseRequestState(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;

            if (!CompressResources)
            {
                return;
            }

            if (context.CurrentHandler is Page && context.Request["HTTP_X_MICROSOFTAJAX"] == null &&
                context.Request.HttpMethod == "GET")
            {
                CompressResponse(context);
            }
        }

        #region Static

        /// <summary>
        /// Handles any unhandled exceptions that take place whilst compressing the response.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.EventArgs">EventArgs</see> that contains the event data.</param>
        private void ContextError(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;

            // If this CompressionModule is compressing the response and an unhandled exception
            // has occurred, remove the WebResourceFilter as that will cause garbage characters to
            // be sent to the browser instead of a yellow screen of death.
            if (WillCompressResponse)
            {
                context.Response.Filter = null;
                WillCompressResponse = false;
            }
        }
        #endregion
        #endregion
        #endregion
    }
}
