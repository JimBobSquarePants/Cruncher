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
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Text.RegularExpressions;
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

                // TODO: Do we need this anymore??
                //if (PeachConfiguration.CompressWebResource)
                //{
                //    context.Response.Filter = new WebResourceFilter(context.Response.Filter);
                //    WillCompressResponse = true;
                //}
            }
            //else if (!PeachConfiguration.CompressWebResource && context.Request.Path.Contains("WebResource.axd"))
            //{
            //    context.Response.Cache.SetExpires(DateTime.Now.AddDays(30));
            //}
        }

        #region Static

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.EventArgs">EventArgs</see> that contains the event data.</param>
        private void ContextError(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;

            // If this CompressionModule will be compressing the response and an unhandled exception
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

        /// <summary>
        /// The web resource filter.
        /// </summary>
        private class WebResourceFilter : Stream
        {
            #region Constants and Fields

            /// <summary>
            /// The _sink.
            /// </summary>
            private readonly Stream sink;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="WebResourceFilter"/> class.
            /// </summary>
            /// <param name="sink">
            /// The sink stream.
            /// </param>
            public WebResourceFilter(Stream sink)
            {
                this.sink = sink;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets a value indicating whether CanRead.
            /// </summary>
            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Gets a value indicating whether CanSeek.
            /// </summary>
            public override bool CanSeek
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Gets a value indicating whether CanWrite.
            /// </summary>
            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Gets Length.
            /// </summary>
            public override long Length
            {
                get
                {
                    return 0;
                }
            }

            /// <summary>
            /// Gets or sets Position.
            /// </summary>
            public override long Position { get; set; }

            #endregion

            #region Public Methods

            /// <summary>
            /// The close.
            /// </summary>
            public override void Close()
            {
                this.sink.Close();
            }

            /// <summary>
            /// Evaluates the replacement for each link match.
            /// </summary>
            /// <param name="match">
            /// The match.
            /// </param>
            /// <returns>
            /// The evaluator.
            /// </returns>
            public string Evaluator(Match match)
            {
                string relative = match.Groups[1].Value;
                string absolute = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                // TODO: DO we need this?
                string root = VirtualPathUtility.ToAbsolute("~/");

                //string root = VirtualPathUtility.ToAbsolute(PeachConfiguration.VirtualPath);
                string replaced = string.Format("{0}js.axd?path={1}", root, HttpUtility.UrlEncode(absolute + relative));

                return match.Value.Replace(relative, replaced);
            }

            /// <summary>
            /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data 
            /// to be written to the underlying device.
            /// </summary>
            /// <exception cref="T:System.IO.IOException">
            /// An I/O error occurs.
            /// </exception>
            public override void Flush()
            {
                this.sink.Flush();
            }

            /// <summary>
            /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances
            /// the position within the stream by the number of bytes read.
            /// </summary>
            /// <param name="buffer">
            /// An array of bytes. When this method returns, the buffer contains the specified byte array with the values
            /// between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced 
            /// by the bytes read from the current source.
            /// </param>
            /// <param name="offset">
            /// The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the 
            /// current stream.
            /// </param>
            /// <param name="count">
            /// The maximum number of bytes to be read from the current stream.
            /// </param>
            /// <returns>
            /// The total number of bytes read into the buffer. This can be less than the number of bytes requested 
            /// if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
            /// </returns>
            /// <exception cref="T:System.ArgumentException">
            /// The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
            /// </exception>
            /// <exception cref="T:System.ArgumentNullException">
            ///     <paramref name="buffer"/> is null.
            /// </exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            ///     <paramref name="offset"/> or <paramref name="count"/> is negative.
            /// </exception>
            /// <exception cref="T:System.IO.IOException">
            /// An I/O error occurs.
            /// </exception>
            /// <exception cref="T:System.NotSupportedException">
            /// The stream does not support reading.
            /// </exception>
            /// <exception cref="T:System.ObjectDisposedException">
            /// Methods were called after the stream was closed.
            /// </exception>
            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.sink.Read(buffer, offset, count);
            }

            /// <summary>
            /// When overridden in a derived class, sets the position within the current stream.
            /// </summary>
            /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
            /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
            /// <returns>
            /// The new position within the current stream.
            /// </returns>
            /// <exception cref="T:System.IO.IOException">
            /// An I/O error occurs.
            /// </exception>
            /// <exception cref="T:System.NotSupportedException">
            /// The stream does not support seeking, such as if the stream is constructed from a pipe or console output.
            /// </exception>
            /// <exception cref="T:System.ObjectDisposedException">
            /// Methods were called after the stream was closed.
            /// </exception>
            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.sink.Seek(offset, origin);
            }

            /// <summary>
            /// When overridden in a derived class, sets the length of the current stream.
            /// </summary>
            /// <param name="value">The desired length of the current stream in bytes.</param>
            /// <exception cref="T:System.IO.IOException">
            /// An I/O error occurs.
            /// </exception>
            /// <exception cref="T:System.NotSupportedException">
            /// The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output.
            /// </exception>
            /// <exception cref="T:System.ObjectDisposedException">
            /// Methods were called after the stream was closed.
            /// </exception>
            public override void SetLength(long value)
            {
                this.sink.SetLength(value);
            }

            /// <summary>
            /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
            /// </summary>
            /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
            /// <param name="count">The number of bytes to be written to the current stream.</param>
            /// <exception cref="T:System.ArgumentException">
            /// The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.
            /// </exception>
            /// <exception cref="T:System.ArgumentNullException">
            ///     <paramref name="buffer"/> is null.
            /// </exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            ///     <paramref name="offset"/> or <paramref name="count"/> is negative.
            /// </exception>
            /// <exception cref="T:System.IO.IOException">
            /// An I/O error occurs.
            /// </exception>
            /// <exception cref="T:System.NotSupportedException">
            /// The stream does not support writing.
            /// </exception>
            /// <exception cref="T:System.ObjectDisposedException">
            /// Methods were called after the stream was closed.
            /// </exception>
            public override void Write(byte[] buffer, int offset, int count)
            {
                var html = Encoding.UTF8.GetString(buffer);

                var regex =
                    new Regex(
                        "<script\\s*src=\"((?=[^\"]*webresource.axd)[^\"]*)\"\\s*type=\"text/javascript\"[^>]*>[^<]*(?:</script>)?",
                        RegexOptions.IgnoreCase);
                html = regex.Replace(html, this.Evaluator);

                var outdata = Encoding.UTF8.GetBytes(html);
                this.sink.Write(outdata, 0, outdata.GetLength(0));
            }

            #endregion
        }
    }
}
