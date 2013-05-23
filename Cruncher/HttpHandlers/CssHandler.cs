#region Licence
// -----------------------------------------------------------------------
// <copyright file="CssHandler.cs" company="James South">
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
    using System.Globalization;
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
    using Cruncher.Extensions;
    using Cruncher.Helpers;
    using Cruncher.HttpModules;
    #endregion

    /// <summary>
    /// Concatenates and minifies CSS files before serving them to a page.
    /// </summary>
    public class CssHandler : HandlerBase
    {
        #region Fields
        /// <summary>
        /// The regular expression to search files for.
        /// </summary>
        private static readonly Regex ImportsRegex = new Regex(@"(?:@import\s*(url\(|\""))(?<filename>[^.]+(\.css|\.less))(?:(\)|\"")((?<media>[^;]+);|;))", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// The default path for CSS files on the server.
        /// </summary>
        private static readonly IList<string> CSSPaths = CruncherConfiguration.Instance.CSSPaths;

        /// <summary>
        /// Whether to minify CSS files on the server.
        /// </summary>
        private static readonly bool MinifyCSS = CruncherConfiguration.Instance.MinifyCSS;

        /// <summary>
        /// Whether to compress client resource files on the server.
        /// </summary>
        private static readonly bool CompressResources = CruncherConfiguration.Instance.CompressResources;

        /// <summary>
        /// A list of the fileCache dependencies that will be monitored by the application.
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
            this.CombinedFilesCacheKey = HttpUtility.HtmlDecode(path);

            if (!string.IsNullOrWhiteSpace(path))
            {
                string[] cssNames = path.Split('|');
                StringBuilder stringBuilder = new StringBuilder();

                // Minification and caching only takes place if the correct values are set.
                bool minify = MinifyCSS;

                if (minify)
                {
                    // Try and pull it from the cache.
                    combinedCss = (string)HttpRuntime.Cache[this.CombinedFilesCacheKey];
                }

                // Check to see if the combined snippet exists. If not we process the list.
                if (string.IsNullOrWhiteSpace(combinedCss))
                {
                    Array.ForEach(
                        cssNames,
                        cssName =>
                        {
                            // Anything without a path extension should be a token representing a remote file.
                            string cssSnippet = !CruncherConfiguration.Instance.AllowedExtensionsRegex.IsMatch(cssName)
                                                ? this.RetrieveRemoteFile(cssName, minify)
                                                : this.RetrieveLocalFile(cssName, minify);

                            // Run the snippet through the Preprocessor and append.
                            stringBuilder.Append(cssSnippet);
                        });

                    // Process and minify the css here as a whole.
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.Status = HttpStatusCode.NotFound.ToString();
                }
            }
        }
        #endregion
        #endregion

        #region Protected
        /// <summary>
        /// Transforms the content of the given string using the correct PreProcessor. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>The transformed string.</returns>
        protected override string PreProcessInput(string input, string path)
        {
            // Do the base processing then process any specific code here. 
            input = base.PreProcessInput(input, path);

            // Run the last filter. This should be the resourcePreprocessor.
            input = CruncherConfiguration.Instance.PreProcessors
                .First(p => string.IsNullOrWhiteSpace(p.AllowedExtension))
                .Transform(input, path);

            return input;
        }

        /// <summary>
        /// Retrieves the local CSS style sheet from the disk
        /// </summary>
        /// <param name="file">The file name of the style sheet to retrieve.</param>
        /// <param name="minify">Whether or not the local script should be minified.</param>
        /// <returns>
        /// The retrieved and processed local style sheet.
        /// </returns>
        protected override string RetrieveLocalFile(string file, bool minify)
        {
            if (!CruncherConfiguration.Instance.AllowedExtensionsRegex.IsMatch(Path.GetExtension(file)))
            {
                throw new SecurityException("No access");
            }

            try
            {
                // Get the path from the server.
                // Loop through each possible directory.
                List<string> files = CSSPaths
                    .SelectMany(cssFolder => Directory.GetFiles(
                        HttpContext.Current.Server.MapPath(cssFolder),
                        file,
                        SearchOption.AllDirectories))
                    .ToList();

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

                        // Run any filters 
                        css = this.PreProcessInput(css, path);

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
        #endregion

        #region Private
        /// <summary>
        /// Call this method to do any post-processing on the CSS before its returned in the context response.
        /// </summary>
        /// <param name="css">The CSS style sheet to process</param>
        /// <param name="shouldMinify">Whether the style sheet should be minified.</param>
        /// <returns>The processed CSS style sheet</returns>
        private string ProcessCss(string css, bool shouldMinify)
        {
            if (shouldMinify)
            {
                CssMinifier minifier = new CssMinifier
                {
                    ColorNamesRange = ColorNamesRange.W3CStrict
                };

                css = minifier.Minify(css);

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
        /// Parses the string for CSS imports and adds them to the file dependency list.
        /// </summary>
        /// <param name="css">
        /// The CSS to parse for import statements.
        /// </param>
        /// <param name="minify">Whether or not the local script should be minified.</param>
        /// <returns>The CSS file parsed for imports.</returns>
        private string ParseImportsAndCache(string css, bool minify)
        {
            // Check for imports and parse if necessary.
            if (!css.Contains("@import", StringComparison.OrdinalIgnoreCase))
            {
                return css;
            }

            // Recursively parse the css for imports.
            foreach (Match match in ImportsRegex.Matches(css))
            {
                // Recursively parse the css for imports.
                GroupCollection groups = match.Groups;
                Capture fileName = groups["filename"].Captures[0];
                CaptureCollection mediaQueries = groups["media"].Captures;
                Capture mediaQuery = null;

                if (mediaQueries.Count > 0)
                {
                    mediaQuery = mediaQueries[0];
                }

                // Check and add the @import params to the cache dependency list.
                // Get the match
                List<string> files = CSSPaths
                    .SelectMany(cssPath => Directory.GetFiles(
                        HttpContext.Current.Server.MapPath(cssPath),
                        Path.GetFileName(fileName.ToString()),
                        SearchOption.AllDirectories))
                    .ToList();

                string file = files.FirstOrDefault();
                string thisCSS = string.Empty;

                // Read the file.
                if (file != null)
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        thisCSS = mediaQuery != null
                                      ? string.Format(
                                          CultureInfo.InvariantCulture,
                                          "@media {0}{{{1}{2}{1}}}",
                                          mediaQuery,
                                          Environment.NewLine,
                                          this.ParseImportsAndCache(
                                              this.PreProcessInput(reader.ReadToEnd(), file), minify))
                                      : this.ParseImportsAndCache(this.PreProcessInput(reader.ReadToEnd(), file), minify);
                    }
                }

                // Replace the regex match with the full qualified css.
                css = css.Replace(match.Value, thisCSS);

                if (minify)
                {
                    this.cacheDependencies.Add(new CacheDependency(files.FirstOrDefault()));
                }
            }

            return css;
        }
        #endregion
        #endregion
    }
}
