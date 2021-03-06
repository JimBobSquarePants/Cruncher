﻿// --------------------------------------------------------------------------------------------------------------------
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

    using Cruncher.Extensions;
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
            HttpContext context = HttpContext.Current;

            // Minify on release.
            if (!context.IsDebuggingEnabled)
            {
                string fileContent = AsyncHelper.RunSync(() => CssProcessor.ProcessCssCrunchAsync(context, true, fileNames));
                string fileName = $"{fileContent.ToMd5Fingerprint()}.css";
                return
                    new HtmlString(
                        string.Format(
                            CssPhysicalFileTemplate,
                            AsyncHelper.RunSync(
                                () => ResourceHelper.CreateResourcePhysicalFileAsync(fileName, fileContent)),
                            mediaQuery));
            }

            // Render them separately for debug mode.
            foreach (string name in fileNames)
            {
                string currentName = name;
                string fileContent = AsyncHelper.RunSync(() => CssProcessor.ProcessCssCrunchAsync(context, false, currentName));
                string fileName = $"{Path.GetFileNameWithoutExtension(name)}{fileContent.ToMd5Fingerprint()}.css";
                stringBuilder.AppendFormat(
                    CssPhysicalFileTemplate,
                    AsyncHelper.RunSync(() => ResourceHelper.CreateResourcePhysicalFileAsync(fileName, fileContent)),
                    mediaQuery);
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
            HttpContext context = HttpContext.Current;

            string behaviourParam = behaviour == JavaScriptLoadBehaviour.Inline
                                        ? string.Empty
                                        : behaviour.ToString().ToLowerInvariant();

            // Minify on release.
            if (!context.IsDebuggingEnabled)
            {
                string fileContent = AsyncHelper.RunSync(() => JavaScriptHandler.ProcessJavascriptCrunchAsync(context, true, fileNames));
                string fileName = $"{fileContent.ToMd5Fingerprint()}.js";
                return
                    new HtmlString(
                        string.Format(
                            JavaScriptPhysicalFileTemplate,
                            AsyncHelper.RunSync(
                                () => ResourceHelper.CreateResourcePhysicalFileAsync(fileName, fileContent)),
                            behaviourParam));
            }

            // Render them separately for debug mode.
            foreach (string name in fileNames)
            {
                string currentName = name;
                string fileContent = AsyncHelper.RunSync(() => JavaScriptHandler.ProcessJavascriptCrunchAsync(context, false, currentName));
                string fileName = $"{Path.GetFileNameWithoutExtension(name)}{fileContent.ToMd5Fingerprint()}.js";
                stringBuilder.AppendFormat(
                    JavaScriptPhysicalFileTemplate,
                    AsyncHelper.RunSync(() => ResourceHelper.CreateResourcePhysicalFileAsync(fileName, fileContent)),
                    behaviourParam);
                stringBuilder.AppendLine();
            }

            return new HtmlString(stringBuilder.ToString());
        }
    }
}
