#region Licence
// -----------------------------------------------------------------------
// <copyright file="CssHandler.cs" company="James South">
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
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using Cruncher.Compression;
    using Cruncher.Config;
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    using Cruncher.HttpModules;
    #endregion

    /// <summary>
    /// Concatinates and minifies css files before serving them to a page.
    /// </summary>
    public class CssHandler : HandlerBase
    {
        #region Fields
        /// <summary>
        /// The regular expression to search files for.
        /// </summary>
        private static readonly Regex ImportsRegex = new Regex(@"(?:@import\surl\()(?<filename>[^.]+\.css)(?:\);)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// The default path for css files on the server.
        /// </summary>
        private static readonly string[] CSSPaths = CruncherConfiguration.Instance.CSSPaths;

        /// <summary>
        /// Whether to minify css files on the server.
        /// </summary>
        private static readonly bool MinifyCSS = CruncherConfiguration.Instance.MinifyCSS;

        /// <summary>
        /// Whether to compress client resource files on the server.
        /// </summary>
        private static readonly bool CompressResources = CruncherConfiguration.Instance.CompressResources;

        /// <summary>
        /// The value used to replace the token '{root}' within a css file to determine the absolute root path for resources.
        /// </summary>
        private static readonly string RelativeCSSRoot = CruncherConfiguration.Instance.RelativeCSSRoot;

        /// <summary>
        /// A list of the fileCacheDependancies that will be monitored by the application.
        /// </summary>
        private readonly List<CacheDependency> cacheDependencies = new List<CacheDependency>();
        #endregion

        #region Properties
        #region IHttpHander Members
        /// <summary>
        ///     Gets a value indicating whether another request can use the <see cref = "T:System.Web.IHttpHandler"></see> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref = "T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
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
            string combinedCss = string.Empty;
            this.CombinedFilesCacheKey = context.Server.HtmlDecode(path);

            if (!string.IsNullOrWhiteSpace(path))
            {
                string[] cssNames = path.Split('|');
                StringBuilder stringBuilder = new StringBuilder();

                // Minification and caching only takes place if the correct values are set.
                bool minify = MinifyCSS;

                if (minify)
                {
                    // Try and pull it from the cache.
                    combinedCss = (string)context.Cache[this.CombinedFilesCacheKey];
                }

                // Check to see if the combined snippet exists. If not we process the list.
                if (string.IsNullOrWhiteSpace(combinedCss))
                {
                    Array.ForEach(
                        cssNames,
                        cssName =>
                        {
                            string cssSnippet = string.Empty;

                            if (string.IsNullOrWhiteSpace(cssSnippet))
                            {
                                // Anything without a path extension should be a token representing a remote file.
                                cssSnippet = StringComparer.OrdinalIgnoreCase.Compare(
                                    Path.GetExtension(cssName), ".css") != 0
                                                 ? this.RetrieveRemoteCss(cssName, CombinedFilesCacheKey, minify)
                                                 : this.RetrieveLocalCss(cssName, minify);
                            }

                            stringBuilder.Append(cssSnippet);
                        });

                    // Minify the css here as a whole.
                    combinedCss = this.ProcessCss(stringBuilder.ToString(), minify);
                }

                // Make sure css isn't empty
                if (!string.IsNullOrWhiteSpace(combinedCss))
                {
                    // Configure response headers
                    this.SetHeaders(combinedCss.GetHashCode(), context, ResponseType.Css, minify);
                    context.Response.Write(combinedCss);

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
        /// Call this method to do any post-processing on the css before its returned in the context response.
        /// </summary>
        /// <param name="css">The css stylesheet to process</param>
        /// <param name="shouldMinify">Whether the stylesheet should be minified.</param>
        /// <returns>The processed css stylesheet</returns>
        private string ProcessCss(string css, bool shouldMinify)
        {
            if (shouldMinify)
            {
                CssMinifier minifier = new CssMinifier
                {
                    ColorNamesRange = ColorNamesRange.W3CStrict
                };

                css = minifier.Minify(css);
            }

            css = css.Replace("{root}", RelativeCSSRoot);

            if (shouldMinify)
            {
                if (!string.IsNullOrWhiteSpace(css))
                {
                    if (this.cacheDependencies != null)
                    {
                        using (AggregateCacheDependency aggregate = new AggregateCacheDependency())
                        {
                            aggregate.Add(this.cacheDependencies.ToArray());

                            // Add the combined css to the cache.
                            HttpRuntime.Cache.Insert(
                                this.CombinedFilesCacheKey,
                                css,
                                aggregate,
                                Cache.NoAbsoluteExpiration,
                                new TimeSpan(MaxCacheDays, 0, 0, 0),
                                CacheItemPriority.High,
                                null);
                        }
                    }
                }
            }

            return css;
        }

        /// <summary>
        /// Retrieves and caches the specified remote CSS.
        /// </summary>
        /// <param name="file">The file name of the remote style sheet to retrieve.</param>
        /// <param name="cacheKey">The key used to insert this script into the cache.</param>
        /// <param name="minify">Whether or not the remote script should be minified.</param>
        /// <returns>
        /// The retrieved and processed remote css.
        /// </returns>
        private string RetrieveRemoteCss(string file, string cacheKey, bool minify)
        {
            Uri url;

            if (Uri.TryCreate(this.GetUrlFromToken(file), UriKind.Absolute, out url))
            {
                try
                {
                    RemoteFile remoteFile = new RemoteFile(url, false);
                    string css = remoteFile.GetFileAsString();

                    if (!string.IsNullOrWhiteSpace(css))
                    {
                        if (minify)
                        {
                            // Insert into cache
                            this.RemoteFileNotifier(cacheKey, css);
                        }
                    }

                    return css;
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
        /// Retrieves the local css style sheet from the disk
        /// </summary>
        /// <param name="file">The file name of the style sheet to retrieve.</param>
        /// <param name="minify">Whether or not the local script should be minified.</param>
        /// <returns>
        /// The retrieved and processed local style sheet.
        /// </returns>
        private string RetrieveLocalCss(string file, bool minify)
        {
            if (StringComparer.OrdinalIgnoreCase.Compare(Path.GetExtension(file), ".css") != 0 || file.StartsWith("http"))
            {
                throw new SecurityException("No access");
            }

            try
            {
                List<string> files = new List<string>();

                // Get the path from the server.
                // Loop through each possible directory.
                Array.ForEach(
                    CSSPaths,
                    cssFolder => Array.ForEach(
                        Directory.GetFiles(HttpContext.Current.Server.MapPath(cssFolder), file, SearchOption.AllDirectories),
                        files.Add));

                // We only want the first file.
                string path = files.FirstOrDefault();

                string css = string.Empty;

                if (path != null)
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        css = reader.ReadToEnd();
                    }

                    if (!string.IsNullOrWhiteSpace(css))
                    {
                        if (minify)
                        {
                            // Add the file to the cache dependancy list.
                            this.cacheDependencies.Add(new CacheDependency(path));
                        }

                        // Parse any import statements.
                        css = this.ParseImportsAndCache(css, minify);
                    }
                }

                return css;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Parses the string for css imports and adds them to the file dependency list.
        /// </summary>
        /// <param name="css">
        /// The css to parse for import statements.
        /// </param>
        /// <param name="minify">Whether or not the local script should be minified.</param>
        /// <returns>The css file parsed for imports.</returns>
        private string ParseImportsAndCache(string css, bool minify)
        {
            // Check for imports and parse if necessary.
            if (!css.Contains("@import", StringComparison.OrdinalIgnoreCase))
            {
                return css;
            }

            // Recursivly parse the css for imports.
            foreach (Match match in ImportsRegex.Matches(css))
            {
                // Recursivly parse the css for imports.
                GroupCollection groups = match.Groups;

                // Check and add the @import params to the cache dependancy list.
                foreach (var groupName in groups["filename"].Captures)
                {
                    // Get the match
                    List<string> files = new List<string>();
                    Array.ForEach(
                        CSSPaths,
                        cssPath => Array.ForEach(
                            Directory.GetFiles(
                                HttpContext.Current.Server.MapPath(cssPath),
                                groupName.ToString(),
                                SearchOption.AllDirectories),
                            files.Add));

                    string file = files.FirstOrDefault();
                    string thisCSS = string.Empty;

                    // Read the file.
                    if (file != null)
                    {
                        using (StreamReader reader = new StreamReader(file))
                        {
                            // Recursiveley parse the css.
                            thisCSS = this.ParseImportsAndCache(reader.ReadToEnd(), minify);
                        }
                    }

                    // Replace the regex match with the full qualified css.
                    css = css.Replace(match.Value, thisCSS);

                    if (minify)
                    {
                        this.cacheDependencies.Add(new CacheDependency(files.FirstOrDefault()));
                    }
                }
            }

            return css;
        }
        #endregion
        #endregion
    }
}
