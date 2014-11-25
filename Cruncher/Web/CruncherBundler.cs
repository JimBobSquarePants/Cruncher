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
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;

    using Cruncher.Helpers;
    using Cruncher.Web.Configuration;

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
        /// The template for generating css links pointing to a physical file
        /// </summary>
        private const string CssTemplatePhysicalFile = "<link rel=\"stylesheet\" href=\"{0}\" {1}>";

        /// <summary>
        /// The template for generating JavaScript links.
        /// </summary>
        private const string JavaScriptTemplate = "<script type=\"text/javascript\" src=\"/js.axd?path={0}{1}{2}\"></script>";

        /// <summary>
        /// The template for generating JavaScript links pointing to a physical file
        /// </summary>
        private const string JavaScriptTemplatePhysicalFile = "<script type=\"text/javascript\" src=\"{0}\"></script>";

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
        /// Whether to force cruncher to output the crunched css in an un-minified state.
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
        /// Whether to force cruncher to output the crunched css in an un-minified state.
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
            return RenderCSS(false, mediaQuery, true, true, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a stylesheet link to the crunched css representing the given files.
        /// </summary>
        /// <param name="forceUnMinify">
        /// Whether to force cruncher to output the crunched css in an un-minified state.
        /// </param>
        /// <param name="mediaQuery">
        /// The media query to apply to the link. For reference see:
        /// <a href="https://developer.mozilla.org/en-US/docs/Web/Guide/CSS/Media_queries?redirectlocale=en-US&amp;redirectslug=CSS%2FMedia_queries"/>Media Queries<a/> 
        /// </param>
        /// <param name="createPhysicalFile">
        /// If true it will create a physical file for the crunched javascript
        /// NOTE: If this parameter is set to true, the value of the version parameter will be ignored and version will take place
        /// </param>
        /// <param name="version">
        /// If true it will automatically version the crunched css by adding a new querystring parameter v followed by the version number or creating a physical file depending on the value of the createPhysicalFile parameter.
        /// Each time that any css file is modified a new version number will be issued. Defaults to true
        /// NOTE: If the createPhysicalFile parameter is set to true, the value of this parameter will be ignored and version will take place
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
        public static HtmlString RenderCSS(bool forceUnMinify, HtmlString mediaQuery, bool createPhysicalFile = true, bool version = true, params string[] fileNames)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // When createPhysicalFile parameter if true, version is forced to true
            if (createPhysicalFile)
            {
                version = true;
            }

            if (CruncherConfiguration.Instance.MinifyCSS)
            {
                string minify = forceUnMinify ? "&minify=false" : string.Empty;

                foreach (string fileName in fileNames)
                {
                    stringBuilder.AppendFormat("{0}|", fileName);
                }

                string path = stringBuilder.ToString().TrimEnd('|');

                if (createPhysicalFile)
                {
                    string fileContent = CssHandler.ProcessCssCrunch(path, !forceUnMinify);
                    string fileName = string.Format("{0}.css", fileContent.GetHashCode());
                    return new HtmlString(string.Format(CssTemplatePhysicalFile, ResourceHelper.CreateResourcePhysicalFile(fileName, fileContent), mediaQuery));
                }

                string cssVersion = string.Empty;
                if (version)
                {
                    int versionNumber = CssHandler.ProcessCssCrunch(path, !forceUnMinify).GetHashCode();
                    cssVersion = string.Format("&v={0}", versionNumber);
                }

                return new HtmlString(string.Format(CssTemplate, path, minify, cssVersion, mediaQuery));
            }

            // Render them separately for debug mode.
            foreach (string name in fileNames)
            {
                if (createPhysicalFile)
                {
                    string fileContent = CssHandler.ProcessCssCrunch(name, false);
                    string fileName = string.Format("{0}.css", fileContent.GetHashCode());
                    stringBuilder.AppendFormat(CssTemplatePhysicalFile, fileName, mediaQuery);
                    stringBuilder.AppendLine();
                }
                else
                {
                    string cssVersion = string.Empty;
                    if (version)
                    {
                        int versionNumber = CssHandler.ProcessCssCrunch(name, false).GetHashCode();
                        cssVersion = string.Format("&v={0}", versionNumber);
                    }

                    stringBuilder.AppendFormat(CssTemplate, name, string.Empty, cssVersion, mediaQuery);
                    stringBuilder.AppendLine();
                }
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
        /// Whether to force cruncher to output the crunched css in an un-minified state.
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
            return RenderJavaScript(forceUnMinify, true, true, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a script tag linking to the crunched JavaScript representing the given files.
        /// </summary>
        /// <param name="forceUnMinify">
        /// Whether to force cruncher to output the crunched css in an un-minified state.
        /// </param>
        /// <param name="createPhysicalFile">
        /// If true it will create a physical file for the crunched javascript
        /// NOTE: If this parameter is set to true, the value of the version parameter will be ignored and version will take place
        /// </param>
        /// <param name="version">
        /// If true it will automatically version the crunched javascript by adding a new querystring parameter v followed by the version number or creating a physical file depending on the value of the createPhysicalFile parameter.
        /// Each time that any javascript file is modified a new version number will be issued. Defaults to true
        /// NOTE: If the createPhysicalFile parameter is set to true, the value of this parameter will be ignored and version will take place
        /// </param>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .js, and .coffee files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the script tag with the correct link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderJavaScript(bool forceUnMinify, bool createPhysicalFile = true, bool version = true, params string[] fileNames)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // When createPhysicalFile parameter if true, version is forced to true
            if (createPhysicalFile)
            {
                version = true;
            }

            if (CruncherConfiguration.Instance.MinifyJavaScript)
            {
                string minify = forceUnMinify ? "&minify=false" : string.Empty;

                foreach (string fileName in fileNames)
                {
                    stringBuilder.AppendFormat("{0}|", fileName);
                }

                string path = stringBuilder.ToString().TrimEnd('|');

                if (createPhysicalFile)
                {
                    string fileContent = JavaScriptHandler.ProcessJavascriptCrunch(path, !forceUnMinify);
                    string fileName = string.Format("{0}.js", fileContent.GetHashCode());
                    return new HtmlString(string.Format(JavaScriptTemplatePhysicalFile, ResourceHelper.CreateResourcePhysicalFile(fileName, fileContent)));
                }

                string javaScriptVersion = string.Empty;
                if (version)
                {
                    int versionNumber = JavaScriptHandler.ProcessJavascriptCrunch(path, !forceUnMinify).GetHashCode();
                    javaScriptVersion = string.Format("&v={0}", versionNumber);
                }

                return new HtmlString(string.Format(JavaScriptTemplate, path, minify, javaScriptVersion));
            }

            // Render them separately for debug mode.
            foreach (string fileName in fileNames)
            {
                if (createPhysicalFile)
                {
                    string fileContent = JavaScriptHandler.ProcessJavascriptCrunch(fileName, false);
                    string filename = string.Format("{0}.js", fileContent.GetHashCode());
                    stringBuilder.AppendFormat(JavaScriptTemplatePhysicalFile, ResourceHelper.CreateResourcePhysicalFile(filename, fileContent));
                    stringBuilder.AppendLine();
                }
                else
                {
                    string javaScriptVersion = string.Empty;
                    if (version)
                    {
                        int versionNumber = JavaScriptHandler.ProcessJavascriptCrunch(fileName, false).GetHashCode();
                        javaScriptVersion = string.Format("&v={0}", versionNumber);
                    }

                    stringBuilder.AppendFormat(JavaScriptTemplate, fileName, string.Empty, javaScriptVersion);
                    stringBuilder.AppendLine();
                }
            }

            return new HtmlString(stringBuilder.ToString());
        }
        #endregion
    }
}
