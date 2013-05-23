#region Licence
// -----------------------------------------------------------------------
// <copyright file="JavaScriptHandler.cs" company="James South">
//     Copyright (c) James South.
//     Licensed under the Apache License, Version 2.0.
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
    using System.Net;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using Cruncher.Compression;
    using Cruncher.Config;
    using Cruncher.Helpers;
    using Cruncher.HttpModules;
    #endregion

    /// <summary>
    /// Concatenates and minifies JavaScript files before serving them to a page.
    /// </summary>
    public class JavaScriptHandler : HandlerBase
    {
        #region Fields
        /// <summary>
        /// The regular expression for matching file type.
        /// </summary>
        private static readonly Regex ExtensionsRegex = new Regex(@"\.js", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// The default path for JavaScript files on the server.
        /// </summary>
        private static readonly IList<string> JavaScriptPaths = CruncherConfiguration.Instance.JavaScriptPaths;

        /// <summary>
        /// Whether to minify JavaScript files on the server.
        /// </summary>
        private static readonly bool MinifyJavaScript = CruncherConfiguration.Instance.MinifyJavaScript;

        /// <summary>
        /// Whether to compress client resource files on the server.
        /// </summary>
        private static readonly bool CompressResources = CruncherConfiguration.Instance.CompressResources;

        /// <summary>
        /// A list of the fileCacheDependencies that will be monitored by the application.
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
            this.CombinedFilesCacheKey = HttpUtility.HtmlDecode(path);

            if (!string.IsNullOrWhiteSpace(path))
            {
                string[] javaScriptNames = path.Split('|');
                StringBuilder stringBuilder = new StringBuilder();

                // Minification and caching only takes place if the correct values are set.
                bool minify = MinifyJavaScript;

                if (minify)
                {
                    // Try and pull it from the cache.
                    combinedJavaScript = (string)HttpRuntime.Cache[this.CombinedFilesCacheKey];
                }

                // Check to see if the combined snippet exists. If not we process the list.
                if (string.IsNullOrWhiteSpace(combinedJavaScript))
                {
                    Array.ForEach(
                        javaScriptNames,
                        jsName =>
                        {
                            // Anything without a path extension should be a token representing a remote file.
                            string jsSnippet = !ExtensionsRegex.IsMatch(jsName)
                                ? this.RetrieveRemoteFile(jsName, minify)
                                : this.RetrieveLocalFile(jsName, minify);

                            // Run the snippet through the Preprocessor and append.
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.Status = HttpStatusCode.NotFound.ToString();
                }
            }
        }
        #endregion
        #endregion

        #region Protected
        /// <summary>
        /// Transforms the content of the given string using the correct Pre-processor. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>The transformed string.</returns>
        protected override string PreProcessInput(string input, string path)
        {
            // Do the base processing then process any specific code here. 
            input = base.PreProcessInput(input, path);

            return input;
        }

        /// <summary>
        /// Retrieves the local script from the disk
        /// </summary>
        /// <param name="file">The file name of the script to retrieve.</param>
        /// <param name="minify">Whether or not the local script should be minified</param>
        /// <returns>
        /// The retrieved and processed local script.
        /// </returns>
        protected override string RetrieveLocalFile(string file, bool minify)
        {
            if (!ExtensionsRegex.IsMatch(Path.GetExtension(file)))
            {
                throw new SecurityException("No access");
            }

            try
            {
                // Get the path from the server.
                // Loop through each possible directory.
                List<string> files = JavaScriptPaths
                    .SelectMany(scriptFolder => Directory.GetFiles(
                        HttpContext.Current.Server.MapPath(scriptFolder), 
                        file, 
                        SearchOption.AllDirectories))
                    .ToList();

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
                            // Add the file to the cache dependency list.
                            this.cacheDependencies.Add(new CacheDependency(path));
                        }

                        // Run any filters 
                        script = this.PreProcessInput(script, path);
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
        #endregion
        #endregion
    }
}
