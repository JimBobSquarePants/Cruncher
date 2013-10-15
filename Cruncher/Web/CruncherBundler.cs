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
    #endregion

    /// <summary>
    /// Provides methods for rendering CSS and JavaScript links onto a webpage.
    /// </summary>
    public static class CruncherBundler
    {
        /// <summary>
        /// The template for generating css links.
        /// </summary>
        private const string CssTemplate = "<link rel=\"stylesheet\" href=\"/css.axd?path={0}{1}\" {2}>";

        /// <summary>
        /// The JavaScript prefix.
        /// </summary>
        private const string JavaScriptTemplate = "<script type=\"text/javascript\" src=\"/js.axd?path={0}{1}\"></script>";

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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderCSS(bool forceUnMinify, HtmlString mediaQuery, params string[] fileNames)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string minify = forceUnMinify ? "&minify=false" : string.Empty;

            foreach (string fileName in fileNames)
            {
                stringBuilder.AppendFormat("{0}|", fileName);
            }

            return new HtmlString(string.Format(CssTemplate, stringBuilder.ToString().TrimEnd('|'), minify, mediaQuery));
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
            StringBuilder stringBuilder = new StringBuilder();
            string minify = forceUnMinify ? "&minify=false" : string.Empty;

            foreach (string fileName in fileNames)
            {
                stringBuilder.AppendFormat("{0}|", fileName);
            }

            return new HtmlString(string.Format(JavaScriptTemplate, stringBuilder.ToString().TrimEnd('|'), minify));
        } 
        #endregion
    }
}
