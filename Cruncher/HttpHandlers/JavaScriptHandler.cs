#region Licence
// -----------------------------------------------------------------------
// <copyright file="JavaScriptHandler.cs" company="James South">
//     Copyright (c) 2012,  James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.HttpHandlers
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Security;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using Cruncher.Compression;
    using Cruncher.Config;
    using Cruncher.Helpers;
    using Cruncher.HttpModules;
    #endregion

    /// <summary>
    /// Concatinates and minifies javascript files before serving them to a page.
    /// </summary>
    public class JavaScriptHandler : HandlerBase
    {
        #region Fields
        /// <summary>
        /// The default path for javascript files on the server.
        /// </summary>
        private static readonly string[] JavaScriptPaths = CruncherConfiguration.Instance.JavaScriptPaths;

        /// <summary>
        /// Whether to minify javascript files on the server.
        /// </summary>
        private static readonly bool MinifyJavaScript = CruncherConfiguration.Instance.MinifyJavaScript;

        /// <summary>
        /// Whether to compress client resource files on the server.
        /// </summary>
        private static readonly bool CompressResources = CruncherConfiguration.Instance.CompressResources;

        /// <summary>
        /// A list of the fileCacheDependancies that will be monitored by the application.
        /// </summary>
        private readonly List<CacheDependency> cacheDependencies = new List<CacheDependency>();
        #endregion

        #region Properties
        #region IHttpHander Members
        /// <summary>
        ///     Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        public override bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets a key for storing the combined processed files in the context cache.
        /// </summary>
        /// <returns>
        /// A key for storing the combined processed script in the context cache.
        /// </returns>
        protected override string CombinedFilesCacheKey { get; set; }
        #endregion

        #region Methods
        #region Public
        #region IHttpHandler Members
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom 
        /// HttpHandler that implements the <see cref="T:System.Web.IHttpHandler">IHttpHandler</see> interface.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides 
        /// references to the intrinsic server objects 
        /// <example>Request, Response, Session, and Server</example> used to service HTTP requests.
        /// </param>
        public override void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;

            string path = request.QueryString["path"];
            string combinedJavaScript = string.Empty;
            this.CombinedFilesCacheKey = context.Server.HtmlDecode(path);

            if (!string.IsNullOrWhiteSpace(path))
            {
                string[] javaScriptNames = path.Split('|');
                StringBuilder stringBuilder = new StringBuilder();

                // Minification and caching only takes place if the correct values are set.
                bool minify = MinifyJavaScript;

                if (minify)
                {
                    // Try and pull it from the cache.
                    combinedJavaScript = (string)context.Cache[this.CombinedFilesCacheKey];
                }

                // Check to see if the combined snippet exists. If not we process the list.
                if (string.IsNullOrWhiteSpace(combinedJavaScript))
                {
                    Array.ForEach(
                        javaScriptNames,
                        jsName =>
                        {
                            string jsSnippet = string.Empty;

                            if (string.IsNullOrWhiteSpace(jsSnippet))
                            {
                                // Anything without a path extension should be a token representing a remote file.
                                jsSnippet = StringComparer.OrdinalIgnoreCase.Compare(
                                    Path.GetExtension(jsName), ".js") != 0
                                                 ? this.RetrieveRemoteJavaScript(jsName, CombinedFilesCacheKey, minify)
                                                 : this.RetrieveLocalJavaScript(jsName, minify);
                            }

                            stringBuilder.Append(jsSnippet);
                        });

                    // Minify the js here as a whole.
                    combinedJavaScript = this.ProcessJavaScript(stringBuilder.ToString(), minify);
                }

                // Make sure js isn't empty
                if (!string.IsNullOrWhiteSpace(combinedJavaScript))
                {
                    // Configure response headers
                    this.SetHeaders(combinedJavaScript.GetHashCode(), context, ResponseType.JavaScript, minify);
                    context.Response.Write(combinedJavaScript);

                    // Compress the response if applicable.
                    if (CompressResources)
                    {
                        CompressionModule.CompressResponse(context);
                    }
                }
                else
                {
                    context.Response.Status = "404 Not Found";
                }
            }
        }
        #endregion

        #endregion
        #region Private
        /// <summary>
        /// Processes the script resource before being written to the response.
        /// </summary>
        /// <param name="script">The JavaScript resource to process.</param>
        /// <param name="shouldMinify">Whether the script should be minified.</param>
        /// <param name="terminate">Whether the script should be terminated with a semicolon.</param>
        /// <returns>The processed script resource.</returns>
        private string ProcessJavaScript(string script, bool shouldMinify, bool terminate = false)
        {
            JavaScriptMinifier minifier;

            if (shouldMinify)
            {
                minifier = new JavaScriptMinifier
                {
                    VariableMinification = VariableMinification.LocalVariablesOnly,
                    TermSemiColons = terminate
                };
            }
            else
            {
                minifier = new JavaScriptMinifier
                {
                    VariableMinification = VariableMinification.None,
                    PreserveFunctionNames = true,
                    RemoveWhiteSpace = false,
                    TermSemiColons = terminate
                };
            }

            script = minifier.Minify(script);

            if (shouldMinify)
            {
                if (!string.IsNullOrWhiteSpace(script))
                {
                    if (this.cacheDependencies != null)
                    {
                        using (AggregateCacheDependency aggregate = new AggregateCacheDependency())
                        {
                            aggregate.Add(this.cacheDependencies.ToArray());

                            // Add the combined script to the cache.
                            HttpRuntime.Cache.Insert(
                                this.CombinedFilesCacheKey,
                                script,
                                aggregate,
                                Cache.NoAbsoluteExpiration,
                                new TimeSpan(MaxCacheDays, 0, 0, 0),
                                CacheItemPriority.High,
                                null);
                        }
                    }
                }
            }

            return script;
        }

        /// <summary>
        /// Retrieves and caches the specified remote JavaScript.
        /// </summary>
        /// <param name="file">The file name of the remote script to retrieve.</param>
        /// <param name="cacheKey">The key used to insert this script into the cache.</param>
        /// <param name="minify">Whether or not the remote script should be minified.</param>
        /// <returns>
        /// The retrieved and processed remote javascript.
        /// </returns>
        private string RetrieveRemoteJavaScript(string file, string cacheKey, bool minify)
        {
            Uri url;

            if (Uri.TryCreate(this.GetUrlFromToken(file), UriKind.Absolute, out url))
            {
                try
                {
                    RemoteFile remoteFile = new RemoteFile(url, false);
                    string script = remoteFile.GetFileAsString();

                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        if (minify)
                        {
                            // Insert into cache
                            this.RemoteFileNotifier(cacheKey, script);
                        }
                    }

                    return script;
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

            return string.Empty;
        }

        /// <summary>
        /// Retrieves the local script from the disk
        /// </summary>
        /// <param name="file">The file name of the script to retrieve.</param>
        /// <param name="minify">Whether or not the local script should be minified</param>
        /// <returns>
        /// The retrieved and processed local script.
        /// </returns>
        private string RetrieveLocalJavaScript(string file, bool minify)
        {
            if (StringComparer.OrdinalIgnoreCase.Compare(Path.GetExtension(file), ".js") != 0 || file.StartsWith("http"))
            {
                throw new SecurityException("No access");
            }

            try
            {
                List<string> files = new List<string>();

                // Get the path from the server.
                // Loop through each possible directory.
                Array.ForEach(
                    JavaScriptPaths,
                    scriptFolder => Array.ForEach(
                        Directory.GetFiles(HttpContext.Current.Server.MapPath(scriptFolder), file, SearchOption.AllDirectories),
                        files.Add));

                // We only want the first file.
                string path = files.FirstOrDefault();

                string script = string.Empty;

                if (path != null)
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        script = reader.ReadToEnd();
                    }

                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        if (minify)
                        {
                            // Add the file to the cache dependancy list.
                            this.cacheDependencies.Add(new CacheDependency(path));
                        }
                    }
                }

                return script;
            }
            catch
            {
                throw;
            }
        }
        #endregion
        #endregion
    }
}
