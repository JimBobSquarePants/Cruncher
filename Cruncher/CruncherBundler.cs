// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruncherBundler.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides methods for rendering CSS and JavaScript links onto a webpage.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Web;

    using Cruncher.Helpers;

    /// <summary>
    /// Provides methods for rendering CSS and JavaScript links onto a webpage.
    /// </summary>
    public static class CruncherBundler
    {
        /// <summary>
        /// The template for generating css links pointing to a physical file
        /// </summary>
        private const string CssPhysicalFileTemplate = "<link rel=\"stylesheet\" href=\"{0}\" {1}/>";

        /// <summary>
        /// The template for generating JavaScript links pointing to a physical file
        /// </summary>
        private const string JavaScriptPhysicalFileTemplate = "<script type=\"text/javascript\" src=\"{0}\" {1}></script>";

        /// <summary>
        /// The CSS handler.
        /// </summary>
        private static readonly CssProcessor CssProcessor = new CssProcessor();

        /// <summary>
        /// The JavaScript handler.
        /// </summary>
        private static readonly JavaScriptProcessor JavaScriptHandler = new JavaScriptProcessor();

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
            return RenderCSS(null, fileNames);
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
            StringBuilder stringBuilder = new StringBuilder();

            // Minify on release.
            if (!HttpContext.Current.IsDebuggingEnabled)
            {
                string fileContent = CssProcessor.ProcessCssCrunch(true, fileNames);
                string fileName = string.Format("{0}.css", fileContent.GetHashCode());
                return new HtmlString(string.Format(CssPhysicalFileTemplate, ResourceHelper.CreateResourcePhysicalFile(fileName, fileContent), mediaQuery));
            }

            // Render them separately for debug mode.
            foreach (string name in fileNames)
            {
                string fileContent = CssProcessor.ProcessCssCrunch(false, name);
                string fileName = string.Format("{0}{1}.css", Path.GetFileNameWithoutExtension(name), fileContent.GetHashCode());
                stringBuilder.AppendFormat(CssPhysicalFileTemplate, ResourceHelper.CreateResourcePhysicalFile(fileName, fileContent), mediaQuery);
                stringBuilder.AppendLine();
            }

            return new HtmlString(stringBuilder.ToString());
        }

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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderJavaScript(params string[] fileNames)
        {
            return RenderJavaScript(JavaScriptLoadBehaviour.Inline, fileNames);
        }

        /// <summary>
        /// Renders the correct html to create a script tag linking to the crunched JavaScript representing the given files.
        /// </summary>
        /// <param name="behaviour">
        /// The <see cref="JavaScriptLoadBehaviour"/> describing the way the browser should load the JavaScript into the page.
        /// </param>
        /// <param name="fileNames">
        /// The file names, without the directory path, to link to. These can be .js, and .coffee files or an extension-less token representing an
        /// external file of the given formats as configured in the web.config.
        /// </param>
        /// <returns>
        /// The <see cref="HtmlString"/> containing the script tag with the correct link.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static HtmlString RenderJavaScript(JavaScriptLoadBehaviour behaviour, params string[] fileNames)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string behaviourParam = behaviour == JavaScriptLoadBehaviour.Inline
                                        ? string.Empty
                                        : behaviour.ToString().ToLowerInvariant();

            // Minify on release.
            if (!HttpContext.Current.IsDebuggingEnabled)
            {
                string fileContent = JavaScriptHandler.ProcessJavascriptCrunch(true, fileNames);
                string fileName = string.Format("{0}.js", fileContent.GetHashCode());
                return new HtmlString(string.Format(JavaScriptPhysicalFileTemplate, ResourceHelper.CreateResourcePhysicalFile(fileName, fileContent), behaviourParam));
            }

            // Render them separately for debug mode.
            foreach (string name in fileNames)
            {
                string fileContent = JavaScriptHandler.ProcessJavascriptCrunch(false, name);
                string fileName = string.Format("{0}{1}.js", Path.GetFileNameWithoutExtension(name), fileContent.GetHashCode());
                stringBuilder.AppendFormat(JavaScriptPhysicalFileTemplate, ResourceHelper.CreateResourcePhysicalFile(fileName, fileContent), behaviourParam);
                stringBuilder.AppendLine();
            }

            return new HtmlString(stringBuilder.ToString());
        }
    }
}
