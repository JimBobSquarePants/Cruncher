// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruncherBundler.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides methods for rendering CSS and JavaScript links onto a webpage.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Web
{
    #region Using
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;

    using Cruncher.Web.Configuration;

    #endregion

    /// <summary>
    /// Provides methods for rendering CSS and JavaScript links onto a webpage.
    /// </summary>
    public static class CruncherBundler
    {
        /// <summary>
        /// The template for generating css links.
        /// </summary>
        private const string CssTemplate = "<link rel=\"stylesheet\" href=\"/css.axd?path={0}{1}{2}\" {3}>";

        /// <summary>
        /// The template for generating css links.
        /// </summary>
        private const string CssDebugTemplate = "<link rel=\"stylesheet\" href=\"/css.axd?path={0}{1}\" {2}>";

        /// <summary>
        /// The template for generating JavaScript links.
        /// </summary>
        private const string JavaScriptTemplate = "<script type=\"text/javascript\" src=\"/js.axd?path={0}{1}{2}\"></script>";

        /// <summary>
        /// The template for generating JavaScript links.
        /// </summary>
        private const string JavaScriptDebugTemplate = "<script type=\"text/javascript\" src=\"/js.axd?path={0}{1}\"></script>";

        /// <summary>
        /// The CSS handler.
        /// </summary>
        private static readonly CssHandler CssHandler = new CssHandler();

        /// <summary>
        /// The JavaScript handler.
        /// </summary>
        private static readonly JavaScriptHandler JavaScriptHandler = new JavaScriptHandler();

        #region CSS
        /// <summary>
        /// Renders the correct html to create a stylesheet link to the crunched css representing the given files.
        /// </summary>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .css, .less, .sass, and .scss files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the stylesheet link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderCSS(params string[] fileNames)
        {
            return RenderCSS(false, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a stylesheet link to the crunched css representing the given files.
        /// </summary>
        /// <param name="forceUnMinify">
        /// Whether to force cruncher to output the crunched css in an unminified state.
        /// </param>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .css, .less, .sass, and .scss files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the stylesheet link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderCSS(bool forceUnMinify, params string[] fileNames)
        {
            return RenderCSS(forceUnMinify, null, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a stylesheet link to the crunched css representing the given files.
        /// </summary>
        /// <param name="mediaQuery">
        /// The media query to apply to the link. For reference see:
        /// <a href="https://developer.mozilla.org/en-US/docs/Web/Guide/CSS/Media_queries?redirectlocale=en-US&amp;redirectslug=CSS%2FMedia_queries"/>Media Queries<a/> 
        /// </param>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .css, .less, .sass, and .scss files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the stylesheet link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderCSS(HtmlString mediaQuery, params string[] fileNames)
        {
            return RenderCSS(false, mediaQuery, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a stylesheet link to the crunched css representing the given files.
        /// </summary>
        /// <param name="forceUnMinify">
        /// Whether to force cruncher to output the crunched css in an unminified state.
        /// </param>
        /// <param name="mediaQuery">
        /// The media query to apply to the link. For reference see:
        /// <a href="https://developer.mozilla.org/en-US/docs/Web/Guide/CSS/Media_queries?redirectlocale=en-US&amp;redirectslug=CSS%2FMedia_queries"/>Media Queries<a/> 
        /// </param>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .css, .less, .sass, and .scss files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the stylesheet link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderCSS(bool forceUnMinify, HtmlString mediaQuery, params string[] fileNames)
        {
            return RenderCSS(false, mediaQuery, true, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a stylesheet link to the crunched css representing the given files.
        /// </summary>
        /// <param name="forceUnMinify">
        /// Whether to force cruncher to output the crunched css in an unminified state.
        /// </param>
        /// <param name="mediaQuery">
        /// The media query to apply to the link. For reference see:
        /// <a href="https://developer.mozilla.org/en-US/docs/Web/Guide/CSS/Media_queries?redirectlocale=en-US&amp;redirectslug=CSS%2FMedia_queries"/>Media Queries<a/> 
        /// </param>
        /// <param name="versioning">
        /// If true it will automatically version the crunched css by adding a new querystring parameter v followed by the version number. 
        /// Each time that any css file is modified a new version number will be issued. Defaults to true
        /// </param>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .css, .less, .sass, and .scss files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the stylesheet link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderCSS(bool forceUnMinify, HtmlString mediaQuery, bool versioning = true, params string[] fileNames)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (CruncherConfiguration.Instance.MinifyCSS)
            {
                string minify = forceUnMinify ? "&minify=false" : string.Empty;

                foreach (string fileName in fileNames)
                {
                    stringBuilder.AppendFormat("{0}|", fileName);
                }

                string path = stringBuilder.ToString().TrimEnd('|');

                string version = string.Empty;
                if (versioning)
                {
                    int versionNumber = CssHandler.ProcessCssCrunch(path, !forceUnMinify).GetHashCode();
                    version = string.Format("&v={0}", versionNumber);
                }

                return new HtmlString(string.Format(CssTemplate, path, minify, version, mediaQuery));
            }

            // Render them separately for debug mode.
            foreach (string fileName in fileNames)
            {
                string version = string.Empty;
                if (versioning)
                {
                    int versionNumber = CssHandler.ProcessCssCrunch(fileName, false).GetHashCode();
                    version = string.Format("&v={0}", versionNumber);
                }

                stringBuilder.AppendFormat(CssDebugTemplate, fileName, version, mediaQuery);
                stringBuilder.AppendLine();
            }

            return new HtmlString(stringBuilder.ToString());
        }
        #endregion

        #region JavaScript
        /// <summary>
        /// Renders the correct html to create a script tag linking to the crunched JavaScript representing the given files.
        /// </summary>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .js, and .coffee files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the script tag with the correct link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderJavaScript(params string[] fileNames)
        {
            return RenderJavaScript(false, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a script tag linking to the crunched JavaScript representing the given files.
        /// </summary>
        /// <param name="forceUnMinify">
        /// Whether to force cruncher to output the crunched css in an unminified state.
        /// </param>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .js, and .coffee files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the script tag with the correct link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderJavaScript(bool forceUnMinify, params string[] fileNames)
        {
            return RenderJavaScript(forceUnMinify, true, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a script tag linking to the crunched JavaScript representing the given files.
        /// </summary>
        /// <param name="forceUnMinify">
        /// Whether to force cruncher to output the crunched css in an unminified state.
        /// </param>
        /// <param name="versioning">
        /// If true it will automatically version the crunched javascript by adding a new querystring parameter v followed by the version number. 
        /// Each time that any javascript file is modified a new version number will be issued. Defaults to true
        /// </param>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .js, and .coffee files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the script tag with the correct link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderJavaScript(bool forceUnMinify, bool versioning = true, params string[] fileNames)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (CruncherConfiguration.Instance.MinifyJavaScript)
            {
                string minify = forceUnMinify ? "&minify=false" : string.Empty;

                foreach (string fileName in fileNames)
                {
                    stringBuilder.AppendFormat("{0}|", fileName);
                }

                string path = stringBuilder.ToString().TrimEnd('|');

                string version = string.Empty;
                if (versioning)
                {
                    int versionNumber = JavaScriptHandler.ProcessJavascriptCrunch(path, !forceUnMinify).GetHashCode();
                    version = string.Format("&v={0}", versionNumber);
                }

                return new HtmlString(string.Format(JavaScriptTemplate, path, minify, version));
            }

            // Render them separately for debug mode.
            foreach (string fileName in fileNames)
            {
                string version = string.Empty;
                if (versioning)
                {
                    int versionNumber = JavaScriptHandler.ProcessJavascriptCrunch(fileName, false).GetHashCode();
                    version = string.Format("&v={0}", versionNumber);
                }

                stringBuilder.AppendFormat(JavaScriptDebugTemplate, fileName, version);
                stringBuilder.AppendLine();
            }

            return new HtmlString(stringBuilder.ToString());
        }
        #endregion
    }
}
